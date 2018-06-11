using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class FileIconCollection
    {
        public static FileIconCollection Current { get; } = new FileIconCollection();

        private struct Key : IEquatable<Key>
        {
            public Key(string filename, FileIconType iconType)
            {
                FileName = filename;
                IconType = iconType;
            }

            public string FileName { get; private set; }
            public FileIconType IconType { get; private set; }

            #region IEquatable Support

            public override bool Equals(object other)
            {
                if (other is Key key)
                {
                    return Equals(key);
                }
                return false;
            }

            public bool Equals(Key other)
            {
                return IconType == other.IconType && FileName == other.FileName;
            }

            public override int GetHashCode()
            {
                return FileName.GetHashCode() ^ IconType.GetHashCode();
            }

            public static bool operator ==(Key lhs, Key rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(Key lhs, Key rhs)
            {
                return !(lhs.Equals(rhs));
            }

            #endregion
        }

        private Dictionary<Key, BitmapSourceCollection> _caches = new Dictionary<Key, BitmapSourceCollection>();


        public void Clear()
        {
            _caches.Clear();
        }

        public BitmapSource CreateDefaultFileIcon(double width)
        {
            return CreateFileIcon("__dummy__", FileIconType.FileType, width);
        }

        public BitmapSource CreateDefaultFolderIcon(double width)
        {
            return CreateFileIcon("__dummy__", FileIconType.DirectoryType, width);
        }

        public BitmapSource CreateFileIcon(string filename, FileIconType iconType, double width)
        {
            var collection = CreateFileIconCollection(filename, iconType);
            return collection.GetBitmapSource(width);
        }

        private  BitmapSourceCollection CreateFileIconCollection(string filename, FileIconType iconType)
        {
            if (iconType == FileIconType.FileType)
            {
                filename = System.IO.Path.GetExtension(filename);
            }

            var key = new Key(filename, iconType);
            if (_caches.TryGetValue(key, out BitmapSourceCollection collection))
            {
                return collection;
            }

            var bitmaps = FileIcon.CreateIconCollection(filename, iconType);
            collection = new BitmapSourceCollection(bitmaps);
            if (iconType == FileIconType.DirectoryType || iconType == FileIconType.FileType)
            {
                _caches.Add(key, collection);
            }
            return collection;
        }

    }

    /// <summary>
    /// 指定サイズにもっとも適したBitmapSourceを返す
    /// アイコン用
    /// </summary>
    public class BitmapSourceCollection
    {
        public BitmapSourceCollection()
        {
        }

        public BitmapSourceCollection(List<BitmapSource> bitmaps)
        {
            if (bitmaps == null) return;

            foreach(var bitmap in bitmaps)
            {
                Add(bitmap);
            }
        }


        public List<BitmapSource> Frames { get; private set; } = new List<BitmapSource>();


        public void Add(BitmapSource source)
        {
            Frames.Add(source);
            Frames.Sort((x, y) => x.PixelWidth - y.PixelWidth);
        }

        public BitmapSource GetBitmapSource()
        {
            return Frames.Last();
        }

        public BitmapSource GetBitmapSource(double width)
        {
            foreach (var frame in Frames)
            {
                if (width <= frame.PixelWidth)
                {
                    return frame;
                }
            }

            return Frames.Last();
        }
    }
}
