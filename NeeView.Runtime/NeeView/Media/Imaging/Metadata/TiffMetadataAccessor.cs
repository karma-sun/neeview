using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public class TiffMetadataAccessor : BasicMetadataAccessor
    {
        public TiffMetadataAccessor(BitmapMetadata meta) : base(meta)
        {
            Debug.Assert(this.Metadata.Format == "tiff");
        }
    }

}
