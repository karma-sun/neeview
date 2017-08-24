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
    }
}
