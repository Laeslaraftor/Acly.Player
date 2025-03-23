using Acly.Player.Properties;
using Acly.Requests;
using Acly.Tokens;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Acly.Player.Server
{
    internal static class ImagesServer
    {
        private const string _ServerAddress = "http://localhost:8159/";

        private static readonly Dictionary<Token, string> _Images = [];
        private static readonly Dictionary<IPlayer, List<Token>> _PlayersTokens = [];
        private static readonly SimpleWebListener _Listener = new(_ServerAddress);
        private static readonly Token _EmptyImageToken = new("empty");

        #region Установка

        public static bool Start()
        {
            if (_Listener.Start())
            {
                _Listener.RequestHandled += OnListenerRequestHandled;
                return true;
            }

            return false;
        }
        public static bool Stop()
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

        public static bool IsAdded(string FilePath)
        {
            return IsAdded(FilePath, out _);
        }
        public static Token AddImage(IPlayer Player, string FilePath)
        {
            if (IsAdded(FilePath, out var FileToken))
            {
                return FileToken;
            }

            FileToken = new();
            _Images.Add(FileToken, FilePath);
            AddPlayerToken(Player, FileToken);

            return FileToken;
        }
        public static bool Remove(Token FileToken)
        {
            bool Result = _Images.Remove(FileToken);

            if (Result && _Images.Count == 0)
            {
                Stop();
            }

            return Result;
        }
        public static Uri GetLink(Token FileToken)
        {
            Uri Result = new($"{_ServerAddress}?t={FileToken}", UriKind.Absolute);

#if DEBUG
            Debug.WriteLine(Result);
#endif

            return Result;
        }
        public static Uri GetEmptyLink() => GetLink(_EmptyImageToken);

        private static bool IsAdded(string FilePath, out Token FileToken)
        {
            FileToken = new();

            foreach (var Info in _Images)
            {
                if (Info.Value == FilePath)
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
            TryCatch(() =>
            {
                Context.Response.Close(Response, false);
            });
        }
        private static void TryCatch(Action DoAction)
        {
            try
            {
                DoAction.Invoke();
            }
            catch (Exception Error)
            {
                StringBuilder Builder = new();
                Builder.AppendLine($"Произошла ошибка {Error.GetType().Name}");
                Builder.AppendLine(Error.Message);
                Builder.AppendLine(Error.StackTrace);

                Debug.WriteLine(Builder.ToString());
            }
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

            if (FileToken != _EmptyImageToken && _Images.TryGetValue(FileToken, out var ImagePath))
            {
                TryCatch(async () =>
                {
                    byte[] Data = await File.ReadAllBytesAsync(ImagePath);
                    Close(Context, Data);
                });
                
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
