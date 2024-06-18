using Android.Media;

namespace Acly.Player.Android
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class MediaData : MediaDataSource
    {
        /// <summary>
        /// Создать новый экземпляр
        /// </summary>
        /// <param name="Data">Массив байтов аудиофайла</param>
        public MediaData(byte[] Data)
        {
            _Data = Data;
        }

        public override long Size => _Data.Length;

        private readonly byte[] _Data;

        public override int ReadAt(long position, byte[]? buffer, int offset, int size)
        {
            if (buffer == null)
            {
                return 0;
            }
            if (position + size > _Data.Length)
            {
                size = _Data.Length - (int)position;
            }

            Array.Copy(_Data, position, buffer, offset, size);

            return size;
        }
        public override void Close()
        {
        }
    }
}
