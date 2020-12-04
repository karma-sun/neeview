using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 指定サイズにもっとも適したBitmapSourceを返す
    /// アイコン用
    /// </summary>
    public class BitmapSourceCollection : IImageSourceCollection
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

        public void Freeze()
        {
            foreach (var frame in Frames)
            {
                frame.Freeze();
            }
        }

        public ImageSource GetImageSource(double width)
        {
            return GetBitmapSource(width);
        }
    }
}
