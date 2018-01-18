// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// BackgroundSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class BackgroundSettingWindow : Window
    {
        /// <summary>
        /// ViewModel
        /// </summary>
        private BackgroundSettingWindowViewModel _vm;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source"></param>
        public BackgroundSettingWindow(BrushSource source)
        {
            InitializeComponent();

            _vm = new BackgroundSettingWindowViewModel(source);
            this.DataContext = _vm;

            this.Closed += BackgroundSettingWindow_Closed;
        }

        /// <summary>
        /// Closed イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundSettingWindow_Closed(object sender, EventArgs e)
        {
            _vm?.Closed();
        }

        /// <summary>
        /// OKButton Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// CanelButton Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// 結果取得
        /// </summary>
        public BrushSource Result => _vm?.Source;
    }

}
