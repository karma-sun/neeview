using NeeView.Susie;
using System;
using System.Diagnostics;
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
            if (!entry.IsIgnoreFileExtension && !PictureProfile.Current.IsSusieSupported(entry.Link ?? entry.EntryName)) return null;

            if (entry.IsFileSystem)
            {
                return Create(entry.Link ?? entry.GetFileSystemPath(), entry);
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
            var rawData = entry.GetRawData();
            if (rawData != null)
            {
                ////Debug.WriteLine($"SusiePictureStream: {entry.EntryLastName} from RawData");
                buff = rawData;
            }
            else
            {
                ////Debug.WriteLine($"SusiePictureStream: {entry.EntryLastName} from Stream");
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    buff = ms.ToArray();
                }
            }

            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var result = accessor.GetPicture(entry.RawEntryName, buff, !entry.IsIgnoreFileExtension);
            if (result == null || result.BitmapData == null)
            {
                throw new SusieIOException();
            }

            return new NamedStream(new MemoryStream(result.BitmapData), result.Plugin.Name, result.BitmapData);
        }


        // Bitmap読み込み(ファイル版)
        private NamedStream Create(string fileName, ArchiveEntry entry)
        {
            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var result = accessor.GetPicture(fileName, null, !entry.IsIgnoreFileExtension);
            if (result == null || result.BitmapData == null)
            {
                throw new SusieIOException();
            }

            return new NamedStream(new MemoryStream(result.BitmapData), result.Plugin.Name, result.BitmapData);
        }
    }


    /// <summary>
    /// Susie 例外
    /// </summary>
    [Serializable]
    public class SusieIOException : Exception
    {
        public SusieIOException() : base(Properties.Resources.SusieLoadFailedException_Message)
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
