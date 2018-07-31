using NeeLaboratory.Windows.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ListBoxのページサムネイルを読み込む機能
    /// </summary>
    public class ListBoxThumbnailLoader
    {
        private IPageListPanel _panel;
        private QueueElementPriority _priority;

        public ListBoxThumbnailLoader(IPageListPanel panelListBox, QueueElementPriority priority)
        {
            _panel = panelListBox;
            _priority = priority;

            _panel.PageCollectionListBox.Loaded += ListBox_Loaded; ;
            _panel.PageCollectionListBox.IsVisibleChanged += ListBox_IsVisibleChanged; ;
            _panel.PageCollectionListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged));
        }

        private void ListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Load();
            }
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        public void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Load();
        }

        public void Load()
        {
            if (!_panel.IsThumbnailVisibled)
            {
                return;
            }

            var virtualizingPanel = VisualTreeUtility.FindVisualChild<VirtualizingPanel>(_panel.PageCollectionListBox);
            if (virtualizingPanel == null)
            {
                return;
            }

            // 有効な ListBoxItem 収集
            var items = _panel.CollectPageList(virtualizingPanel.Children.Cast<ListBoxItem>().Select(i => i.DataContext)).ToList();

            // 未処理の要求を解除
            JobEngine.Current.Clear(_priority);

            // 要求
            foreach (var item in items)
            {
                item.GetPage()?.LoadThumbnail(_priority);
            }
        }
    }
}
