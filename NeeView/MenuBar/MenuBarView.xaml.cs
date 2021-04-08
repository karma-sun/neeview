using NeeView.Windows;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        private MenuBarViewModel _vm;


        public MenuBarView()
        {
            InitializeComponent();

            if (Environment.IsCanaryPackage)
            {
                this.Watermark.Visibility = Visibility.Visible;
                this.Watermark.Background = Brushes.DarkOrange;
                this.WatermarkText.Text = "Canary " + Environment.DateVersion;
            }
            else if (Environment.IsBetaPackage)
            {
                this.Watermark.Visibility = Visibility.Visible;
                this.Watermark.Background = Brushes.Purple;
                this.WatermarkText.Text = "Beta " + Environment.DateVersion;
            }

            this.WindowCaptionButtons.MouseRightButtonUp += (s, e) => e.Handled = true;
            this.MainMenuJoint.MouseRightButtonUp += (s, e) => e.Handled = true;
            this.MouseRightButtonUp += MenuBarView_MouseRightButtonUp;
        }


        public MenuBar Source
        {
            get { return (MenuBar)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MenuBar), typeof(MenuBarView), new PropertyMetadata(null, SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MenuBarView)?.Initialize();
        }


        public void Initialize()
        {
            _vm = new MenuBarViewModel(this.Source, this);
            this.Root.DataContext = _vm;
        }

        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        // システムメニュー表示
        private void MenuBarView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_vm.IsCaptionEnabled)
            {
                WindowTools.ShowSystemMenu(Window.GetWindow(this));
                e.Handled = true;
            }
        }
    }
}
