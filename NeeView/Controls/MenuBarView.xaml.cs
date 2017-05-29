// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Diagnostics;
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
    /// MenuBar : View
    /// </summary>
    public partial class MenuBarView : UserControl
    {
        public MenuBar Source
        {
            get { return (MenuBar)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MenuBar), typeof(MenuBarView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MenuBarView)?.Initialize();
        }

        //
        public MenuBarView()
        {
            InitializeComponent();

#if DEBUG
            this.MenuItemDev.Visibility = Visibility.Visible;
#endif
        }

        private MenuBarViewModel _vm;

        //
        public void Initialize()
        {
            _vm = new MenuBarViewModel(this, this.Source);
            this.Root.DataContext = _vm;
        }


        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }


        #region 開発用

        // [開発用] テストボタン
        private async void MenuItemDevButton_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(1000);
            Debug.WriteLine("TEST");
            Debugger.Break();
            //ModelContext.CommandTable.OpenCommandListHelp();
            //Config.Current .RemoveApplicationData();
        }

        // 開発用コマンド：テンポラリフォルダーを開く
        private void MenuItemDevTempFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Temporary.TempDirectory);
        }

        // 開発用コマンド：アプリケーションフォルダーを開く
        private void MenuItemDevApplicationFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        // 開発用コマンド：アプリケーションデータフォルダーを開く
        private void MenuItemDevApplicationDataFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Config.Current .LocalApplicationDataPath);
        }

        #endregion

    }


}
