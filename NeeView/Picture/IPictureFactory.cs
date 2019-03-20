using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Picture Factory interface.
    /// </summary>
    public interface IPictureFactory
    {
        Picture Create(ArchiveEntry entry, PictureCreateOptions options, CancellationToken token);

        BitmapSource CreateBitmapSource(ArchiveEntry entry, byte[] raw, Size size, bool keepAspectRatio, CancellationToken token);
        byte[] CreateImage(ArchiveEntry entry, byte[] raw, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting, CancellationToken token);
    }
}
