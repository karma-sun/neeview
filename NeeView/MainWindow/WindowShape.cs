using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

namespace NeeView
{
    /// <summary>
    /// WindowStateEx
    /// </summary>
    public enum WindowStateEx
    {
        None, // 未設定
        Normal,
        Minimized,
        Maximized,
        FullScreen,
    }


    [Obsolete]
    public enum WindowChromeFrameV1
    {
        None,
        WindowFrame,
        Line,
    }

    /// <summary>
    /// WindowChromeFrame Type
    /// </summary>
    public enum WindowChromeFrame
    {
        [AliasName("@EnumWindowChromeFrameNone")]
        None,

        [AliasName("@EnumWindowChromeFrameWindowFrame")]
        WindowFrame,
    }

    public interface ITopmostControllable
    {
        bool IsTopmost { get; set; }

        void ToggleTopmost();
    }


    /// <summary>
    /// MainWindowに特化したウィンドウ制御
    /// </summary>
    public class WindowShape : BindableBase, ITopmostControllable
    {
        private Window _window;

        private Thickness _windowBorderThickness;
        private bool _isEnabled;

        private MainWindowChromeAccessor _windowChromeAccessor;

        private WindowStateManager _manager;


        public WindowShape(WindowStateManager manager, MainWindowChromeAccessor windowChromeAccessor)
        {
            _window = MainWindow.Current;

            _windowChromeAccessor = windowChromeAccessor;
            _manager = manager;

            _manager.StateChanged += WindowStateManager_StateChanged;

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.WindowChromeFrame), (s, e) =>
            {
                Refresh();
            });

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsCaptionVisible), (s, e) =>
            {
                Refresh();
            });

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsTopmost), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsTopmost));
            });

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.State), (s, e) =>
            {
                _manager.SetWindowState(Config.Current.Window.State);
            });
        }



        public Thickness WindowBorderThickness
        {
            get { return _windowBorderThickness; }
            set { SetProperty(ref _windowBorderThickness, value); }
        }

        public bool CanCaptionVisible
        {
            get { return Config.Current.Window.IsCaptionVisible && !_manager.IsFullScreen; }
        }

        public bool IsTopmost
        {
            get { return Config.Current.Window.IsTopmost; }
            set { Config.Current.Window.IsTopmost = value; }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    Refresh();
                }
            }
        }


        private void WindowStateManager_StateChanged(object sender, WindowStateChangedEventArgs e)
        {
            Config.Current.Window.State = e.NewState;

            UpdateWindowBorderThickness();

            RaisePropertyChanged(nameof(CanCaptionVisible));
        }

        private void ValidateWindowState()
        {
            if (Config.Current.Window.State != WindowStateEx.None) return;

            switch (_window.WindowState)
            {
                case System.Windows.WindowState.Normal:
                    Config.Current.Window.State = WindowStateEx.Normal;
                    break;
                case System.Windows.WindowState.Minimized:
                    Config.Current.Window.State = WindowStateEx.Minimized;
                    break;
                case System.Windows.WindowState.Maximized:
                    Config.Current.Window.State = WindowStateEx.Maximized;
                    break;
            }
        }

        public void UpdateWindowBorderThickness()
        {
            // NOTE: Windows7 only
            if (!Environment.IsWindows7) return;

            var dipScale = (_window is IHasDpiScale hasDpiScale) ? hasDpiScale.GetDpiScale() : new DpiScale(1.0, 1.0);

            if (_windowChromeAccessor.IsEnabled && _window.WindowState != System.Windows.WindowState.Maximized && Config.Current.Window.WindowChromeFrame == WindowChromeFrame.WindowFrame)
            {
                var x = 1.0 / dipScale.DpiScaleX;
                var y = 1.0 / dipScale.DpiScaleY;
                this.WindowBorderThickness = new Thickness(x, y, x, y);
            }
            else
            {
                this.WindowBorderThickness = default;
            }
        }

        public void ToggleCaptionVisible()
        {
            Config.Current.Window.IsCaptionVisible = !Config.Current.Window.IsCaptionVisible;
        }

        public void ToggleTopmost()
        {
            Config.Current.Window.IsTopmost = !Config.Current.Window.IsTopmost;
        }

        private void UpdateWindowChrome()
        {
            _windowChromeAccessor.IsEnabled = !Config.Current.Window.IsCaptionVisible;
        }

        /// <summary>
        /// 状態を最新にする
        /// </summary>
        public void Refresh()
        {
            if (!this.IsEnabled) return;

            ValidateWindowState();
            UpdateWindowChrome();
            _manager.SetWindowState(Config.Current.Window.State);
            UpdateWindowBorderThickness();
            RaisePropertyChanged(null);
        }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public WindowStateEx State { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsCaptionVisible { get; set; }

            [DataMember]
            public bool IsTopMost { get; set; }

            [DataMember]
            public bool IsFullScreenWithTaskBar { get; set; }

            [DataMember, DefaultValue(8.0)]
            public double MaximizeWindowGapWidth { get; set; }

            [DataMember]
            public WindowStateEx LastState { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public Memento Clone()
            {
                return (Memento)this.MemberwiseClone();
            }

            public void RestoreConfig(Config config)
            {
                config.Window.IsTopmost = IsTopMost;
                config.Window.IsCaptionVisible = IsCaptionVisible;
                config.Window.MaximizeWindowGapWidth = MaximizeWindowGapWidth;
                config.Window.State = State;
                config.Window.LastState = LastState;
            }
        }

        #endregion
    }
}
