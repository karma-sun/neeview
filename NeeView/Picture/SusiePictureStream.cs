using Susie;
using System;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// Susie画像をストリームで取得
    /// </summary>
    class SusiePictureStream : IPictureStream
    {
        // 画像ストリーム取得。
        // 対象に応じてファイルからの読み込みかメモリからの読み込みかを変更
        public NamedStream Create(ArchiveEntry entry)
        {
            if (!PictureProfile.Current.IsSusieSupported(entry.EntryName)) return null;

            if (entry.IsFileSystem)
            {
                return Create(entry.GetFileSystemPath(), entry);
            }
            else
            {
                using (var stream = entry.OpenEntry())
                {
                    return Create(stream, entry);
                }
            }
        }

        // Bitmap読み込み(stream)
        private NamedStream Create(Stream stream, ArchiveEntry entry)
        {
            byte[] buff;
            if (stream is MemoryStream)
            {
                buff = ((MemoryStream)stream).GetBuffer();
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    buff = ms.GetBuffer();
                }
            }

            SusiePlugin susiePlugin = null;

            var bytes = SusieContext.Current.Susie?.GetPicture(entry.RawEntryName, buff, true, out susiePlugin);
            if (bytes == null)
            {
                throw new SusieIOException();
            }

            return new NamedStream(new MemoryStream(bytes), susiePlugin?.ToString());
        }


        // Bitmap読み込み(ファイル版)
        private NamedStream Create(string fileName, ArchiveEntry entry)
        {
            SusiePlugin susiePlugin = null;

            var bytes = SusieContext.Current.Susie?.GetPictureFromFile(fileName, true, out susiePlugin);
            if (bytes == null)
            {
                throw new SusieIOException();
            }

            return new NamedStream(new MemoryStream(bytes), susiePlugin?.ToString());
        }
    }


    /// <summary>
    /// Susie 例外
    /// </summary>
    [Serializable]
    public class SusieIOException : Exception
    {
        public SusieIOException() : base(Properties.Resources.ExceptionSusieLoadFailed)
        {
        }

        public SusieIOException(string message) : base(message)
        {
        }

        public SusieIOException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
