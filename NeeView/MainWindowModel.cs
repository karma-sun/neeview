using Microsoft.Win32;
using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    // パネルカラー
    public enum PanelColor
    {
        Dark,
        Light,
    }

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
        public static MainWindowModel Current { get; private set; }

        #region Fields

        // パネル表示ロック
        private bool _isPanelVisibleLocked;

        // 古いパネル表示ロック。コマンドでロックのトグルをできるようにするため
        private bool _isPanelVisibleLockedOld;


        private PanelColor _panelColor = PanelColor.Dark;
        private ContextMenuSetting _contextMenuSetting = new ContextMenuSetting();
        private bool _isHideMenu;
        private bool _isIsHidePageSlider;
        private bool _isHidePanel; // = true;

        private bool _IsHidePanelInFullscreen = true;
        private bool _IsVisibleWindowTitle = true;
        private bool _isVisibleAddressBar = true;
        private bool _isVisibleBusy = true;

        private DateTime _scrollPageTime;
        private const double _scrollPageMargin = 100.0;

        #endregion

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        public MainWindowModel()
        {
            Current = this;

            // Window Shape
            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.Current.IsFullScreen),
                (s, e) => RefreshCanHidePanel());
        }

        #endregion

        #region Events

        public event EventHandler ThemeColorChanged;
        public event EventHandler CanHidePanelChanged;

        #endregion

        #region Properties

        // 「ブックを開く」ダイアログを現在の場所を基準にして開く
        [PropertyMember("@ParamIsOpenbookAtCurrentPlace")]
        public bool IsOpenbookAtCurrentPlace { get; set; }

        //
        [PropertyMember("@ParamPanelColor")]
        public PanelColor PanelColor
        {
            get { return _panelColor; }
            set
            {
                if (SetProperty(ref _panelColor, value))
                {
                    UpdateThemeColor();
                }
            }
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
                RaisePropertyChanged(nameof(CanHidePageSlider));
            }
        }

        //
        public bool CanHidePageSlider => IsHidePageSlider || WindowShape.Current.IsFullScreen;

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
        private bool _CanHidePanel;
        public bool CanHidePanel
        {
            get { return _CanHidePanel; }
            private set
            {
                if (SetProperty(ref _CanHidePanel, value))
                {
                    CanHidePanelChanged?.Invoke(this, null);
                }
            }
        }

        public void RefreshCanHidePanel()
        {
            CanHidePanel = IsHidePanel || (IsHidePanelInFullscreen && WindowShape.Current.IsFullScreen);
        }

        /// <summary>
        /// IsVisibleWindowTitle property.
        /// タイトルバーが表示されておらず、スライダーにフォーカスがある場合等にキャンバスにタイトルを表示する
        /// </summary>
        [PropertyMember("@ParamIsVisibleWindowTitle")]
        public bool IsVisibleWindowTitle
        {
            get { return _IsVisibleWindowTitle; }
            set { if (_IsVisibleWindowTitle != value) { _IsVisibleWindowTitle = value; RaisePropertyChanged(); } }
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

        /// <summary>
        /// メニューエリアマウスオーバー
        /// Viewから更新される
        /// </summary>
        public bool IsMenuAreaMouseOver { get; set; }

        /// <summary>
        /// ステータスエリアマウスオーバー
        /// Viewから更新される
        /// </summary>
        public bool IsStatusAreaMouseOver { get; set; }

        // メニューエリア、ステータスエリアどちらかの上にマウスがある
        public bool IsFontAreaMouseOver => IsMenuAreaMouseOver || IsStatusAreaMouseOver;

        // 何かキーが押されているか
        public AnyKey AnyKey { get; } = new AnyKey();

        [PropertyMember("@ParamIsAccessKeyEnabled", Tips = "@ParamIsAccessKeyEnabledTips")]
        public bool IsAccessKeyEnabled { get; set; } = true;

        #endregion

        #region Methods

        //
        public void UpdateThemeColor()
        {
            if (App.Current == null) return;

            if (PanelColor == PanelColor.Dark)
            {
                App.Current.Resources["NVBackgroundFade"] = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromRgb(0x28, 0x28, 0x28));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x90, 0xEE, 0x90));
                App.Current.Resources["NVPanelIconBackground"] = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
                App.Current.Resources["NVPanelIconForeground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVFolderPen"] = null;
            }
            else
            {
                App.Current.Resources["NVBackgroundFade"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromRgb(0xF8, 0xF8, 0xF8));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x44, 0xBB, 0x44));
                App.Current.Resources["NVPanelIconBackground"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                App.Current.Resources["NVPanelIconForeground"] = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
                App.Current.Resources["NVFolderPen"] = new Pen(new SolidColorBrush(Color.FromRgb(0xDE, 0xB9, 0x82)), 1);
            }

            ThemeColorChanged?.Invoke(this, null);
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

            var setting = SaveData.Current.UserSetting;

            // 設定反映
            SaveData.Current.RestoreSetting(setting, true);
            SaveData.Current.RestoreSettingCompatible(setting, true);

            // 履歴読み込み
            SaveData.Current.LoadHistory(setting);

            // ブックマーク読み込み
            SaveData.Current.LoadBookmark(setting);

            // ページマーク読込
            SaveData.Current.LoadPagemark(setting);

            // ロード設定破棄
            SaveData.Current.UserSetting = null;


            // ブックを開く
            if (App.Current.Option.IsBlank != SwitchOption.on)
            {
                bool isRefreshFolderList = App.Current.Option.FolderList == null;
                if (App.Current.Option.StartupPlace != null)
                {
                    // 起動引数の場所で開く
                    BookHub.Current.RequestLoad(App.Current.Option.StartupPlace, null, BookLoadOption.None, isRefreshFolderList);
                }
                else
                {
                    // 最後に開いたフォルダーを復元する
                    LoadLastFolder(isRefreshFolderList);
                }
            }

            // 指定されたフォルダーリストの場所を反映
            if (App.Current.Option.FolderList != null)
            {
                Models.Current.FolderList.ResetPlace(App.Current.Option.FolderList);
                Models.Current.SidePanel.IsVisibleFolderList = true;
            }

            // スライドショーの自動再生
            if (App.Current.Option.IsSlideShow != null ? App.Current.Option.IsSlideShow == SwitchOption.on : SlideShow.Current.IsAutoPlaySlideShow)
            {
                SlideShow.Current.IsPlayingSlideShow = true;
            }
        }

        // 最後に開いたフォルダーを開く
        private void LoadLastFolder(bool isRefreshFolderList)
        {
            if (!App.Current.IsOpenLastBook) return;

            string place = BookHistoryCollection.Current.LastAddress;
            if (place != null)
            {
                BookHub.Current.RequestLoad(place, null, BookLoadOption.Resume | BookLoadOption.IsBook, isRefreshFolderList);
            }
        }


        // ダイアログでファイル選択して画像を読み込む
        public void LoadAs()
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = GetDefaultFolder();

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                BookHub.Current.RequestLoad(dialog.FileName, null, BookLoadOption.None, true);
            }
            else
            {
                return;
            }
        }


        // ファイルを開く基準となるフォルダーを取得
        private string GetDefaultFolder()
        {
            // 既に開いている場合、その場所を起点とする
            if (this.IsOpenbookAtCurrentPlace && BookHub.Current.Book != null)
            {
                return System.IO.Path.GetDirectoryName(BookHub.Current.Book.Place);
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
        public void PrevScrollPage()
        {
            var parameter = (ScrollPageCommandParameter)CommandTable.Current[CommandType.PrevScrollPage].Parameter;

            int bookReadDirection = (BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = MouseInput.Current.IsLoupeMode ? false : DragTransformControl.Current.ScrollN(-1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation, parameter.Scroll / 100.0);

            if (!isScrolled)
            {
                var span = DateTime.Now - _scrollPageTime;
                if (!parameter.IsStop || _scrollPageMargin < span.TotalMilliseconds)
                {
                    ContentCanvas.Current.NextViewOrigin = (BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightBottom : DragViewOrigin.LeftBottom;
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
        public void NextScrollPage()
        {
            var parameter = (ScrollPageCommandParameter)CommandTable.Current[CommandType.NextScrollPage].Parameter;

            int bookReadDirection = (BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = MouseInput.Current.IsLoupeMode ? false : DragTransformControl.Current.ScrollN(+1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation, parameter.Scroll / 100.0);

            if (!isScrolled)
            {
                var span = DateTime.Now - _scrollPageTime;
                if (!parameter.IsStop || _scrollPageMargin < span.TotalMilliseconds)
                {
                    ContentCanvas.Current.NextViewOrigin = (BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightTop : DragViewOrigin.LeftTop;
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
            if (Config.Current.IsAppxPackage)
            {
                new MessageDialog(Resources.DialogOpenSettingFolderError, Resources.DialogOpenSettingFolderErrorTitle).ShowDialog();
                return;
            }

            Process.Start("explorer.exe", $"\"{Config.Current.LocalApplicationDataPath}\"");
        }

        // オンラインヘルプ
        public void OpenOnlineHelp()
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }


        // 履歴削除
        // TODO: 直接変更し、最近使ったファイルはイベントで更新すべき
        public void ClearHistory()
        {
            BookHistoryCollection.Current.Clear();
            MenuBar.Current.UpdateLastFiles();
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

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelColor PanelColor { get; set; }
            [DataMember]
            public ContextMenuSetting ContextMenuSetting { get; set; }
            [DataMember]
            public bool IsHideMenu { get; set; }
            [DataMember, DefaultValue(true)]
            public bool IsVisibleAddressBar { get; set; }
            [DataMember]
            public bool IsHidePanel { get; set; }
            [DataMember]
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

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.PanelColor = this.PanelColor;
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

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.PanelColor = memento.PanelColor;
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
        }

        #endregion
    }
}
