// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PageListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListBox : UserControl
    {
        // delete command
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(PageListBox));

        //
        private PageListViewModel _vm;

        //
        private ThumbnailHelper _thumbnailHelper;


        // static constructor
        static PageListBox()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        // constructor
        public PageListBox()
        {
            InitializeComponent();
        }
        
        // constructor
        public PageListBox(PageListViewModel vm) : this()
        {
            _vm = vm;
            this.DataContext = _vm;

            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, Remove_CanExec));

            // タッチスクロール操作の終端挙動抑制
            this.ListBox.ManipulationBoundaryFeedback += SidePanel.Current.ScrollViewer_ManipulationBoundaryFeedback;

            _thumbnailHelper = new ThumbnailHelper(this.ListBox, _vm.RequestThumbnail);
        }


        //
        private void Remove_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            e.CanExecute = item != null && _vm.CanRemove(item) && FileIOProfile.Current.IsEnabled;
        }

        //
        private async void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as Page;
            if (item != null)
            {
                await _vm.Remove(item);
            }
        }


        //
        public void FocusSelectedItem()
        {
            if (this.ListBox.SelectedIndex < 0) return;

            this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);

            if (_vm.Model.FocusAtOnce)
            {
                _vm.Model.FocusAtOnce = false;
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                lbi?.Focus();
            }
        }

        // フォルダーリスト 選択項目変更
        private void PageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.IsLoaded)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }


        // 履歴項目決定
        private void PageListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            if (page != null)
            {
                _vm.Jump(page);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void PageListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            {
                if (e.Key == Key.Return)
                {
                    _vm.Jump(page);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void PageList_KeyDown(object sender, KeyEventArgs e)
        {
            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }

        //
        private async void PaegList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                await Task.Yield();
                FocusSelectedItem();
            }
        }
    }
}
