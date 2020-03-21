using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class ImageConfig : BindableBase
    {
        public ImageStandardConfig Standard { get; set; } = new ImageStandardConfig();

        public ImageSvgConfig Svg { get; set; } = new ImageSvgConfig();
    }

}