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
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// FolderListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListControlView : UserControl
    {
        /// <summary>
        /// Setting property.
        /// </summary>
        public FolderListSetting Setting
        {
            get { return (FolderListSetting)GetValue(SettingProperty); }
            set { SetValue(SettingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Setting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingProperty =
            DependencyProperty.Register("Setting", typeof(FolderListSetting), typeof(FolderListControlView), new PropertyMetadata(new FolderListSetting(), new PropertyChangedCallback(SettingPropertyChanged)));

        //
        public static void SettingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する
            FolderListControlView ctrl = d as FolderListControlView;
            if (ctrl != null)
            {
                ctrl._vm.SetSetting(ctrl.Setting);
            }
        }


        /// <summary>
        /// BookHub property.
        /// </summary>
        public BookHub BookHub
        {
            get { return (BookHub)GetValue(BookHubProperty); }
            set { SetValue(BookHubProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BookHub.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BookHubProperty =
            DependencyProperty.Register("BookHub", typeof(BookHub), typeof(FolderListControlView), new PropertyMetadata(null, new PropertyChangedCallback(BookHubPropertyChanged)));

        //
        public static void BookHubPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する
            FolderListControlView ctrl = d as FolderListControlView;
            if (ctrl != null)
            {
                ctrl._vm.BookHub = ctrl.BookHub;
            }
        }

        /// <summary>
        /// is renaming ?
        /// </summary>
        public bool IsRenaming => _vm.FolderListViewModel != null ? _vm.FolderListViewModel.IsRenaming : false;

        /// <summary>
        /// view model
        /// </summary>
        private FolderListControlViewModel _vm;

        /// <summary>
        /// 応急処置：本来VMが外部から参照できるのはまずい
        /// </summary>
        public FolderListControlViewModel VM => _vm;

        /// <summary>
        /// constructor
        /// </summary>
        public FolderListControlView()
        {
            InitializeComponent();

            _vm = new FolderListControlViewModel();
            this.DockPanel.DataContext = _vm;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="vm"></param>
        public void Initialize(MainWindowVM vm)
        {
            _vm.Initialize(vm);
        }


        /// <summary>
        /// フォルダーリストの場所指定
        /// </summary>
        /// <param name="place"></param>
        /// <param name="select"></param>
        /// <param name="isFocus"></param>
        public void SetPlace(string place, string select, bool isFocus)
        {
            var oprions = (isFocus ? FolderSetPlaceOption.IsFocus : FolderSetPlaceOption.None) | FolderSetPlaceOption.IsUpdateHistory;
            _vm.SetPlace(place, select, oprions);
        }


        /// <summary>
        /// 表示更新イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FolderList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                await Task.Yield();
                _vm.FocusSelectedItem(true);
            }
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
    }
}
