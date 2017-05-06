﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows;
using NeeView.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// BookmarkListView.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkListView : UserControl
    {
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(BookmarkListView));

        private BookmarkListViewModel _vm;

        private ThumbnailHelper _thumbnailHelper;

        //
        public BookmarkListView()
        {
            InitializeComponent();
        }

        //
        public BookmarkListView(BookmarkList model) : this()
        {
            _vm = new BookmarkListViewModel(model);
            _vm.SelectedItemChanging += OnItemsChanging;
            _vm.SelectedItemChanged += OnItemsChanged;
            this.DockPanel.DataContext = _vm;

            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
            this.BookmarkListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));

            _thumbnailHelper = new ThumbnailHelper(this.BookmarkListBox, _vm.RequestThumbnail);
        }


        //
        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as BookMementoUnitNode;
            if (item != null)
            {
                _vm.Remove(item);
            }
        }


        //
        private void OnItemsChanging(object sender, BookmarkListViewModel.SelectedItemChangeEventArgs e)
        {
            var index = this.BookmarkListBox.SelectedIndex;

            ListBoxItem lbi = index >= 0 ? (ListBoxItem)(this.BookmarkListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
            e.IsFocused = lbi != null ? lbi.IsFocused : false;
        }

        //
        private void OnItemsChanged(object sender, BookmarkListViewModel.SelectedItemChangeEventArgs e)
        {
            if (e.IsFocused)
            {
                this.BookmarkListBox.ScrollIntoView(this.BookmarkListBox.SelectedItem);

                var index = this.BookmarkListBox.SelectedIndex;
                var lbi = index >= 0 ? (ListBoxItem)(this.BookmarkListBox.ItemContainerGenerator.ContainerFromIndex(index)) : null;
                lbi?.Focus();
            }
        }




        // 同期
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // nop.                
        }

        // 履歴項目決定
        private void BookmarkListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var historyItem = (sender as ListBoxItem)?.Content as BookMementoUnitNode;
            if (historyItem != null)
            {
                _vm.Load(historyItem.Value.Memento.Place);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void BookmarkListItem_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            var historyItem = (sender as ListBoxItem)?.Content as BookMementoUnitNode;
            {
                if (e.Key == Key.Return)
                {
                    _vm.Load(historyItem.Value.Memento.Place);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void BookmarkList_KeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示時間リセット
            Messenger.Send(this, new MessageEventArgs("ResetHideDelay") { Parameter = new ResetHideDelayParam() { PanelSide = PanelSide.Left } });

            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        private void BookmarkListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e, "BookmarkItem");
        }

        private void BookmarkListBox_Drop(object sender, DragEventArgs e)
        {
            var list = (sender as ListBox).Tag as ObservableCollection<BookMementoUnitNode>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop<BookMementoUnitNode>(sender, e, "BookmarkItem", list);
                e.Handled = true;
            }
        }


        // 表示/非表示イベント
        private void BookmarkListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue as bool? == true)
            {
                if (this.BookmarkListBox.SelectedIndex < 0)
                {
                    this.BookmarkListBox.SelectedIndex = 0;
                }

                FocusSelectedItem();
            }
        }

        //
        public void FocusSelectedItem()
        {
            if (this.BookmarkListBox.SelectedIndex < 0) return;

            this.BookmarkListBox.ScrollIntoView(this.BookmarkListBox.SelectedItem);

            ListBoxItem lbi = (ListBoxItem)(this.BookmarkListBox.ItemContainerGenerator.ContainerFromIndex(this.BookmarkListBox.SelectedIndex));
            lbi?.Focus();
        }
    }
}