using NeeLaboratory.ComponentModel;
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
        [AliasName]
        None, // 未設定

        [AliasName]
        Normal,

        [AliasName]
        Minimized,

        [AliasName]
        Maximized,

        [AliasName]
        FullScreen,
    }

    public static class WindowStateExExtensions
    {
        public static WindowState ToWindowState(this WindowStateEx self)
        {
            switch(self)
            {
                default:
                case WindowStateEx.None:
                case WindowStateEx.Normal:
                    return WindowState.Normal;

                case WindowStateEx.Minimized:
                    return WindowState.Minimized;

                case WindowStateEx.Maximized:
                case WindowStateEx.FullScreen:
                    return WindowState.Maximized;
            }
        }

        public static bool IsFullScreen(this WindowStateEx self)
        {
            return self == WindowStateEx.FullScreen;
        }
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
        [AliasName]
        None,

        [AliasName]
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
        private bool _isEnabled;
        private MainWindowChromeAccessor _windowChromeAccessor;
        private WindowStateManager _manager;
        private bool _autoHideMode;


        public WindowShape(WindowStateManager manager, MainWindowChromeAccessor windowChromeAccessor)
        {
            _window = MainWindow.Current;
            _windowChromeAccessor = windowChromeAccessor;

            _manager = manager;
            _manager.StateChanged += WindowStateManager_StateChanged;

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsTopmost),
                (s, e) => RaisePropertyChanged(nameof(IsTopmost)));

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.State),
                (s, e) => _manager.SetWindowState(Config.Current.Window.State));

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsAutoHideInNormal),
                (s, e) => UpdatePanelHideMode());

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsAutoHidInMaximized),
                (s, e) => UpdatePanelHideMode());

            Config.Current.Window.AddPropertyChanged(nameof(WindowConfig.IsAutoHideInFullScreen),
                (s, e) => UpdatePanelHideMode());
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

        public bool AutoHideMode
        {
            get { return _autoHideMode; }
            set { SetProperty(ref _autoHideMode, value); }
        }


        private void WindowStateManager_StateChanged(object sender, WindowStateChangedEventArgs e)
        {
            Config.Current.Window.State = e.NewState;
            UpdatePanelHideMode();
        }

        public void UpdatePanelHideMode()
        {
            switch (_manager.CurrentState)
            {
                case WindowStateEx.Normal:
                    AutoHideMode = Config.Current.Window.IsAutoHideInNormal;
                    break;
                case WindowStateEx.Maximized:
                    AutoHideMode = Config.Current.Window.IsAutoHidInMaximized;
                    break;
                case WindowStateEx.FullScreen:
                    AutoHideMode = Config.Current.Window.IsAutoHideInFullScreen;
                    break;
                default:
                    AutoHideMode = false;
                    break;
            }
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

        public void ToggleTopmost()
        {
            Config.Current.Window.IsTopmost = !Config.Current.Window.IsTopmost;
        }

        /// <summary>
        /// 状態を最新にする
        /// </summary>
        public void Refresh()
        {
            if (!this.IsEnabled) return;

            ValidateWindowState();
            UpdatePanelHideMode();
            _windowChromeAccessor.IsEnabled = true;
            _manager.SetWindowState(Config.Current.Window.State);
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
                config.Window.MaximizeWindowGapWidth = MaximizeWindowGapWidth;
                config.Window.State = State;
                config.Window.LastState = LastState;
            }
        }

        #endregion
    }
}
