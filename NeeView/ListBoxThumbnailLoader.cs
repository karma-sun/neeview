// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    /// TODO: Behavior化
    /// </summary>
    public class ListBoxThumbnailLoader
    {
        private ListBox _listBox;
        private QueueElementPriority _priority;
        private VirtualizingStackPanel _virtualizingStackPanel;

        public ListBoxThumbnailLoader(ListBox listBox, QueueElementPriority priority)
        {
            _listBox = listBox;
            _priority = priority;

            _listBox.Loaded += ListBox_Loaded; ;
            _listBox.IsVisibleChanged += ListBox_IsVisibleChanged; ;
            _listBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(ListBox_ScrollChanged));
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
            if (_virtualizingStackPanel == null)
            {
                _virtualizingStackPanel = VisualTreeUtility.FindVisualChild<VirtualizingStackPanel>(_listBox);
                if (_virtualizingStackPanel == null)
                {
                    return;
                }
            }

            // 有効な ListBoxItem 収集
            var items = _virtualizingStackPanel.Children.Cast<ListBoxItem>().Select(i => i.DataContext).OfType<IHasPage>().ToList();

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
