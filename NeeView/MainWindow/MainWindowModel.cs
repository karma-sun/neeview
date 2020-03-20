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
            BookHub.Current.RequestLoad(path, null, BookLoadOption.None, true);
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

        #region Fields

        // パネル表示ロック
        private bool _isPanelVisibleLocked;

        // 古いパネル表示ロック。コマンドでロックのトグルをできるようにするため
        private bool _isPanelVisibleLockedOld;


        private ContextMenuSetting _contextMenuSetting = new ContextMenuSetting();
        private bool _isHideMenu;
        private bool _isIsHidePageSlider;
        private bool _canHidePageSlider;
        private bool _isHidePanel; // = true;
        private bool _canHidePanel;

        private bool _IsHidePageSliderInFullscreen = true;
        private bool _IsHidePanelInFullscreen = true;
        private bool _IsVisibleWindowTitle = true;
        private bool _isVisibleAddressBar = true;
        private bool _isVisibleBusy = true;

        private DateTime _scrollPageTime;
        private const double _scrollPageMargin = 100.0;

        private double _sliderOpacity = 1.0;
        private SolidColorBrush _sliderBackground;
        private SolidColorBrush _sliderBackgroundGlass;

        private bool _isCursorHideEnabled = true;
        private double _cursorHideTime = 2.0;

        private volatile EditCommandWindow _editCommandWindow;

        #endregion

        #region Constructors

        private MainWindowModel()
        {
            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.IsFullScreen),
                (s, e) =>
                {
                    RefreshCanHidePanel();
                    RefreshCanHidePageSlider();
                    RaisePropertyChanged(nameof(CanHideMenu));
                });
            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.CanCaptionVisible),
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(CanVisibleWindowTitle));
                });

            ThemeProfile.Current.ThemeColorChanged += (s, e) => RefreshSliderBrushes();

            RefreshCanHidePanel();
            RefreshCanHidePageSlider();

            RefreshSliderBrushes();

            CompositionTarget.Rendering += (s, e) => Rendering?.Invoke(s, e);
        }

        #endregion

        #region Events

        public event EventHandler CanHidePanelChanged;

        public event EventHandler Rendering;

        public event EventHandler FocusMainViewCall;

        #endregion

        #region Properties

        // 「ブックを開く」ダイアログを現在の場所を基準にして開く
        [PropertyMember("@ParamIsOpenbookAtCurrentPlace")]
        public bool IsOpenbookAtCurrentPlace { get; set; }

        // スライダー透明度
        [PropertyPercent("@ParamSliderOpacity", Tips = "@ParamSliderOpacityTips")]
        public double SliderOpacity
        {
            get { return _sliderOpacity; }
            set
            {
                if (SetProperty(ref _sliderOpacity, value))
                {
                    RefreshSliderBrushes();
                }
            }
        }

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


        //
        public ContextMenuSetting ContextMenuSetting
        {
            get { return _contextMenuSetting; }
            set
            {
                _contextMenuSetting = value;
                _contextMenuSetting.Validate();
                RaisePropertyChanged();
            }
        }

        // メニューを自動的に隠す
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set
            {
                _isHideMenu = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHideMenu));
                RaisePropertyChanged(nameof(CanVisibleWindowTitle));
            }
        }

        //
        public bool CanHideMenu => IsHideMenu || WindowShape.Current.IsFullScreen;

        // スライダーを自動的に隠す
        public bool IsHidePageSlider
        {
            get { return _isIsHidePageSlider; }
            set
            {
                _isIsHidePageSlider = value;
                RaisePropertyChanged();
                RefreshCanHidePageSlider();
            }
        }

        /// <summary>
        /// フルスクリーン時にスライダーを隠す
        /// </summary>
        [PropertyMember("@ParamIsHidePageSliderInFullscreen")]
        public bool IsHidePageSliderInFullscreen
        {
            get { return _IsHidePageSliderInFullscreen; }
            set { if (_IsHidePageSliderInFullscreen != value) { _IsHidePageSliderInFullscreen = value; RaisePropertyChanged(); RefreshCanHidePageSlider(); } }
        }

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

        // パネルを自動的に隠す
        public bool IsHidePanel
        {
            get { return _isHidePanel; }
            set
            {
                _isHidePanel = value;
                RaisePropertyChanged();
                RefreshCanHidePanel();
            }
        }

        /// <summary>
        /// フルスクリーン時にパネルを隠す
        /// </summary>
        [PropertyMember("@ParamIsHidePanelInFullscreen")]
        public bool IsHidePanelInFullscreen
        {
            get { return _IsHidePanelInFullscreen; }
            set { if (_IsHidePanelInFullscreen != value) { _IsHidePanelInFullscreen = value; RaisePropertyChanged(); RefreshCanHidePanel(); } }
        }

        // パネルを自動的に隠せるか
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
        /// IsVisibleWindowTitle property.
        /// タイトルバーが表示されておらず、スライダーにフォーカスがある場合等にキャンバスにタイトルを表示する
        /// </summary>
        [PropertyMember("@ParamIsVisibleWindowTitle")]
        public bool IsVisibleWindowTitle
        {
            get { return _IsVisibleWindowTitle; }
            set
            {
                if (SetProperty(ref _IsVisibleWindowTitle, value))
                {
                    RaisePropertyChanged(nameof(CanVisibleWindowTitle));
                }
            }
        }

        public bool CanVisibleWindowTitle
        {
            get => IsVisibleWindowTitle && CanHideMenu && !WindowShape.Current.CanCaptionVisible;
        }


        // アドレスバーON/OFF
        public bool IsVisibleAddressBar
        {
            get { return _isVisibleAddressBar; }
            set { _isVisibleAddressBar = value; RaisePropertyChanged(); }
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
                    SidePanel.Current.IsVisibleLocked = _isPanelVisibleLocked;
                }
            }
        }

        /// <summary>
        /// IsVisibleBusy property.
        /// </summary>
        [PropertyMember("@ParamIsVisibleBusy")]
        public bool IsVisibleBusy
        {
            get { return _isVisibleBusy; }
            set { if (_isVisibleBusy != value) { _isVisibleBusy = value; RaisePropertyChanged(); } }
        }

        [PropertyMember("@ParamIsAccessKeyEnabled", Tips = "@ParamIsAccessKeyEnabledTips")]
        public bool IsAccessKeyEnabled { get; set; } = true;

        /// <summary>
        /// カーソルの自動非表示
        /// </summary>
        [PropertyMember("@ParamIsCursorHideEnabled")]
        public bool IsCursorHideEnabled
        {
            get { return _isCursorHideEnabled; }
            set { SetProperty(ref _isCursorHideEnabled, value); }
        }

        [PropertyRange("@ParameterCursorHideTime", 1.0, 10.0, TickFrequency = 0.2, IsEditable = true)]
        public double CursorHideTime
        {
            get => _cursorHideTime;
            set => SetProperty(ref _cursorHideTime, Math.Max(1.0, value));
        }

        [PropertyMember("@ParameterIsCursorHideReleaseAction")]
        public bool IsCursorHideReleaseAction { get; set; } = true;

        [PropertyRange("@ParameterCursorHideReleaseDistance", 0.0, 1000.0, TickFrequency = 1.0, IsEditable = true)]
        public double CursorHideReleaseDistance { get; set; } = 5.0;

        #endregion

        #region Methods

        private void RefreshSliderBrushes()
        {
            var original = (SolidColorBrush)App.Current.Resources["NVBaseBrush"];
            var glass = CreatePanelBrush(original, _sliderOpacity);

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


        private void RefreshCanHidePageSlider()
        {
            CanHidePageSlider = IsHidePageSlider || (IsHidePageSliderInFullscreen && WindowShape.Current.IsFullScreen);
        }

        public void RefreshCanHidePanel()
        {
            CanHidePanel = IsHidePanel || (IsHidePanelInFullscreen && WindowShape.Current.IsFullScreen);
        }

        //
        public bool ToggleHideMenu()
        {
            IsHideMenu = !IsHideMenu;
            return IsHideMenu;
        }

        //
        public bool ToggleHidePageSlider()
        {
            IsHidePageSlider = !IsHidePageSlider;
            return IsHidePageSlider;
        }

        public bool ToggleHidePanel()
        {
            IsHidePanel = !IsHidePanel;
            return IsHidePanel;
        }

        public bool ToggleVisibleAddressBar()
        {
            IsVisibleAddressBar = !IsVisibleAddressBar;
            return IsVisibleAddressBar;
        }

        // 起動時処理
        public void Loaded()
        {
            // Chrome反映
            WindowShape.Current.WindowChromeFrame = App.Current.WindowChromeFrame;

            // 設定反映
            SaveData.Current.RestoreSetting(SaveData.Current.UserSettingTemp);
            // 保持設定破棄
            SaveData.Current.ReleaseUserSettingTemp();

            // 現在セッションでのファイルの保存場所の確定
            App.Current.UpdateLocation();
            SaveData.Current.UpdateLocation();

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

            // オプション指定があればフォルダーリスト表示
            if (App.Current.Option.FolderList != null)
            {
                SidePanel.Current.IsVisibleFolderList = true;
            }

            // スライドショーの自動再生
            if (App.Current.Option.IsSlideShow != null ? App.Current.Option.IsSlideShow == SwitchOption.on : Config.Current.SlideShow.IsAutoPlaySlideShow)
            {
                SlideShow.Current.IsPlayingSlideShow = true;
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
            BookHub.Current.RequestLoad(path, null, BookLoadOption.None, true);
        }

        // ファイルを開く基準となるフォルダーを取得
        private string GetDefaultFolder()
        {
            // 既に開いている場合、その場所を起点とする
            if (this.IsOpenbookAtCurrentPlace && BookHub.Current.Book != null)
            {
                return System.IO.Path.GetDirectoryName(BookHub.Current.Book.Address);
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// スクロール＋前のページに戻る。
        /// ルーペ使用時はページ移動のみ行う。
        /// </summary>
        public void PrevScrollPage(ScrollPageCommandParameter parameter)
        {
            int bookReadDirection = (BookSettingPresenter.Current.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = MouseInput.Current.IsLoupeMode ? false : DragTransformControl.Current.ScrollN(-1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation, parameter.Scroll / 100.0);

            if (!isScrolled)
            {
                var span = DateTime.Now - _scrollPageTime;
                if (!parameter.IsStop || _scrollPageMargin < span.TotalMilliseconds)
                {
                    ContentCanvas.Current.NextViewOrigin = (BookSettingPresenter.Current.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightBottom : DragViewOrigin.LeftBottom;
                    BookOperation.Current.PrevPage();
                    return;
                }
            }

            _scrollPageTime = DateTime.Now;
        }

        /// <summary>
        /// スクロール＋次のページに進む。
        /// ルーペ使用時はページ移動のみ行う。
        /// </summary>
        public void NextScrollPage(ScrollPageCommandParameter parameter)
        {
            int bookReadDirection = (BookSettingPresenter.Current.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = MouseInput.Current.IsLoupeMode ? false : DragTransformControl.Current.ScrollN(+1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation, parameter.Scroll / 100.0);

            if (!isScrolled)
            {
                var span = DateTime.Now - _scrollPageTime;
                if (!parameter.IsStop || _scrollPageMargin < span.TotalMilliseconds)
                {
                    ContentCanvas.Current.NextViewOrigin = (BookSettingPresenter.Current.LatestSetting.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightTop : DragViewOrigin.LeftTop;
                    BookOperation.Current.NextPage();
                    return;
                }
            }

            _scrollPageTime = DateTime.Now;
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
                    WindowShape.Current.CreateSnapMemento();
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
                new MessageDialog(Resources.DialogOpenSettingFolderError, Resources.DialogOpenSettingFolderErrorTitle).ShowDialog();
                return;
            }

            Process.Start("explorer.exe", $"\"{Environment.LocalApplicationDataPath}\"");
        }

        // スクリプトファイルの場所を開く
        public void OpenScriptsFolder()
        {
            var path = Config.Current.Script.GetCurrentScriptFolder();

            try
            {
                Process.Start("explorer.exe", $"\"{path}\"");
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Resources.DialogOpenScriptsFolderErrorTitle).ShowDialog();
            }
        }

        // オンラインヘルプ
        public void OpenOnlineHelp()
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
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
            if (ContentCanvas.Current.IsMediaContent)
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
                SidePanel.Current.CloseAllPanels();
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

        #endregion

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

            public void RestoreConfig()
            {
                // TODO: ContextMenuSetting
            }

        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.ContextMenuSetting = this.ContextMenuSetting.Clone();

            memento.IsHideMenu = this.IsHideMenu;
            memento.IsHidePageSlider = this.IsHidePageSlider;
            memento.IsVisibleAddressBar = this.IsVisibleAddressBar;
            memento.IsHidePanel = this.IsHidePanel;
            memento.IsHidePanelInFullscreen = this.IsHidePanelInFullscreen;
            memento.IsVisibleWindowTitle = this.IsVisibleWindowTitle;
            memento.IsVisibleBusy = this.IsVisibleBusy;
            memento.IsOpenbookAtCurrentPlace = this.IsOpenbookAtCurrentPlace;
            memento.IsAccessKeyEnabled = this.IsAccessKeyEnabled;
            memento.SliderOpacity = this.SliderOpacity;
            memento.IsHidePageSliderInFullscreen = this.IsHidePageSliderInFullscreen;
            memento.IsCursorHideEnabled = this.IsCursorHideEnabled;
            memento.CursorHideTime = this.CursorHideTime;
            memento.IsCursorHideReleaseAction = this.IsCursorHideReleaseAction;
            memento.CursorHideReleaseDistance = this.CursorHideReleaseDistance;

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.ContextMenuSetting = memento.ContextMenuSetting.Clone();

            this.IsHideMenu = memento.IsHideMenu;
            this.IsHidePageSlider = memento.IsHidePageSlider;
            this.IsHidePanel = memento.IsHidePanel;
            this.IsVisibleAddressBar = memento.IsVisibleAddressBar;
            this.IsHidePanelInFullscreen = memento.IsHidePanelInFullscreen;
            this.IsVisibleWindowTitle = memento.IsVisibleWindowTitle;
            this.IsVisibleBusy = memento.IsVisibleBusy;
            this.IsOpenbookAtCurrentPlace = memento.IsOpenbookAtCurrentPlace;
            this.IsAccessKeyEnabled = memento.IsAccessKeyEnabled;
            this.SliderOpacity = memento.SliderOpacity;
            this.IsHidePageSliderInFullscreen = memento.IsHidePageSliderInFullscreen;
            this.IsCursorHideEnabled = memento.IsCursorHideEnabled;
            this.CursorHideTime = memento.CursorHideTime;
            this.IsCursorHideReleaseAction = memento.IsCursorHideReleaseAction;
            this.CursorHideReleaseDistance = memento.CursorHideReleaseDistance;
        }

        #endregion
    }

}
