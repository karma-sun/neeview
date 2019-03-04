using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// BitmapFactory interface
    /// </summary>
    public interface IBitmapFactory
    {
        BitmapImage Create(Stream stream, BitmapInfo info, Size size, CancellationToken token);
        void CreateImage(Stream stream, BitmapInfo info, Stream outStream, Size size, BitmapImageFormat format, int quality);
    }
}
