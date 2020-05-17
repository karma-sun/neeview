namespace NeeView
{
    public class ConfigMap
    {
        static ConfigMap() => Current = new ConfigMap();
        public static ConfigMap Current { get; }


        public ConfigMap()
        {
            Map = new PropertyMap(NeeView.Config.Current);

            if (Environment.IsZipLikePackage)
            {
                ((PropertyMap)Map[nameof(NeeView.Config.System)]).AddProperty(ExplorerContextMenu.Current, nameof(ExplorerContextMenu.IsEnabled), "IsExplorerContextMenuEnabled");
            }

            //Map.CreateHelpHtml("nv.Config");
        }

        public PropertyMap Map { get; private set; }
    }
}
