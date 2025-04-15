using Acly.Player.Properties;
using Acly.Requests;
using Acly.Tokens;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Acly.Player.Server
{
    /// <summary>
    /// Сервер картинок
    /// </summary>
    public static class ImagesServer
    {
        /// <summary>
        /// Если true то при возникновении исключений, они будут выводится в отладочную консоль
        /// </summary>
        public static bool ExceptionsLog { get; set; }

        private const string _ServerAddress = "http://localhost:8159/";

        private static readonly Dictionary<Token, KeyValuePair<string, byte[]>> _Images = [];
        private static readonly Dictionary<IPlayer, List<Token>> _PlayersTokens = [];
        private static readonly SimpleWebListener _Listener = new(_ServerAddress);
        private static readonly Token _EmptyImageToken = new("empty");

        #region Установка

        internal static bool Start()
        {
            if (_Listener.Start())
            {
                _Listener.RequestHandled += OnListenerRequestHandled;
                return true;
            }

            return false;
        }
        internal static bool Stop()
        {
            if (_Listener.Stop())
            {
                _Listener.RequestHandled -= OnListenerRequestHandled;
                return true;
            }

            return false;
        }

        #endregion

        #region Управление

        internal static bool IsAdded(string FilePath)
        {
            return IsAdded(FilePath, out _);
        }
        internal static Token AddImage(IPlayer Player, string FilePath)
        {
            if (IsAdded(FilePath, out var FileToken))
            {
                return FileToken;
            }

            FileToken = new();
            byte[] ImageData = Resources.DefaultImage;

            if (File.Exists(FilePath))
            {
                try
                {
                    ImageData = File.ReadAllBytes(FilePath);
                }
                catch (Exception Error)
                {
                    PrintException(Error);
                }
            }

            _Images.Add(FileToken, new(FilePath, ImageData));
            AddPlayerToken(Player, FileToken);

            return FileToken;
        }
        internal static bool Remove(Token FileToken)
        {
            bool Result = _Images.Remove(FileToken);

            if (Result && _Images.Count == 0)
            {
                Stop();
            }

            return Result;
        }
        internal static Uri GetLink(Token FileToken)
        {
            Uri Result = new($"{_ServerAddress}?t={FileToken}", UriKind.Absolute);

#if DEBUG
            Debug.WriteLine(Result);
#endif

            return Result;
        }
        internal static Uri GetEmptyLink() => GetLink(_EmptyImageToken);

        private static bool IsAdded(string FilePath, out Token FileToken)
        {
            FileToken = new();

            foreach (var Info in _Images)
            {
                if (Info.Value.Key == FilePath)
                {
                    FileToken = Info.Key;
                    return true;
                }
            }

            return false;
        }
        private static void AddPlayerToken(IPlayer Player, Token Token)
        {
            if (_PlayersTokens.TryGetValue(Player, out var TokensList))
            {
                TokensList.Add(Token);
                return;
            }

            TokensList = [Token];
            _PlayersTokens.Add(Player, TokensList);
            Player.Disposed += OnPlayerDisposed;
        }

        private static void Close(HttpListenerContext Context, byte[] Response)
        {
            try
            {
                Context.Response.Close(Response, false);
            }
            catch (Exception Error)
            {
                PrintException(Error);
            }
        }
        private static void PrintException(Exception Error)
        {
            if (!ExceptionsLog)
            {
                return;
            }

            StringBuilder Builder = new();
            Builder.AppendLine($"Произошла ошибка {Error.GetType().Name}");
            Builder.AppendLine(Error.Message);
            Builder.AppendLine(Error.StackTrace);

            Debug.WriteLine(Builder.ToString());
        }

        #endregion

        #region События

        private static void OnListenerRequestHandled(SimpleWebListener Listener, HttpListenerContext Context)
        {
            var Parts = Context.Request.RawUrl?.Split('=');

            if (Parts == null || Parts.Length == 0)
            {
                Close(Context, Resources.DefaultImage);
                return;
            }

            Token FileToken = new(Parts[^1]);

            if (FileToken != _EmptyImageToken && _Images.TryGetValue(FileToken, out var ImageData))
            {
                Close(Context, ImageData.Value);
                return;
            }

            Close(Context, Resources.DefaultImage);
        }
        private static void OnPlayerDisposed(IPlayer Player)
        {
            Player.Disposed -= OnPlayerDisposed;

            if (_PlayersTokens.TryGetValue(Player, out var TokensList))
            {
                foreach (var Token in TokensList)
                {
                    Remove(Token);
                }
            }
        }

        #endregion
    }
}
