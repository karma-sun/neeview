// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
        private ListBox _ListBox;
        private VirtualizingStackPanel _ListPanel;
        private RequestThumbnailDelegate _RequestThumbnailDelegate;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="listBox">管理するListBox</param>
        /// <param name="func">サムネイル要求リクエストのデリゲート</param>
        public ThumbnailHelper(ListBox listBox, RequestThumbnailDelegate func)
        {
            _ListBox = listBox;

            _ListBox.Loaded += OnLoaded;
            _ListBox.IsVisibleChanged += OnIsVisibleChanged;
            _ListBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChanged));

            _RequestThumbnailDelegate = func;
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
            _ListPanel = NVUtility.FindVisualChild<VirtualizingStackPanel>(_ListBox);
            UpdateThumbnails(1);
        }

        // スクロールしたらサムネ更新
        public void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_ListPanel != null && _ListBox.Items.Count > 0)
            {
                UpdateThumbnails(e.VerticalChange < 0 ? -1 : +1);
            }
        }


        // サムネ更新。表示されているページのサムネの読み込み要求
        public void UpdateThumbnails(int direction)
        {
            if (_ListPanel != null && _ListBox.IsVisible)
            {
                _RequestThumbnailDelegate?.Invoke((int)_ListPanel.VerticalOffset, (int)_ListPanel.ViewportHeight + 1, 1, direction);
            }
        }
    }


}
