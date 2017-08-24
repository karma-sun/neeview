// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PrintWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PrintWindow : Window
    {
        /// <summary>
        /// ViewModel
        /// </summary>
        private PrintWindowViewModel _vm;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public PrintWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public PrintWindow(PrintContext context) : this()
        {
            _vm = new PrintWindowViewModel(context);
            this.DataContext = _vm;

            _vm.Close += ViewModel_Close;

            this.Closed += PrintWindow_Closed;
        }

        /// <summary>
        /// ウィンドウ終了イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrintWindow_Closed(object sender, EventArgs e)
        {
            _vm.Closed();
        }

        /// <summary>
        /// ウィンドウ終了リクエスト処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewModel_Close(object sender, PrintWindowCloseEventArgs e)
        {
            this.DialogResult = e.Result;
            this.Close();
        }
    }
    

    /// <summary>
    /// プレビューページ用コントロール
    /// </summary>
    public class PrintPreviewControl : System.Windows.Controls.Primitives.UniformGrid
    {
        /// <summary>
        /// プレビューデータ
        /// </summary>
        public IEnumerable<FrameworkElement> ItemsSource
        {
            get { return (IEnumerable<FrameworkElement>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<FrameworkElement>), typeof(PrintPreviewControl), new PropertyMetadata(null, ItemsSource_Changed));

        //
        private static void ItemsSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PrintPreviewControl;
            if (control != null)
            {
                control.Reflesh();
            }
        }


        /// <summary>
        /// 更新
        /// </summary>
        public void Reflesh()
        {
            this.Children.Clear();

            if (ItemsSource == null) return;

            foreach (var child in ItemsSource)
            {
                var grid = new Grid();
                grid.Background = Brushes.White;
                grid.Margin = new Thickness(10);
                grid.Children.Add(child);

                this.Children.Add(grid);
            }
        }
    }
}
