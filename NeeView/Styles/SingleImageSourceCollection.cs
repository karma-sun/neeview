using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// イメージソースが１つだけのImageSourceCollection
    /// </summary>
    public class SingleImageSourceCollection : IImageSourceCollection
    {
        private ImageSource _imageSource;

        public SingleImageSourceCollection(ImageSource imageSource)
        {
            _imageSource = imageSource;
        }

        public ImageSource GetImageSource(double width)
        {
            return _imageSource;
        }
    }
}
