using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class FileIconBitmap : BitmapSourceCollection
    {
        public FileIconBitmap()
        {
        }

        public FileIconBitmap(string path)
        {
            Initialize(path);
        }

        public void Initialize(string path)
        {
            Frames.Clear();

            foreach (FileIcon.IconSize iconSize in Enum.GetValues(typeof(FileIcon.IconSize)))
            {
                var bitmap = FileIcon.CreateFileIcon(path, iconSize);
                if (bitmap != null)
                {
                    Add(bitmap);
                }
            }
        }
    }

    public class FileTypeIconBitmap : BitmapSourceCollection
    {
        public FileTypeIconBitmap()
        {
        }

        public FileTypeIconBitmap(string path)
        {
            Initialize(path);
        }

        public void Initialize(string path)
        {
            Frames.Clear();

            foreach (FileIcon.IconSize iconSize in Enum.GetValues(typeof(FileIcon.IconSize)))
            {
                var bitmap = FileIcon.CreateFileTypeIcon(path, iconSize);
                if (bitmap != null)
                {
                    Add(bitmap);
                }
            }
        }
    }

    public class DirectoryIconBitmap : BitmapSourceCollection
    {
        public DirectoryIconBitmap()
        {
        }

        public DirectoryIconBitmap(string path)
        {
            Initialize(path);
        }

        public void Initialize(string path)
        {
            Frames.Clear();

            foreach (FileIcon.IconSize iconSize in Enum.GetValues(typeof(FileIcon.IconSize)))
            {
                var bitmap = FileIcon.CreateDirectoryTypeIcon(path, iconSize);
                if (bitmap != null)
                {
                    Add(bitmap);
                }
            }
        }
    }

    /// <summary>
    /// 指定サイズにもっとも適したBitmapSourceを返す
    /// アイコン用
    /// </summary>
    public class BitmapSourceCollection
    {
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
