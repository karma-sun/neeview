using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// CaptionBar.xaml の相互作用ロジック
    /// </summary>
    public partial class CaptionBar : UserControl
    {
        MainWindowCaptionEmulator _windowCaptionEmulator;


        public CaptionBar()
        {
            InitializeComponent();

            this.Loaded += CaptionBar_Loaded;
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CaptionBar), new PropertyMetadata(""));


        public WindowStateManager WindowStateManager
        {
            get { return (WindowStateManager)GetValue(WindowStateManagerProperty); }
            set { SetValue(WindowStateManagerProperty, value); }
        }

        public static readonly DependencyProperty WindowStateManagerProperty =
            DependencyProperty.Register("WindowStateManager", typeof(WindowStateManager), typeof(CaptionBar), new PropertyMetadata(null, WindowStateManagerPropertyChanged));

        private static void WindowStateManagerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CaptionBar)?.UpdateCaptionEmulator();
        }


        public bool IsMinimizeEnabled
        {
            get { return (bool)GetValue(IsMinimizeEnabledProperty); }
            set { SetValue(IsMinimizeEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMinimizeEnabledProperty =
            DependencyProperty.Register("IsMinimizeEnabled", typeof(bool), typeof(CaptionBar), new PropertyMetadata(true));



        private void CaptionBar_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCaptionEmulator();
        }

        private void UpdateCaptionEmulator()
        {
            var window = Window.GetWindow(this);
            if (window is null) return;

            if (_windowCaptionEmulator is null)
            {
                _windowCaptionEmulator = new MainWindowCaptionEmulator(window, this);
                _windowCaptionEmulator.IsEnabled = true;
            }

            _windowCaptionEmulator.WindowStateManager = this.WindowStateManager;
        }
    }
}
