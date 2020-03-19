namespace NeeView
{
    public class ConfigMap
    {
        public static ConfigMap Current { get; } = new ConfigMap();


        public ConfigMap()
        {
            Map = new PropertyMap(NeeView.Config.Current);
            ((PropertyMap)Map[nameof(NeeView.Config.System)]).AddProperty(ExplorerContextMenu.Current, nameof(ExplorerContextMenu.IsEnabled), "IsExplorerContextMenuEnabled");

            //Map.CreateHelpHtml("nv.Config");
        }

        public PropertyMap Map { get; private set; }
    }
}
