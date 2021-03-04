namespace NeeView.Media.Imaging.Metadata
{
    public class DummyMetadataAccessor : BitmapMetadataAccessor
    {
        public override object GetValue(BitmapMetadataKey key)
        {
            return null;
        }
    }

}
