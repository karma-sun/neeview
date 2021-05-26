namespace NeeView
{
    public class ConfigMap
    {
        public ConfigMap(IAccessDiagnostics accessDiagnostics)
        {
            Map = new PropertyMap(NeeView.Config.Current, accessDiagnostics, "nv.Config");

            // エクスプローラーのコンテキストメニューへの追加フラグ
            if (Environment.IsZipLikePackage)
            {
                ((PropertyMap)Map[nameof(NeeView.Config.System)]).AddProperty(ExplorerContextMenu.Current, nameof(ExplorerContextMenu.IsEnabled), "IsExplorerContextMenuEnabled");
            }
        }

        public PropertyMap Map { get; private set; }
    }
}
