// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    /// リストボックスにサムネイル管理機能を追加する
    /// </summary>
    public class ThumbnailHelper
    {
        // サムネイル要求リクエスト
        public delegate void RequestThumbnailDelegate(int start, int count, int margin, int direction);

        //
        private ListBox _listBox;
        private VirtualizingStackPanel _listPanel;
        private RequestThumbnailDelegate _requestThumbnailDelegate;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="listBox">管理するListBox</param>
        /// <param name="func">サムネイル要求リクエストのデリゲート</param>
        public ThumbnailHelper(ListBox listBox, RequestThumbnailDelegate func)
        {
            _listBox = listBox;

            _listBox.Loaded += OnLoaded;
            _listBox.IsVisibleChanged += OnIsVisibleChanged;
            _listBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChanged));

            _requestThumbnailDelegate = func;
        }

        //
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Debug.WriteLine($"{_ListBox.Name}: {_ListBox.IsVisible}");
            if ((bool)e.NewValue == true)
            {
                UpdateThumbnails(1);
            }
        }

        //
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _listPanel = NVUtility.FindVisualChild<VirtualizingStackPanel>(_listBox);
            UpdateThumbnails(1);
        }

        // スクロールしたらサムネ更新
        public void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_listPanel != null && _listBox.Items.Count > 0)
            {
                UpdateThumbnails(e.VerticalChange < 0 ? -1 : +1);
            }
        }

        // サムネ更新。表示されているページのサムネの読み込み要求
        public void UpdateThumbnails(int direction)
        {
            if (_listPanel == null || !_listBox.IsVisible || _listPanel.Children.Count <= 0) return;

            var scrollUnit = VirtualizingStackPanel.GetScrollUnit(_listBox);

            int start;
            int count;

            if (scrollUnit == ScrollUnit.Item)
            {
                start = (int)_listPanel.VerticalOffset;
                count = (int)_listPanel.ViewportHeight;
            }
            else if (scrollUnit == ScrollUnit.Pixel)
            {
                var itemHeight = (_listPanel.Children[0] as ListBoxItem).ActualHeight;
                if (itemHeight <= 0.0) return; // 項目の準備ができていない？
                start = (int)(_listPanel.VerticalOffset / itemHeight);
                count = (int)(_listPanel.ViewportHeight / itemHeight) + 1;
            }
            else
            {
                return;
            }

            // タイミングにより計算値が不正になることがある対策
            // 再現性が低い
            if (count < 0)
            {
                Debug.WriteLine($"Error Value!: {count}");
                return;
            }

            _requestThumbnailDelegate?.Invoke(start, count, 2, direction);
        }
    }
}
