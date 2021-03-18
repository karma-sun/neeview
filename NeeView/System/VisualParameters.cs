using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class VisualParameters : BindableBase, IDisposable
    {
        static VisualParameters() => Current = new VisualParameters();
        public static VisualParameters Current { get; }


        private bool _isHighContrast = SystemParameters.HighContrast;


        private double _systemFontSize = 12.0;
        private string _defaultFontName;
        private double _defaultFontSize;
        private double _menuFontSize;
        private double _folderTreeFontSize;
        private string _panelFontName;
        private double _panelFontSize;
        private double _fontIconSize;
        private bool _disposedValue;


        private VisualParameters()
        {
            ApplicationDisposer.Current.Add(this);

            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

            Config.Current.Fonts.PropertyChanged +=
                (s, e) => UpdateFonts();

            UpdateFonts();
        }


        public static string SystemFontName { get; private set; } = SystemFonts.MessageFontFamily.Source;
        public static double SystemMessageFontSize { get; private set; } = SystemFonts.MessageFontSize;
        public static double SystemMenuFontSize { get; private set; } = SystemFonts.MenuFontSize;


        public bool IsHighContrast
        {
            get { return _isHighContrast; }
            set { SetProperty(ref _isHighContrast, value); }
        }

        public string DefaultFontName
        {
            get { return _defaultFontName; }
            set
            {
                if (SetProperty(ref _defaultFontName, value))
                {
                    App.Current.Resources["DefaultFontFamily"] = new FontFamily(_defaultFontName ?? "");
                }
            }
        }

        public string PanelFontName
        {
            get { return _panelFontName; }
            set
            {
                if (SetProperty(ref _panelFontName, value))
                {
                    App.Current.Resources["PanelFontFamily"] = new FontFamily(_panelFontName ?? "");
                }
            }
        }

        public double SystemFontSize
        {
            get { return _systemFontSize; }
            set
            {
                if (SetProperty(ref _systemFontSize, value))
                {
                    var limitSize = Math.Max(_systemFontSize, 24.0);
                    App.Current.Resources["SystemFontSize"] = _systemFontSize;
                    App.Current.Resources["SystemFontSizeNormal"] = Math.Min(_systemFontSize * 1.25, limitSize);
                    App.Current.Resources["SystemFontSizeLarge"] = Math.Min(_systemFontSize * 1.5, limitSize);
                    App.Current.Resources["SystemFontSizeHuge"] = Math.Min(_systemFontSize * 2.0, limitSize);
                }
            }
        }

        public double DefaultFontSize
        {
            get { return _defaultFontSize; }
            set
            {
                if (SetProperty(ref _defaultFontSize, value))
                {
                    App.Current.Resources["DefaultFontSize"] = _defaultFontSize;
                }
            }
        }

        public double MenuFontSize
        {
            get { return _menuFontSize; }
            set
            {
                if (SetProperty(ref _menuFontSize, value))
                {
                    App.Current.Resources["MenuFontSize"] = _menuFontSize;
                }
            }
        }
        public double FolderTreeFontSize
        {
            get { return _folderTreeFontSize; }
            set
            {
                if (SetProperty(ref _folderTreeFontSize, value))
                {
                    App.Current.Resources["FolderTreeFontSize"] = _folderTreeFontSize;
                }
            }
        }

        public double PaneFontSize
        {
            get { return _panelFontSize; }
            set
            {
                if (SetProperty(ref _panelFontSize, value))
                {
                    App.Current.Resources["PanelFontSize"] = _panelFontSize;
                }
            }
        }

        public double FontIconSize
        {
            get { return _fontIconSize; }
            set
            {
                if (SetProperty(ref _fontIconSize, value))
                {
                    App.Current.Resources["FontIconSize"] = _fontIconSize;
                }
            }
        }


        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SystemParameters.HighContrast):
                    IsHighContrast = SystemParameters.HighContrast;
                    break;
            }
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Window)
            {
                AppDispatcher.BeginInvoke(UpdateFonts);
            }
        }

        private void UpdateFonts()
        {
            SystemFontName = SystemFonts.MessageFontFamily.Source;
            SystemMessageFontSize = SystemFonts.MessageFontSize;
            SystemMenuFontSize = SystemFonts.MenuFontSize;

            SystemFontSize = SystemMessageFontSize;

            DefaultFontSize = SystemMessageFontSize * Config.Current.Fonts.FontScale;
            MenuFontSize = SystemMenuFontSize * Config.Current.Fonts.MenuFontScale;
            PaneFontSize = SystemMessageFontSize * Config.Current.Fonts.PanelFontScale;
            FolderTreeFontSize = SystemMessageFontSize * Config.Current.Fonts.FolderTreeFontScale;

            FontIconSize = Math.Max(DefaultFontSize + 15, 28.0);

            DefaultFontName = Config.Current.Fonts.FontName;
            PanelFontName = Config.Current.Fonts.PanelFontName;
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
                SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

                _disposedValue = true;
            }
        }

        ~VisualParameters()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
