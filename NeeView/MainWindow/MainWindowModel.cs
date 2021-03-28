using Microsoft.Win32;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using NeeView.Setting;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// Load command.
    /// </summary>
    public class LoadCommand : ICommand
    {
        public static LoadCommand Command { get; } = new LoadCommand();

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return !BookHub.Current.IsLoading;
        }

        public void Execute(object parameter)
        {
            var path = parameter as string;
            if (parameter == null) return;
            BookHub.Current.RequestLoad(this, path, null, BookLoadOption.None, true);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// MainWindow : Model
    /// </summary>
    public class MainWindowModel : BindableBase
    {
        static MainWindowModel() => Current = new MainWindowModel();
        public static MainWindowModel Current { get; }


        // パネル表示ロック
        private bool _isPanelVisibleLocked;

        // 古いパネル表示ロック。コマンドでロックのトグルをできるようにするため
        private bool _isPanelVisibleLockedOld;

        private ContextMenuSetting _contextMenuSetting = new ContextMenuSetting();
        private bool _canHidePageSlider;
        private bool _canHidePanel;
        private bool _canHideMenu;

        private SolidColorBrush _sliderBackground;
        private SolidColorBrush _sliderBackgroundGlass;

        private volatile EditCommandWindow _editCommandWindow;

        private MainViewComponent _viewComponent;

        private WindowShape _windowShape;
        private WindowStateManager _windowStateManamger;



        private MainWindowModel()
        {
        }


        public event EventHandler CanHidePanelChanged;

        public event EventHandler FocusMainViewCall;



        public WindowShape WindowShape => _windowShape;


        // スライダー背景ブラシ
        public SolidColorBrush SliderBackground
        {
            get { return _sliderBackground; }
            private set { SetProperty(ref _sliderBackground, value); }
        }

        // スライダー背景ブラス(常に不透明度適用)
        public SolidColorBrush SliderBackgroundGlass
        {
            get { return _sliderBackgroundGlass; }
            set { SetProperty(ref _sliderBackgroundGlass, value); }
        }

        /// <summary>
        /// メニューを自動非表示するか
        /// </summary>
        public bool CanHideMenu
        {
            get { return _canHideMenu; }
            set { SetProperty(ref _canHideMenu, value); }
        }

        /// <summary>
        /// スライダーを自動非表示するか
        /// </summary>
        public bool CanHidePageSlider
        {
            get { return _canHidePageSlider; }
            set
            {
                if (SetProperty(ref _canHidePageSlider, value))
                {
                    RefreshSliderBrushes();
                }
            }
        }

        /// <summary>
        /// パネルを自動非表示するか
        /// </summary>
        public bool CanHidePanel
        {
            get { return _canHidePanel; }
            private set
            {
                if (SetProperty(ref _canHidePanel, value))
                {
                    CanHidePanelChanged?.Invoke(this, null);
                }
            }
        }


        /// <summary>
        /// パネル表示状態をロックする
        /// </summary>
        public bool IsPanelVisibleLocked
        {
            get { return _isPanelVisibleLocked; }
            set
            {
                if (_isPanelVisibleLocked != value)
                {
                    _isPanelVisibleLocked = value;
                    RaisePropertyChanged();
                    SidePanelFrame.Current.IsVisibleLocked = _isPanelVisibleLocked;
                }
            }
        }



        public void Initialize(WindowShape windowShape, WindowStateManager windowStateManamger)
        {
            _windowShape = windowShape;
            _windowStateManamger = windowStateManamger;

            _windowShape.AddPropertyChanged(nameof(WindowShape.AutoHideMode),
                (s, e) =>
                {
                    RefreshCanHidePanel();
                    RefreshCanHidePageSlider();
                    RefreshCanHideMenu();
                });

            _viewComponent = MainViewComponent.Current;

            ThemeProfile.Current.ThemeColorChanged += (s, e) => RefreshSliderBrushes();

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHideMenuInAutoHideMode), (s, e) =>
            {
                RefreshCanHideMenu();
            });

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHideMenu), (s, e) =>
            {
                RefreshCanHideMenu();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsEnabled), (s, e) =>
            {
                RefreshCanHidePageSlider();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsHidePageSliderInAutoHideMode), (s, e) =>
            {
                RefreshCanHidePageSlider();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsHidePageSlider), (s, e) =>
            {
                RefreshCanHidePageSlider();
            });

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.Opacity), (s, e) =>
            {
                RefreshSliderBrushes();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsHidePanelInAutoHideMode), (s, e) =>
            {
                RefreshCanHidePanel();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.IsHidePanel), (s, e) =>
            {
                RefreshCanHidePanel();
            });

            RefreshCanHidePanel();
            RefreshCanHidePageSlider();

            RefreshSliderBrushes();

            PageViewRecorder.Current.Initialize();
        }


        private void RefreshSliderBrushes()
        {
            var original = (SolidColorBrush)App.Current.Resources["PageSelectionBar.Background"];
            var glass = CreatePanelBrush(original, Config.Current.Slider.Opacity);

            SliderBackground = CanHidePageSlider ? glass : original;
            SliderBackgroundGlass = glass;
        }

        private SolidColorBrush CreatePanelBrush(SolidColorBrush source, double opacity)
        {
            if (opacity < 1.0)
            {
                var color = source.Color;
                color.A = (byte)NeeLaboratory.MathUtility.Clamp((int)(opacity * 0xFF), 0x00, 0xFF);
                return new SolidColorBrush(color);
            }
            else
            {
                return source;
            }
        }

        private void RefreshCanHideMenu()
        {
            CanHideMenu = Config.Current.MenuBar.IsHideMenu || (Config.Current.MenuBar.IsHideMenuInAutoHideMode && _windowShape.AutoHideMode);
        }

        private void RefreshCanHidePageSlider()
        {
            CanHidePageSlider = Config.Current.Slider.IsEnabled && (Config.Current.Slider.IsHidePageSlider || (Config.Current.Slider.IsHidePageSliderInAutoHideMode && _windowShape.AutoHideMode));
        }

        public void RefreshCanHidePanel()
        {
            CanHidePanel = Config.Current.Panels.IsHidePanel || (Config.Current.Panels.IsHidePanelInAutoHideMode && _windowShape.AutoHideMode);
        }

        public bool ToggleHideMenu()
        {
            Config.Current.MenuBar.IsHideMenu = !Config.Current.MenuBar.IsHideMenu;
            return Config.Current.MenuBar.IsHideMenu;
        }

        public bool ToggleHidePageSlider()
        {
            Config.Current.Slider.IsHidePageSlider = !Config.Current.Slider.IsHidePageSlider;
            return Config.Current.Slider.IsHidePageSlider;
        }

        public bool ToggleHidePanel()
        {
            Config.Current.Panels.IsHidePanel = !Config.Current.Panels.IsHidePanel;
            return Config.Current.Panels.IsHidePanel;
        }

        public bool ToggleVisibleAddressBar()
        {
            Config.Current.MenuBar.IsAddressBarEnabled = !Config.Current.MenuBar.IsAddressBarEnabled;
            return Config.Current.MenuBar.IsAddressBarEnabled;
        }

        // 起動時処理
        public void Loaded()
        {
            CustomLayoutPanelManager.Current.Restore();

            // Susie起動
            // TODO: 非同期化できないか？
            SusiePluginManager.Current.Initialize();

            // 履歴読み込み
            SaveData.Current.LoadHistory();

            // ブックマーク読み込み
            SaveData.Current.LoadBookmark();

            // ページマーク読込
            SaveData.Current.LoadPagemark();

            // SaveDataSync活動開始
            SaveDataSync.Current.Initialize();

            // 最初のブック、フォルダを開く
            new FirstLoader().Load();

            // 最初のブックマークを開く
            BookmarkFolderList.Current.UpdateItems();

            // オプション指定があればフォルダーリスト表示
            if (App.Current.Option.FolderList != null)
            {
                SidePanelFrame.Current.IsVisibleFolderList = true;
            }

            // スライドショーの自動再生
            if (App.Current.Option.IsSlideShow != null ? App.Current.Option.IsSlideShow == SwitchOption.on : Config.Current.StartUp.IsAutoPlaySlideShow)
            {
                SlideShow.Current.IsPlayingSlideShow = true;
            }

            // 起動時スクリプトの実行
            if (!string.IsNullOrWhiteSpace(App.Current.Option.ScriptFile))
            {
                CommandTable.Current.ExecuteScript(this, App.Current.Option.ScriptFile);
            }
        }

        public void ContentRendered()
        {
            if (Config.Current.History.IsAutoCleanupEnabled)
            {
                Task.Run(() => BookHistoryCollection.Current.RemoveUnlinkedAsync(CancellationToken.None));
            }
        }

        // ダイアログでファイル選択して画像を読み込む
        public void LoadAs()
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = GetDefaultFolder();

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                LoadAs(dialog.FileName);
            }
        }

        public void LoadAs(string path)
        {
            BookHub.Current.RequestLoad(this, path, null, BookLoadOption.None, true);
        }

        // ファイルを開く基準となるフォルダーを取得
        private string GetDefaultFolder()
        {
            // 既に開いている場合、その場所を起点とする
            if (Config.Current.System.IsOpenbookAtCurrentPlace && BookHub.Current.Book != null)
            {
                return System.IO.Path.GetDirectoryName(BookHub.Current.Book.Address);
            }
            else
            {
                return "";
            }
        }


        // 設定ウィンドウを開く
        public void OpenSettingWindow()
        {
            if (Setting.SettingWindow.Current != null)
            {
                if (Setting.SettingWindow.Current.WindowState == WindowState.Minimized)
                {
                    Setting.SettingWindow.Current.WindowState = WindowState.Normal;
                }
                Setting.SettingWindow.Current.Activate();
                return;
            }

            var dialog = new Setting.SettingWindow(new Setting.SettingWindowModel());
            dialog.Owner = App.Current.MainWindow;
            dialog.Width = MathUtility.Clamp(App.Current.MainWindow.ActualWidth - 100, 640, 1280);
            dialog.Height = MathUtility.Clamp(App.Current.MainWindow.ActualHeight - 100, 480, 2048);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Show();
        }

        // 設定ウィンドウを閉じる
        public bool CloseSettingWindow()
        {
            if (Setting.SettingWindow.Current != null)
            {
                Setting.SettingWindow.Current.AllowSave = false;
                Setting.SettingWindow.Current.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        // コマンド設定を開く
        public void OpenCommandParameterDialog(string command)
        {
            var dialog = new EditCommandWindow();
            dialog.Initialize(command, EditCommandWindowTab.Default);
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            try
            {
                _editCommandWindow = dialog;
                if (_editCommandWindow.ShowDialog() == true)
                {
                    // 設定の同期
                    SaveDataSync.Current.SaveUserSetting(Config.Current.System.IsSyncUserSetting);
                }
            }
            finally
            {
                _editCommandWindow = null;
            }
        }

        public void CloseCommandParameterDialog()
        {
            _editCommandWindow?.Close();
            _editCommandWindow = null;
        }

        // コンソール設定ウィンドウを開く
        public void OpenConsoleWindow()
        {
            if (ConsoleWindow.Current != null)
            {
                ConsoleWindow.Current.Activate();
                return;
            }

            var dialog = new ConsoleWindow();
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Show();
        }

        // バージョン情報を表示する
        public void OpenVersionWindow()
        {
            var dialog = new VersionWindow();
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }


        // 設定ファイルの場所を開く
        public void OpenSettingFilesFolder()
        {
            if (Environment.IsAppxPackage)
            {
                new MessageDialog(Resources.OpenSettingFolderErrorDialog_Message, Resources.OpenSettingFolderErrorDialog_Title).ShowDialog();
                return;
            }

            ExternalProcess.Start("explorer.exe", $"\"{Environment.LocalApplicationDataPath}\"");
        }

        // スクリプトファイルの場所を開く
        public void OpenScriptsFolder()
        {
            var path = Config.Current.Script.GetCurrentScriptFolder();

            try
            {
                ExternalProcess.Start("explorer.exe", $"\"{path}\"", ExternalProcessAtrtibute.ThrowException);
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Resources.OpenScriptsFolderErrorDialog_Title).ShowDialog();
            }
        }

        // オンラインヘルプ
        public void OpenOnlineHelp()
        {
            ExternalProcess.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }


        /// <summary>
        /// パネル表示ロック開始
        /// コマンドから呼ばれる
        /// </summary>
        public void EnterVisibleLocked()
        {
            this.IsPanelVisibleLocked = !_isPanelVisibleLockedOld;
            _isPanelVisibleLockedOld = _isPanelVisibleLocked;
        }

        /// <summary>
        /// パネル表示ロック解除
        /// 他の操作をした場所から呼ばれる
        /// </summary>
        public void LeaveVisibleLocked()
        {
            if (_isPanelVisibleLocked)
            {
                _isPanelVisibleLockedOld = true;
                this.IsPanelVisibleLocked = false;
            }
            else
            {
                _isPanelVisibleLockedOld = false;
            }
        }

        /// <summary>
        /// 現在のスライダー方向を取得
        /// </summary>
        /// <returns></returns>
        public bool IsLeftToRightSlider()
        {
            if (_viewComponent.ContentCanvas.IsMediaContent)
            {
                return true;
            }
            else
            {
                return !PageSlider.Current.IsSliderDirectionReversed;
            }
        }

        /// <summary>
        /// メインビューにフォーカスを移す。コマンド用
        /// </summary>
        public void FocusMainView(FocusMainViewCommandParameter parameter, bool byMenu)
        {
            if (parameter.NeedClosePanels)
            {
                CustomLayoutPanelManager.Current.LeftDock.SelectedItem = null;
                CustomLayoutPanelManager.Current.RightDock.SelectedItem = null;
                ////SidePanel.Current.CloseAllPanels();
            }

            FocusMainViewCall?.Invoke(this, null);
        }

        /// <summary>
        /// メインビューにフォーカスを移す
        /// </summary>
        public void FocusMainView()
        {
            FocusMainViewCall?.Invoke(this, null);
        }


        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public ContextMenuSetting ContextMenuSetting { get; set; }
            [DataMember]
            public bool IsHideMenu { get; set; }
            [DataMember, DefaultValue(true)]
            public bool IsVisibleAddressBar { get; set; }
            [DataMember]
            public bool IsHidePanel { get; set; }
            [DataMember, DefaultValue(true)]
            public bool IsHidePanelInFullscreen { get; set; }
            [DataMember]
            public bool IsHidePageSlider { get; set; }
            [DataMember]
            public bool IsVisibleWindowTitle { get; set; }
            [DataMember, DefaultValue(true)]
            public bool IsVisibleBusy { get; set; }
            [DataMember, DefaultValue(false)]
            public bool IsOpenbookAtCurrentPlace { get; set; }
            [DataMember, DefaultValue(true)]
            public bool IsAccessKeyEnabled { get; set; }
            [DataMember, DefaultValue(1.0)]
            public double SliderOpacity { get; set; }
            [DataMember, DefaultValue(true)]
            public bool IsHidePageSliderInFullscreen { get; set; }
            [DataMember, DefaultValue(true)]
            public bool IsCursorHideEnabled { get; set; }
            [DataMember, DefaultValue(2.0)]
            public double CursorHideTime { get; set; }
            [DataMember, DefaultValue(true)]
            public bool IsCursorHideReleaseAction { get; set; }
            [DataMember, DefaultValue(5.0)]
            public double CursorHideReleaseDistance { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public PanelColor PanelColor { get; set; } // no used v34.0. moved to ThumbnailProfile.


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                // ContextMenuの復元は上位階層で行っている

                config.Slider.Opacity = SliderOpacity;
                config.Slider.IsHidePageSliderInAutoHideMode = IsHidePageSliderInFullscreen;
                config.Panels.IsHidePanelInAutoHideMode = IsHidePanelInFullscreen;
                config.Mouse.IsCursorHideEnabled = IsCursorHideEnabled;
                config.Mouse.CursorHideTime = CursorHideTime;
                config.Mouse.IsCursorHideReleaseAction = IsCursorHideReleaseAction;
                config.Mouse.CursorHideReleaseDistance = CursorHideReleaseDistance;
                config.System.IsOpenbookAtCurrentPlace = IsOpenbookAtCurrentPlace;
                config.Notice.IsBusyMarkEnabled = IsVisibleBusy;
                config.MenuBar.IsHideMenu = IsHideMenu;
                config.Slider.IsHidePageSlider = IsHidePageSlider;
                config.Panels.IsHidePanel = IsHidePanel;
                config.MenuBar.IsAddressBarEnabled = IsVisibleAddressBar;
                config.Command.IsAccessKeyEnabled = IsAccessKeyEnabled;
            }
        }

        #endregion
    }

}
