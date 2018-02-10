using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// 設定ウィンドウのページ
    /// </summary>
    public class SettingPage : BindableBase
    {
        private List<SettingItem> _items;
        private UIElement _content;
        private bool _isSelected;

        public SettingPage(string header)
        {
            this.Header = header;
        }

        public SettingPage(string header, List<SettingItem> items)
            : this(header)
        {
            _items = items;
        }

        public SettingPage(string header, List<SettingItem> items, params SettingPage[] children)
            : this(header, items)
        {
            this.Children = children.Where(e => e != null).ToList();
        }

        /// <summary>
        /// ページ名
        /// </summary>
        public string Header { get; private set; }

        /// <summary>
        /// 子ページ
        /// </summary>
        public List<SettingPage> Children { get; private set; }

        /// <summary>
        /// TreeViewで、このノードが選択されているか
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { if (_isSelected != value) { _isSelected = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// 表示コンテンツ
        /// </summary>
        public UIElement Content
        {
            get { return _content ?? (_content = CreateContent()); }
        }

        /// <summary>
        /// 表示ページ。
        /// コンテンツがない場合、子のページを返す
        /// </summary>
        public SettingPage DispPage
        {
            get { return (_items != null) ? this : this.Children?.FirstOrDefault(); }
        }

        //
        private UIElement CreateContent()
        {
            if (_items == null)
            {
                return null;
            }

            var stackPanel = new StackPanel();

            foreach (var item in _items)
            {
                var itemContent = item.CreateContent();
                if (itemContent != null)
                {
                    stackPanel.Children.Add(itemContent);
                }
            }

            return stackPanel;
        }
    }

}
