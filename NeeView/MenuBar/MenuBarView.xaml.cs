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

        #region Fields

        private MenuBarViewModel _vm;

        #endregion

        #region Constructors

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

        #endregion

        #region DependencyProperties

        public MenuBar Source
        {
            get { return (MenuBar)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MenuBar), typeof(MenuBarView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MenuBarView)?.Initialize();
        }


        /// <summary>
        /// ウィンドウ非アクティブ時に表示を薄くする機能の有効/無効
        /// </summary>
        public bool IsUnactiveEnabled
        {
            get { return (bool)GetValue(IsUnactiveEnabledProperty); }
            set { SetValue(IsUnactiveEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsUnactiveEnabledProperty =
            DependencyProperty.Register("IsUnactiveEnabled", typeof(bool), typeof(MenuBarView), new PropertyMetadata(false));

        #endregion

        #region Methods

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

        #endregion
    }


    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                return isTrue ? 1.0 : 0.5;
            }
            else
            {
                return 0.5f;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
