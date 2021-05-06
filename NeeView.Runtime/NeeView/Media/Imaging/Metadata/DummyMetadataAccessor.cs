namespace NeeView.Media.Imaging.Metadata
{
    public class DummyMetadataAccessor : BitmapMetadataAccessor
    {
        public override string GetFormat()
        {
            return null;
        }

        public override object GetValue(BitmapMetadataKey key)
        {
            return null;
        }
    }

}
