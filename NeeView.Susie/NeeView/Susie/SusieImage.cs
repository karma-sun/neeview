namespace NeeView.Susie
{
    public class SusieImage
    {
        public SusieImage()
        {
        }

        public SusieImage(SusiePluginInfo plugin, byte[] bitmapData)
        {
            Plugin = plugin;
            BitmapData = bitmapData;
        }

        public SusiePluginInfo Plugin { get; set; }
        public byte[] BitmapData { get; set; }
    }
}
