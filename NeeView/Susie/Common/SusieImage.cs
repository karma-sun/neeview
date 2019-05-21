namespace NeeView.Susie
{
    public class SusieImage
    {
        public SusieImage()
        {
        }

        public SusieImage(string pluginName, byte[] bitmapData)
        {
            PluginName = pluginName;
            BitmapData = bitmapData;
        }

        public string PluginName { get; set; }
        public byte[] BitmapData { get; set; }
    }
}
