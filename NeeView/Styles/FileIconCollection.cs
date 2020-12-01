using NeeView.IO;
using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class FileIconCollection
    {
        static FileIconCollection() => Current = new FileIconCollection();
        public static FileIconCollection Current { get; }

        private struct Key : IEquatable<Key>
        {
            public Key(string filename, FileIconType iconType, bool allowJumbo)
            {
                FileName = filename;
                IconType = iconType;
                AllowJumbo = allowJumbo;
            }

            public string FileName { get; private set; }
            public FileIconType IconType { get; private set; }
            public bool AllowJumbo { get; private set; }

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
                return IconType == other.IconType && FileName == other.FileName && AllowJumbo == other.AllowJumbo;
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

 
        public Task InitializeAsync()
        {
            var task = new Task(() =>
            {
                CreateDefaultFolderIcon(256.0);
                // 標準ファイルアイコンは未使用なので、ここでは読み込まない.
                //CreateDefaultFileIcon(256.0);
            });
            task.Start(SingleThreadedApartment.TaskScheduler); // STA
            return task;
        }


        public void Clear()
        {
            _caches.Clear();
        }

        public BitmapSource CreateDefaultFileIcon(double width)
        {
            return CreateFileIcon("a.__dummy__", FileIconType.FileType, width, true, true);
        }

        public BitmapSource CreateDefaultFolderIcon(double width)
        {
            return CreateFileIcon("__dummy__", FileIconType.DirectoryType, width, true, true);
        }

        // TODO: DPI反映のためにBitmapFrameで複数画像を返す
        public BitmapSource CreateFileIcon(string filename, FileIconType iconType, double width, bool allowJumbo, bool useCache)
        {
            var collection = CreateFileIconCollection(filename, iconType, allowJumbo, useCache);
            return collection?.GetBitmapSource(width);
        }


        private object _lock = new object();

        private BitmapSourceCollection CreateFileIconCollection(string filename, FileIconType iconType, bool allowJumbo, bool useCache)
        {
            if (iconType == FileIconType.FileType)
            {
                filename = System.IO.Path.GetExtension(filename);
            }

            var key = new Key(filename, iconType, allowJumbo);
            if (useCache && _caches.TryGetValue(key, out BitmapSourceCollection collection))
            {
                return collection;
            }

            ////var sw = Stopwatch.StartNew();
            try
            {
                var bitmaps = FileIcon.CreateIconCollection(filename, iconType, allowJumbo);
                collection = new BitmapSourceCollection(bitmaps);
                if (useCache && iconType == FileIconType.DirectoryType || iconType == FileIconType.FileType)
                {
                    _caches[key] = collection;
                }
                return collection;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
            finally
            {
                ////sw.Stop();
                ////Debug.WriteLine($"FileIcon: {filename}: {sw.ElapsedMilliseconds}ms");
            }

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

            foreach (var bitmap in bitmaps)
            {
                Add(bitmap);
            }
        }


        public List<BitmapSource> Frames { get; private set; } = new List<BitmapSource>();

        public bool Any()
        {
            return Frames.Any();
        }

        public void Add(BitmapSource source)
        {
            if (source == null) return;

            Frames.Add(source);
            Frames.Sort((x, y) => x.PixelWidth - y.PixelWidth);
        }

        public BitmapSource GetBitmapSource()
        {
            return Frames.LastOrDefault();
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

            return Frames.LastOrDefault();
        }
    }
}
