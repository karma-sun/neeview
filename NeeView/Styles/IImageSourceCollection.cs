using System.Windows.Media;

namespace NeeView
{
    public interface IImageSourceCollection
    {
        ImageSource GetImageSource(double width);
    }
}
