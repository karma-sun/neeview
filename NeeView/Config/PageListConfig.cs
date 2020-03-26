using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class PageListConfig : BindableBase
    {
        private PanelListItemStyle _panelListItemStyle;
        private PageNameFormat _format = PageNameFormat.Smart;

        /// <summary>
        /// ページリストのリスト項目表示形式
        /// </summary>
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { SetProperty(ref _panelListItemStyle, value); }
        }

        /// <summary>
        /// ページ名表示形式
        /// </summary>
        public PageNameFormat Format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }
    }

}



