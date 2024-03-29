﻿using NeeView.Native;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        /// <summary>
        /// このウィンドウが存在する間だけ設定されるインスタンス
        /// </summary>
        public static SettingWindow Current { get; private set; }

        public SettingWindowViewModel _vm;

        public SettingWindow()
        {
            Interop.NVFpReset();

            InitializeComponent();
        }

        public SettingWindow(SettingWindowModel model)
        {
            InitializeComponent();

            Current = this;

            DragDropHelper.AttachDragOverTerminator(this);

            this.Closing += SettingWindow_Closing;
            this.Closed += (s, e) => Current = null;
            this.KeyDown += SettingWindow_KeyDown;

            _vm = new SettingWindowViewModel(model);
            this.DataContext = _vm;

            _vm.AddPropertyChanged(nameof(SettingWindowViewModel.CurrentPage), UpdateIndexTreeSelected);
        }


        /// <summary>
        /// 設定画面を閉じる時にデータ保存するフラグ
        /// </summary>
        public bool AllowSave { get; set; } = true;

        /// <summary>
        /// ファイル保存せずに終了
        /// </summary>
        public void Cancel()
        {
            AllowSave = false;
            Close();
        }


        private void SettingWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void SettingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 設定を閉じるとメインウィンドウが背後に隠れてしまう現象を抑制
            MainWindow.Current?.Activate();
        }

        private void SettingWindow_Closed(object sender, EventArgs e)
        {
            if (this.AllowSave)
            {
                SaveDataSync.Current.SaveUserSetting(Config.Current.System.IsSyncUserSetting);
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.PageContent.Focus();
        }

        private void IndexTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var settingPage = this.IndexTree.SelectedItem as SettingPage;
            _vm.SelectedItemChanged(settingPage);
        }

        private void UpdateIndexTreeSelected(object sender, PropertyChangedEventArgs e)
        {
            var settingPage = this.IndexTree.SelectedItem as SettingPage;

            if (_vm.IsSearchPageSelected && settingPage != null)
            {
                settingPage.IsSelected = false;
            }
        }
    }

    public class BooleanToSwitchStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Properties.Resources.Word_On : Properties.Resources.Word_Off;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
