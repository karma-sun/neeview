// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
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
using System.Diagnostics;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// FolderListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListView : UserControl
    {
        /// <summary>
        /// requested focus serch box
        /// </summary>
        private volatile bool _requestSearchBoxFocus;

        /// <summary>
        /// is renaming ?
        /// </summary>
        public bool IsRenaming => _vm.IsRenaming;

        /// <summary>
        /// view model
        /// </summary>
        private FolderListViewModel _vm;

        //
        public FolderListView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// constructor
        /// </summary>
        public FolderListView(FolderList model) : this()
        {
            _vm = new FolderListViewModel(model);
            this.DockPanel.DataContext = _vm;

            model.SearchBoxFocus += FolderList_SearchBoxFocus;
        }

        /// <summary>
        /// 検索ボックスのフォーカス要求処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_SearchBoxFocus(object sender, EventArgs e)
        {
            if (!_vm.Model.IsFolderSearchBoxVisible) return;

            if (!_requestSearchBoxFocus)
            {
                _requestSearchBoxFocus = true;
                var task = FocustSearchBoxAsync(); // 非同期
            }
        }

        /// <summary>
        /// 検索ボックスにフォーカスをあわせる。
        /// </summary>
        /// <returns></returns>
        private async Task FocustSearchBoxAsync()
        {
            // 表示が間に合わない場合があるので繰り返しトライする
            while (_requestSearchBoxFocus && _vm.Model.IsFolderSearchBoxVisible)
            {
                var textBox = (this.SearchBox.Template.FindName("PART_EditableTextBox", this.SearchBox) as TextBox);
                if (textBox != null && textBox.IsLoaded && textBox.IsVisible)
                {
                    var focused = textBox.Focus();
                    ////Debug.WriteLine($"Focus: {focused}");
                    if (focused) break;
                }

                ////Debug.WriteLine($"Focus: ready...");
                await Task.Delay(100);
            }

            _requestSearchBoxFocus = false;
            ////Debug.WriteLine($"Focus: done.");
        }


        
        /// <summary>
        /// 履歴戻るボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderPrevButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(-1, 10);
        }

        /// <summary>
        /// 履歴進むボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderNextButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(+1, 10);
        }

        //
        private void FolderListView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.IsVisibleChanged((bool)e.NewValue);
        }

        /// <summary>
        /// 単キーのショートカット無効
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        /// <summary>
        /// 検索ボックスでのキー入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;

            if (e.Key == Key.Enter)
            {
                await _vm.SearchAsync();
            }
        }

    }
}
