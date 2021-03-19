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
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

            [DllImport("user32.dll")]
            public static extern uint TrackPopupMenuEx(IntPtr hMenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

            public const uint TPM_CENTERALIGN = 0x0004;
            public const uint TPM_LEFTALIGN = 0x0000;
            public const uint TPM_RIGHTALIGN = 0x0008;
            public const uint TPM_BOTTOMALIGN = 0x0020;
            public const uint TPM_TOPALIGN = 0x0008;
            public const uint TPM_VCENTERALIGN = 0x0010;
            public const uint TPM_NONOTIFY = 0x0080;
            public const uint TPM_RETURNCMD = 0x0100;
            public const uint TPM_LEFTBUTTON = 0x0000;
            public const uint TPM_RIGHTBUTTON = 0x0002;

            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            public const uint WM_SYSCOMMAND = 0x0112;


            public static void ShowSystemMenu(Window window)
            {
                if (window is null) return;

                var hWnd = (new WindowInteropHelper(window)).Handle;
                if (hWnd == IntPtr.Zero) return;

                var hMenu = GetSystemMenu(hWnd, false);
                if (hMenu == IntPtr.Zero) return;

                var screenPos = window.PointToScreen(Mouse.GetPosition(window));
                uint command = TrackPopupMenuEx(hMenu, TPM_LEFTBUTTON | TPM_RETURNCMD, (int)screenPos.X, (int)screenPos.Y, hWnd, IntPtr.Zero);
                if (command == 0) return;

                PostMessage(hWnd, WM_SYSCOMMAND, new IntPtr(command), IntPtr.Zero);
            }
        }


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
                NativeMethods.ShowSystemMenu(Window.GetWindow(this));
                e.Handled = true;
            }
        }
    }


    public class BooleanToOpacityConverter : IValueConverter
    {
        public double TrueOpacity { get; set; } = 1.0;
        public double FalseOpacity { get; set; } = 0.5;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                return isTrue ? TrueOpacity : FalseOpacity;
            }
            else
            {
                return FalseOpacity;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BrushAddOpacityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is SolidColorBrush brush && values[1] is double opacity)
            {
                ////Debug.WriteLine($"{brush.Color},{opacity}");
                return new SolidColorBrush(brush.Color) { Opacity = opacity };
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    // from https://thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/
    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}
