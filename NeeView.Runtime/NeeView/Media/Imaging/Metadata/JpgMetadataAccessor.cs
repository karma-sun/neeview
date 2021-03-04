using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public class JpgMetadataAccessor : BasicMetadataAccessor
    {
        public JpgMetadataAccessor(BitmapMetadata meta) : base(meta)
        {
            Debug.Assert(this.Metadata.Format == "jpg");
        }


        public override object GetValue(BitmapMetadataKey key)
        {
            switch (key)
            {
                case BitmapMetadataKey.Comments:
                    return this.Metadata.Comment ?? this.Metadata.GetQuery("/com/TextEntry");
                default:
                    return base.GetValue(key);
            }
        }
    }

}
