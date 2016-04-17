// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    // 通知表示の種類
    public enum ShowMessageStyle
    {
        None,
        Normal,
        Tiny,
    }

    // ViewChangedイベント引数
    public class ViewChangeArgs
    {
        public int PageDirection { get; set; }
        public bool ResetViewTransform { get; set; }
    }


    // パネルカラー
    public enum PanelColor
    {
        Dark,
        Light,
    }

    // パネル種類
    public enum PanelType
    {
        None,
        FileInfo,
        FolderList,
        HistoryList,
    }


    /// <summary>
    /// ViewModel
    /// </summary>
    public class MainWindowVM : INotifyPropertyChanged, IDisposable
    {
        #region Events

        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        // ロード中通知
        public event EventHandler<string> Loading;

        // 表示変更を通知
        public event EventHandler<ViewChangeArgs> ViewChanged;

        // ショートカット変更を通知
        public event EventHandler InputGestureChanged;

        // ウィンドウモード変更通知
        public event EventHandler NotifyMenuVisibilityChanged;

        #endregion


        // 移動制限モード
        public bool IsLimitMove { get; set; }

        // 回転、拡縮をコンテンツの中心基準にする
        public bool IsControlCenterImage { get; set; }

        // 回転スナップ
        public bool IsAngleSnap { get; set; }

        // 表示開始時の基準
        public bool IsViewStartPositionCenter { get; set; }

        // 通知表示スタイル
        public ShowMessageStyle NoticeShowMessageStyle { get; set; }

        // コマンド表示スタイル
        public ShowMessageStyle CommandShowMessageStyle { get; set; }

        // ゼスチャ表示スタイル
        public ShowMessageStyle GestureShowMessageStyle { get; set; }

        // NowLoading表示スタイル
        public ShowMessageStyle NowLoadingShowMessageStyle { get; set; }

        // スライダー方向
        #region Property: IsSliderDirectionReversed
        private bool _IsSliderDirectionReversed;
        public bool IsSliderDirectionReversed
        {
            get { return _IsSliderDirectionReversed; }
            set
            {
                if (_IsSliderDirectionReversed != value)
                {
                    _IsSliderDirectionReversed = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        // スケールモード
        #region Property: StretchMode
        private PageStretchMode _StretchMode = PageStretchMode.Uniform;
        public PageStretchMode StretchMode
        {
            get { return _StretchMode; }
            set
            {
                if (_StretchMode != value)
                {
                    _StretchMode = value;
                    OnPropertyChanged();
                    UpdateContentSize();
                    ViewChanged?.Invoke(this, new ViewChangeArgs() { ResetViewTransform = true });
                }
            }
        }
        #endregion

        // 背景スタイル
        #region Property: Background
        private BackgroundStyle _Background;
        public BackgroundStyle Background
        {
            get { return _Background; }
            set { _Background = value; UpdateBackgroundBrush(); OnPropertyChanged(); }
        }
        #endregion

        // ドットのまま拡大
        #region Property: IsEnabledNearestNeighbor
        private bool _IsEnabledNearestNeighbor;
        public bool IsEnabledNearestNeighbor
        {
            get { return _IsEnabledNearestNeighbor; }
            set
            {
                if (_IsEnabledNearestNeighbor != value)
                {
                    _IsEnabledNearestNeighbor = value;
                    OnPropertyChanged();
                    UpdateContentScalingMode();
                }
            }
        }
        #endregion

        // 拡大率キープ
        public bool IsKeepScale { get; set; }

        // 回転キープ
        public bool IsKeepAngle { get; set; }

        // 反転キープ
        public bool IsKeepFlip { get; set; }

        // メニューを自動的に隠す
        #region Property: IsHideMenu
        private bool _IsHideMenu;
        public bool IsHideMenu
        {
            get { return _IsHideMenu; }
            set { _IsHideMenu = value; OnPropertyChanged(); NotifyMenuVisibilityChanged?.Invoke(this, null); }
        }
        public bool ToggleHideMenu()
        {
            IsHideMenu = !IsHideMenu;
            return IsHideMenu;
        }
        #endregion

        // タイトルバーを消す
        #region Property: IsHideTitleBar
        private bool _IsHideTitleBar;
        public bool IsHideTitleBar
        {
            get { return _IsHideTitleBar; }
            set { _IsHideTitleBar = value; OnPropertyChanged(); NotifyMenuVisibilityChanged?.Invoke(this, null); }
        }
        public bool ToggleHideTitleBar()
        {
            IsHideTitleBar = !IsHideTitleBar;
            return IsHideTitleBar;
        }
        #endregion


        // ファイル情報表示ON/OFF
        public bool IsVisibleFileInfo
        {
            get { return RightPanel == PanelType.FileInfo; }
            set { RightPanel = value ? PanelType.FileInfo : PanelType.None; }
        }

        public bool ToggleVisibleFileInfo()
        {
            IsVisibleFileInfo = !IsVisibleFileInfo;
            return IsVisibleFileInfo;
        }


        // フォルダーリスト表示ON/OFF
        public bool IsVisibleFolderList
        {
            get { return LeftPanel == PanelType.FolderList; }
            set { LeftPanel = value ? PanelType.FolderList : PanelType.None; }
        }

        //
        public bool ToggleVisibleFolderList()
        {
            IsVisibleFolderList = !IsVisibleFolderList;
            return IsVisibleFolderList;
        }


        // 履歴リスト表示ON/OFF
        public bool IsVisibleHistoryList
        {
            get { return LeftPanel == PanelType.HistoryList; }
            set { LeftPanel = value ? PanelType.HistoryList : PanelType.None; }
        }

        //
        public bool ToggleVisibleHistoryList()
        {
            IsVisibleHistoryList = !IsVisibleHistoryList;
            return IsVisibleHistoryList;
        }


        // 左パネル
        #region Property: LeftPanel
        private PanelType _LeftPanel;
        public PanelType LeftPanel
        {
            get { return _LeftPanel; }
            set
            {
                _LeftPanel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsVisibleFolderList));
                OnPropertyChanged(nameof(IsVisibleHistoryList));
                NotifyMenuVisibilityChanged?.Invoke(this, null);
            }
        }
        #endregion

        // 右パネル
        #region Property: RightPanel
        private PanelType _RightPanel;
        public PanelType RightPanel
        {
            get { return _RightPanel; }
            set
            {
                _RightPanel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsVisibleFileInfo));
                UpdateFileInfoContent();
                NotifyMenuVisibilityChanged?.Invoke(this, null);
            }
            #endregion
        }

        // パネル幅
        public double LeftPanelWidth { get; set; } = 250;
        public double RightPanelWidth { get; set; } = 250;

        // フルスクリーン
        #region Property: IsFullScreen
        private bool _IsFullScreen;
        public bool IsFullScreen
        {
            get { return _IsFullScreen; }
            set { _IsFullScreen = value; OnPropertyChanged(); NotifyMenuVisibilityChanged?.Invoke(this, null); }
        }
        public bool ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
            return IsFullScreen;
        }
        public bool IsSaveFullScreen { get; set; }
        #endregion

        // 常に手前に表示
        #region Property: IsTopmost
        private bool _IsTopmost;
        public bool IsTopmost
        {
            get { return _IsTopmost; }
            set { _IsTopmost = value; OnPropertyChanged(); }
        }
        public bool ToggleTopmost()
        {
            IsTopmost = !IsTopmost;
            return IsTopmost;
        }
        #endregion

        // 最後のフォルダを開く
        public bool IsLoadLastFolder { get; set; }

        // マルチブートを禁止する
        public bool IsDisableMultiBoot { get; set; }

        // スライドショーの自動開始
        public bool IsAutoPlaySlideShow { get; set; }


        // コマンドバインド用
        // View側で定義されます
        public Dictionary<CommandType, RoutedUICommand> BookCommands { get; set; }

        // 空フォルダ通知表示のON/OFF
        #region Property: IsVisibleEmptyPageMessage
        private bool _IsVisibleEmptyPageMessage = false;
        public bool IsVisibleEmptyPageMessage
        {
            get { return _IsVisibleEmptyPageMessage; }
            set { if (_IsVisibleEmptyPageMessage != value) { _IsVisibleEmptyPageMessage = value; OnPropertyChanged(); } }
        }
        #endregion

        // 空フォルダ通知表示の詳細テキスト
        #region Property: EmptyPageMessage
        private string _EmptyPageMessage;
        public string EmptyPageMessage
        {
            get { return _EmptyPageMessage; }
            set { _EmptyPageMessage = value; OnPropertyChanged(); }
        }
        #endregion

        // 現在ページ番号
        public int Index
        {
            get { return BookHub.GetPageIndex(); }
            set { BookHub.SetPageIndex(value); }
        }

        // 最大ページ番号
        public int IndexMax
        {
            get { return BookHub.GetPageCount(); }
        }

        #region Window Icon

        // ウィンドウアイコン：標準
        private ImageSource _WindowIconDefault;

        // ウィンドウアイコン：スライドショー再生中
        private ImageSource _WindowIconPlay;

        // ウィンドウアイコン初期化
        private void InitializeWindowIcons()
        {
            _WindowIconDefault = null;
            _WindowIconPlay = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Play.ico", UriKind.RelativeOrAbsolute));
        }

        // 現在のウィンドウアイコン取得
        public ImageSource WindowIcon
        {
            get
            {
                return BookHub.IsEnableSlideShow ? _WindowIconPlay : _WindowIconDefault;
            }
        }

        #endregion

        #region Window Title

        // ウィンドウタイトル
        public string WindowTitle
        {
            get
            {
                if (LoadingPath != null)
                    return LoosePath.GetFileName(LoadingPath) + " (読込中)";

                if (BookHub.Current?.Place == null)
                    return _DefaultWindowTitle;

                string text = NVUtility.PlaceToTitle(BookHub.Current.Place);

                if (MainContent != null)
                {
                    if (MainContent.PartSize == 2)
                    {
                        text += $" ({MainContent.Position.Index + 1}/{IndexMax + 1})";
                    }
                    else
                    {
                        string pageNum = (MainContent.Position.Index + 1).ToString() + (MainContent.Position.Part == 1 ? ".5" : ".0");
                        text += $" ({pageNum}/{IndexMax + 1})";
                    }

                    if (Contents[1].IsValid)
                    {
                        string name1 = Contents[1].FullPath?.TrimEnd('\\').Replace('/', '\\').Replace("\\", " > ") + Contents[1].GetPartString();
                        string name0 = LoosePath.GetFileName(Contents[0].FullPath) + Contents[0].GetPartString();
                        text += $" - {name1} | {name0}";
                    }
                    else if (Contents[0].IsValid)
                    {
                        string name = Contents[0].FullPath?.TrimEnd('\\').Replace('/', '\\').Replace("\\", " > ") + Contents[0].GetPartString();
                        text += $" - {name}";
                    }
                }

                return text;
            }
        }

        // ロード中パス
        private string _LoadingPath;
        public string LoadingPath
        {
            get { return _LoadingPath; }
            set { _LoadingPath = value; OnPropertyChanged("WindowTitle"); }
        }

        #endregion

        // 通知テキスト(標準)
        #region Property: InfoText
        private string _InfoText;
        public string InfoText
        {
            get { return _InfoText; }
            set { _InfoText = value; OnPropertyChanged(); }
        }

        // 通知テキストフォントサイズ
        public double InfoTextFontSize { get; set; } = 24.0;

        #endregion

        // 通知テキスト(控えめ)
        #region Property: TinyInfoText
        private string _TinyInfoText;
        public string TinyInfoText
        {
            get { return _TinyInfoText; }
            set { _TinyInfoText = value; OnPropertyChanged(); }
        }
        #endregion

        // 本設定 公開
        public Book.Memento BookSetting => BookHub.BookMemento;

        // 最近使ったフォルダ
        #region Property: LastFiles
        private List<Book.Memento> _LastFiles = new List<Book.Memento>();
        public List<Book.Memento> LastFiles
        {
            get { return _LastFiles; }
            set { _LastFiles = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsEnableLastFiles)); }
        }
        #endregion

        // 最近使ったフォルダの有効フラグ
        public bool IsEnableLastFiles { get { return LastFiles.Count > 0; } }

        // コンテンツ
        public ObservableCollection<ViewContent> Contents { get; private set; }

        // コンテンツマージン
        #region Property: ContentsMargin
        private Thickness _ContentsMargin;
        public Thickness ContentsMargin
        {
            get { return _ContentsMargin; }
            set { _ContentsMargin = value; OnPropertyChanged(); }
        }
        #endregion


        // 見開き時のメインとなるコンテンツ
        #region Property: MainContent
        private ViewContent _MainContent;
        public ViewContent MainContent
        {
            get { return _MainContent; }
            set
            {
                _MainContent = value;
                OnPropertyChanged();
                UpdateFileInfoContent();
            }
        }
        #endregion

        private void UpdateFileInfoContent()
        {
            FileInfoContent = IsVisibleFileInfo ? _MainContent : null;
        }

        #region Property: FileInfoContent
        private ViewContent _FileInfoContent;
        public ViewContent FileInfoContent
        {
            get { return _FileInfoContent; }
            set
            {
                if (_FileInfoContent != value)
                {
                    _FileInfoContent = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Property: FileInfoSetting
        private FileInfoSetting _FileInfoSetting;
        public FileInfoSetting FileInfoSetting
        {
            get { return _FileInfoSetting; }
            set { _FileInfoSetting = value; OnPropertyChanged(); }
        }
        #endregion

        #region Property: FolderListSetting
        private FolderListSetting _FolderListSetting;
        public FolderListSetting FolderListSetting
        {
            get { return _FolderListSetting; }
            set { _FolderListSetting = value; OnPropertyChanged(); }
        }
        #endregion

        // Foregroudh Brush：ファイルページのフォントカラー用
        #region Property: ForegroundBrush
        private Brush _ForegroundBrush = Brushes.White;
        public Brush ForegroundBrush
        {
            get { return _ForegroundBrush; }
            set { if (_ForegroundBrush != value) { _ForegroundBrush = value; OnPropertyChanged(); } }
        }
        #endregion

        // Backgroud Brush
        #region Property: BackgroundBrush
        private Brush _BackgroundBrush = Brushes.Black;
        public Brush BackgroundBrush
        {
            get { return _BackgroundBrush; }
            set { if (_BackgroundBrush != value) { _BackgroundBrush = value; OnPropertyChanged(); UpdateForegroundBrush(); } }
        }
        #endregion




        #region Property: MenuColor
        private PanelColor _MenuColor;
        public PanelColor PanelColor
        {
            get { return _MenuColor; }
            set { if (_MenuColor != value) { _MenuColor = value; FlushPanelColor(); OnPropertyChanged(); } }
        }
        public void FlushPanelColor()
        {
            int alpha = _PanelOpacity * 0xFF / 100;
            if (alpha > 0xff) alpha = 0xff;
            if (alpha < 0x00) alpha = 0x00;
            if (_MenuColor == PanelColor.Dark)
            {
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0x11, 0x11, 0x11));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0x22, 0x22, 0x22));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x90, 0xEE, 0x90));
            }
            else
            {
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0xF8, 0xF8, 0xF8));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x44, 0xBB, 0x44));
            }
        }
        #endregion


        #region Property: PanelOpacity
        private int _PanelOpacity = 100;
        public int PanelOpacity
        {
            get { return _PanelOpacity; }
            set { _PanelOpacity = value; FlushPanelColor(); OnPropertyChanged(); }
        }
        #endregion




        // 本管理
        public BookHub BookHub { get; private set; }



        // 標準ウィンドウタイトル
        private string _DefaultWindowTitle;


        #region 開発用

        // 開発用：JobEndine公開
        public JobEngine JobEngine => ModelContext.JobEngine;

        // 開発用：ページリスト
        public List<Page> PageList => BookHub.Current?.Pages;

        // 開発用：コンテンツ座標
        #region Property: ContentPosition
        private Point _ContentPosition;
        public Point ContentPosition
        {
            get { return _ContentPosition; }
            set { _ContentPosition = value; OnPropertyChanged(); }
        }
        #endregion

        // 開発用：コンテンツ座標情報更新
        public void UpdateContentPosition()
        {
            ContentPosition = MainContent.Content.PointToScreen(new Point(0, 0));
        }

        #endregion

        // DPI倍率
        private Point _DpiScaleFactor = new Point(1, 1);

        // DPIのXY比率が等しい？
        private bool _IsDpiSquare = false;

        // DPI設定
        public void UpdateDpiScaleFactor(Visual visual)
        {
            var dpiScaleFactor = DragExtensions.WPFUtil.GetDpiScaleFactor(visual);
            _DpiScaleFactor = dpiScaleFactor;
            _IsDpiSquare = _DpiScaleFactor.X == _DpiScaleFactor.Y;
        }

        // ダウンロード画像の保存場所
        public string UserDownloadPath { get; set; }

        public string DownloadPath => string.IsNullOrWhiteSpace(UserDownloadPath) ? Temporary.TempDownloadDirectory : UserDownloadPath;

        public string HistoryFileName { get; set; }

        // コンストラクタ
        public MainWindowVM()
        {
            HistoryFileName = System.IO.Path.Combine(Environment.CurrentDirectory, "History.xml");

            InitializeWindowIcons();

            // ModelContext
            ModelContext.Initialize();
            ModelContext.JobEngine.StatusChanged +=
                (s, e) => OnPropertyChanged(nameof(JobEngine));

            // BookHub
            BookHub = new BookHub();

            BookHub.Loading +=
                OnLoading;

            BookHub.BookChanged +=
                OnBookChanged;

            BookHub.PageChanged +=
                OnPageChanged;

            BookHub.ViewContentsChanged +=
                OnViewContentsChanged;

            BookHub.SettingChanged +=
                (s, e) =>
                {
                    OnPropertyChanged(nameof(BookSetting));
                    OnPropertyChanged(nameof(BookHub));
                };

            BookHub.InfoMessage +=
                (s, e) =>
                {
                    switch (NoticeShowMessageStyle)
                    {
                        case ShowMessageStyle.Normal:
                            Messenger.Send(this, new MessageEventArgs("MessageShow")
                            {
                                Parameter = new MessageShowParams(e)
                            });
                            break;
                        case ShowMessageStyle.Tiny:
                            TinyInfoText = e;
                            break;
                    }
                };

            BookHub.SlideShowModeChanged +=
                (s, e) => OnPropertyChanged(nameof(WindowIcon));

            BookHub.EmptyMessage +=
                (s, e) => EmptyPageMessage = e;


            // CommandTable
            ModelContext.CommandTable.SetTarget(this, BookHub);

            // Contents
            Contents = new ObservableCollection<ViewContent>();
            Contents.Add(new ViewContent());
            Contents.Add(new ViewContent());

            // Window title
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = FileVersionInfo.GetVersionInfo(assembly.Location);
            _DefaultWindowTitle = $"{assembly.GetName().Name} {ver.FileMajorPart}.{ver.FileMinorPart}";
#if DEBUG
            _DefaultWindowTitle += " [Debug]";
#endif

            // messenger
            Messenger.AddReciever("UpdateLastFiles", (s, e) => UpdateLastFiles());

            // ダウンロードフォルダ生成
            if (!System.IO.Directory.Exists(Temporary.TempDownloadDirectory))
            {
                System.IO.Directory.CreateDirectory(Temporary.TempDownloadDirectory);
            }
        }

        // Loading表示状態変更
        public void OnLoading(object sender, string e)
        {
            LoadingPath = e;
            Loading?.Invoke(sender, e);
        }


        // 本が変更された
        private void OnBookChanged(object sender, bool isBookmark)
        {
            var title = LoosePath.GetFileName(BookHub.Current.Place);

            switch (NoticeShowMessageStyle)
            {
                case ShowMessageStyle.Normal:
                    App.Current.Dispatcher.Invoke(() =>
                    Messenger.Send(this, new MessageEventArgs("MessageShow")
                    {
                        Parameter = new MessageShowParams(title)
                        {
                            IsBookmark = isBookmark,
                            DispTime = 2.0
                        }
                    }));

                    break;
                case ShowMessageStyle.Tiny:
                    TinyInfoText = title;
                    break;
            }

            OnPropertyChanged(nameof(Index));
            OnPropertyChanged(nameof(IndexMax));

            UpdateLastFiles();

            UpdatePageList(); // 開発用(重い)
        }

        // 開発用：ページ更新
        [Conditional("DEBUG")]
        private void UpdatePageList()
        {
            //OnPropertyChanged(nameof(PageList));
        }

        // 最近使ったファイル 更新
        private void UpdateLastFiles()
        {
            LastFiles = ModelContext.BookHistory.ListUp(10);
        }

        // 履歴削除
        public void ClearHistor()
        {
            ModelContext.BookHistory.Clear();
            UpdateLastFiles();
        }

        // Foregroud Brush 更新
        private void UpdateForegroundBrush()
        {
            var solidColorBrush = BackgroundBrush as SolidColorBrush;
            if (solidColorBrush != null)
            {
                double y =
                    (double)solidColorBrush.Color.R * 0.299 +
                    (double)solidColorBrush.Color.G * 0.587 +
                    (double)solidColorBrush.Color.B * 0.114;

                ForegroundBrush = (y < 0.25) ? Brushes.White : Brushes.Black;
            }
            else
            {
                ForegroundBrush = Brushes.Black;
            }
        }

        // Background Brush 更新
        private void UpdateBackgroundBrush()
        {
            switch (this.Background)
            {
                default:
                case BackgroundStyle.Black:
                    BackgroundBrush = Brushes.Black;
                    break;
                case BackgroundStyle.White:
                    BackgroundBrush = Brushes.White;
                    break;
                case BackgroundStyle.Auto:
                    BackgroundBrush = Contents[Contents[1].IsValid ? 1 : 0].Color;
                    break;
                case BackgroundStyle.Check:
                    BackgroundBrush = (DrawingBrush)App.Current.Resources["CheckerBrush"];
                    break;
            }
        }

        #region アプリ設定

        // アプリ設定作成
        public Setting CreateSetting()
        {
            var setting = new Setting();

            setting.ViewMemento = this.CreateMemento();
            setting.SusieMemento = ModelContext.SusieContext.CreateMemento();
            setting.BookHubMemento = BookHub.CreateMemento();
            setting.CommandMememto = ModelContext.CommandTable.CreateMemento();
            setting.DragActionMemento = ModelContext.DragActionTable.CreateMemento();
            setting.ExporterMemento = Exporter.CreateMemento();

            return setting;
        }

        // アプリ設定反映
        public void RestoreSetting(Setting setting)
        {
            this.Restore(setting.ViewMemento);
            ModelContext.SusieContext.Restore(setting.SusieMemento);
            BookHub.Restore(setting.BookHubMemento);

            ModelContext.CommandTable.Restore(setting.CommandMememto);
            ModelContext.DragActionTable.Restore(setting.DragActionMemento);
            InputGestureChanged?.Invoke(this, null);

            Exporter.Restore(setting.ExporterMemento);
        }

        // アプリ設定読み込み
        public void LoadSetting(MainWindow window)
        {
            Setting setting;

            // 設定の読み込み
            if (System.IO.File.Exists(App.UserSettingFileName))
            {
                try
                {
                    setting = Setting.Load(App.UserSettingFileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Messenger.MessageBox(this, "設定の読み込みに失敗しました。初期設定で起動します。", _DefaultWindowTitle, MessageBoxButton.OK, MessageBoxExImage.Warning);
                    setting = new Setting();
                }
            }
            else
            {
                setting = new Setting();
            }

            // 設定反映
            RestoreSetting(setting);

            // 履歴読み込み
            BookHistory.Memento bookHistoryMemento;
            if (System.IO.File.Exists(HistoryFileName))
            {
                try
                {
                    bookHistoryMemento = BookHistory.Memento.Load(HistoryFileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Messenger.MessageBox(this, "履歴の読み込みに失敗しました。", _DefaultWindowTitle, MessageBoxButton.OK, MessageBoxExImage.Warning);
                    bookHistoryMemento = new BookHistory.Memento();
                }
            }
            else
            {
                bookHistoryMemento = new BookHistory.Memento();
            }

            // 設定ファイルに残っている履歴をマージ
            if (setting.BookHistoryMemento != null)
            {
                bookHistoryMemento.Merge(setting.BookHistoryMemento);
            }

            // 反映
            ModelContext.BookHistory.Restore(bookHistoryMemento);
            UpdateLastFiles();

            // スライドショーの自動再生
            if (IsAutoPlaySlideShow)
            {
                BookHub.IsEnableSlideShow = true;
            }

            // パネル幅復元
            window.LeftPanel.Width = LeftPanelWidth;
            window.RightPanel.Width = RightPanelWidth;

            // ウィンドウ座標復元 (スレッドスリープする)
            WindowPlacement.Restore(window, setting.WindowPlacement, setting.ViewMemento.IsFullScreen);
        }


        // アプリ設定保存
        public void SaveSetting(MainWindow window)
        {
            // 現在の本を履歴に登録
            ModelContext.BookHistory.Add(BookHub.Current);

            // パネル幅保存
            LeftPanelWidth = window.LeftPanel.Width;
            RightPanelWidth = window.RightPanel.Width;

            // 設定
            var setting = CreateSetting();

            // ウィンドウ座標保存
            setting.WindowPlacement = WindowPlacement.CreateMemento(window);

            try
            {
                // 設定をファイルに保存
                setting.Save(App.UserSettingFileName);
            }
            catch { }

            try
            {
                // 履歴をファイルに保存
                var bookHistoryMemento = ModelContext.BookHistory.CreateMemento(true);
                bookHistoryMemento.Save(HistoryFileName);
            }
            catch { }
        }

        #endregion


        // 最後に開いたフォルダを開く
        public void LoadLastFolder()
        {
            if (!IsLoadLastFolder) return;

            var list = ModelContext.BookHistory.ListUp(1);
            if (list.Count > 0)
            {
                string place = list[0].Place;
                if (System.IO.Directory.Exists(place) || System.IO.File.Exists(place))
                    Load(place);
            }
        }

        // 表示コンテンツ更新
        private void OnViewContentsChanged(object sender, ViewSource e)
        {
            var contents = new List<ViewContent>();

            // ViewContent作成
            if (e?.Sources != null)
            {
                foreach (var source in e.Sources)
                {
                    if (source != null)
                    {
                        var content = new ViewContent();
                        content.Content = source.CreateControl(new Binding("ForegroundBrush") { Source = this }, new Binding("BitmapScalingMode") { Source = content });
                        content.Size = new Size(source.Width, source.Height);
                        content.Color = new SolidColorBrush(source.Color);
                        content.FilePlace = BookHub.Current?.Place;
                        content.FullPath = source.FullPath;
                        content.Position = source.Position;
                        content.PartSize = source.PartSize;
                        content.ReadOrder = source.ReadOrder;

                        if (source.Source is BitmapContent)
                        {
                            var bitmapContent = source.Source as BitmapContent;
                            content.Bitmap = bitmapContent.Source;
                            content.Info = bitmapContent.Info;
                        }
                        else if (source.Source is AnimatedGifContent)
                        {
                            var gifResource = source.Source as AnimatedGifContent;
                            content.Bitmap = gifResource.BitmapContent.Source;
                            content.Info = gifResource.BitmapContent.Info;
                            content.Info.Decoder = "MediaPlayer";
                        }
                        else if (source.Source is FilePageContent)
                        {
                            var filePageContext = source.Source as FilePageContent;
                            content.Info = filePageContext.Info;
                            content.Info.Decoder = null;
                        }

                        contents.Add(content);
                    }
                }
            }

            // ページが存在しない場合、専用メッセージを表示する
            IsVisibleEmptyPageMessage = contents.Count == 0;

            // メインとなるコンテンツを指定
            MainContent = contents.Count > 0 ? contents[0] : null;

            // ViewModelプロパティに反映
            for (int index = 0; index < 2; ++index)
            {
                Contents[index] = index < contents.Count ? contents[index] : new ViewContent();
            }

            // 背景色更新
            UpdateBackgroundBrush();

            // コンテンツサイズ更新
            UpdateContentSize();

            // 表示更新を通知
            ViewChanged?.Invoke(this, new ViewChangeArgs() { PageDirection = e != null ? e.Direction : 0 });
            OnPropertyChanged(nameof(WindowTitle));
        }


        // ページ番号の更新
        private void OnPageChanged(object sender, int e)
        {
            OnPropertyChanged(nameof(Index));
            OnPropertyChanged(nameof(IndexMax));
        }


        // ビューエリアサイズ
        private double _ViewWidth;
        private double _ViewHeight;

        // ビューエリアサイズを更新
        public void SetViewSize(double width, double height)
        {
            _ViewWidth = width;
            _ViewHeight = height;

            UpdateContentSize();
        }


        // コンテンツ表示サイズを更新
        private void UpdateContentSize()
        {
            if (!Contents.Any(e => e.IsValid)) return;

            // 2ページ表示時は重なり補正を行う
            double offsetWidth = 0;
            if (Contents[0].Size.Width > 0.5 && Contents[1].Size.Width > 0.5)
            {
                offsetWidth = 1.0 / _DpiScaleFactor.X;
                ContentsMargin = new Thickness(-offsetWidth, 0, 0, 0);
            }
            else
            {
                ContentsMargin = new Thickness(0);
            }

            var sizes = CalcContentSize(_ViewWidth * _DpiScaleFactor.X + offsetWidth, _ViewHeight * _DpiScaleFactor.Y);

            for (int i = 0; i < 2; ++i)
            {
                Contents[i].Width = sizes[i].Width / _DpiScaleFactor.X;
                Contents[i].Height = sizes[i].Height / _DpiScaleFactor.Y;
            }

            UpdateContentScalingMode();
        }

        // ビュー回転
        private double _ViewAngle;

        // ビュースケール
        private double _ViewScale;

        // ビュー変換を更新
        public void SetViewTransform(double scale, double angle)
        {
            _ViewAngle = angle;
            _ViewScale = scale;

            UpdateContentScalingMode();
        }

        // コンテンツスケーリングモードを更新
        private void UpdateContentScalingMode()
        {
            foreach (var content in Contents)
            {
                if (content.Content != null && content.Content is Rectangle)
                {
                    double diff = Math.Abs(content.Size.Width - content.Width * _DpiScaleFactor.X);
                    if (_IsDpiSquare && diff < 0.1 && _ViewAngle == 0.0 && _ViewScale == 1.0)
                    {
                        content.BitmapScalingMode = BitmapScalingMode.NearestNeighbor;
                    }
                    else
                    {
                        content.BitmapScalingMode = (IsEnabledNearestNeighbor && content.Size.Width < content.Width * _DpiScaleFactor.X * _ViewScale) ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality;
                    }
                }
            }
        }

        // ストレッチモードに合わせて各コンテンツのスケールを計算する
        private Size[] CalcContentSize(double width, double height)
        {
            var c0 = Contents[0].Size;
            var c1 = Contents[1].Size;

            // オリジナルサイズ
            if (this.StretchMode == PageStretchMode.None)
            {
                return new Size[] { c0, c1 };
            }

            double rate0 = 1.0;
            double rate1 = 1.0;

            // 2ページ合わせたコンテンツの表示サイズを求める
            Size content;
            if (!Contents[1].IsValid)
            {
                content = c0;
            }
            else
            {
                // どちらもImageでない
                if (c0.Width < 0.1 && c1.Width < 0.1)
                {
                    return new Size[] { c0, c1 };
                }

                if (c0.Width == 0) c0 = c1;
                if (c1.Width == 0) c1 = c0;

                // 高さを 高い方に合わせる
                if (c0.Height > c1.Height)
                {
                    rate1 = c0.Height / c1.Height;
                }
                else
                {
                    rate0 = c1.Height / c0.Height;
                }

                // 高さをあわせたときの幅の合計
                content = new Size(c0.Width * rate0 + c1.Width * rate1, c0.Height * rate0);
            }

            // ビューエリアサイズに合わせる場合のスケール
            double rateW = width / content.Width;
            double rateH = height / content.Height;

            // 拡大はしない
            if (this.StretchMode == PageStretchMode.Inside)
            {
                if (rateW > 1.0) rateW = 1.0;
                if (rateH > 1.0) rateH = 1.0;
            }
            // 縮小はしない
            else if (this.StretchMode == PageStretchMode.Outside)
            {
                if (rateW < 1.0) rateW = 1.0;
                if (rateH < 1.0) rateH = 1.0;
            }

            // 面積をあわせる
            if (this.StretchMode == PageStretchMode.UniformToSize)
            {
                var viewSize = width * height;
                var contentSize = content.Width * content.Height;
                var rate = Math.Sqrt(viewSize / contentSize);
                rate0 *= rate;
                rate1 *= rate;
            }
            // 高さを合わせる
            else if (this.StretchMode == PageStretchMode.UniformToVertical)
            {
                rate0 *= rateH;
                rate1 *= rateH;
            }
            // 枠いっぱいに広げる
            else if (this.StretchMode == PageStretchMode.UniformToFill)
            {
                if (rateW > rateH)
                {
                    rate0 *= rateW;
                    rate1 *= rateW;
                }
                else
                {
                    rate0 *= rateH;
                    rate1 *= rateH;
                }
            }
            // 枠に収めるように広げる
            else
            {
                if (rateW < rateH)
                {
                    rate0 *= rateW;
                    rate1 *= rateW;
                }
                else
                {
                    rate0 *= rateH;
                    rate1 *= rateH;
                }
            }

            var s0 = new Size(c0.Width * rate0, c0.Height * rate0);
            var s1 = new Size(c1.Width * rate1, c1.Height * rate1);
            return new Size[] { s0, s1 };
        }



        // コマンド実行 
        public void Execute(CommandType type, object param)
        {
            // 通知
            if (ModelContext.CommandTable[type].IsShowMessage)
            {
                string message = ModelContext.CommandTable[type].ExecuteMessage(param);

                switch (CommandShowMessageStyle)
                {
                    case ShowMessageStyle.Normal:
                        Messenger.Send(this, new MessageEventArgs("MessageShow")
                        {
                            Parameter = new MessageShowParams(message)
                        });
                        break;
                    case ShowMessageStyle.Tiny:
                        TinyInfoText = message;
                        break;
                }
            }

            // 実行
            ModelContext.CommandTable[type].Execute(param);
        }


        // ゼスチャ表示
        public void ShowGesture(string gesture, string commandName)
        {
            if (string.IsNullOrEmpty(gesture) && string.IsNullOrEmpty(commandName)) return;

            switch (GestureShowMessageStyle)
            {
                case ShowMessageStyle.Normal:
                    Messenger.Send(this, new MessageEventArgs("MessageShow")
                    {
                        Parameter = new MessageShowParams(((commandName != null) ? commandName + "\n" : "") + gesture)
                    });
                    break;
                case ShowMessageStyle.Tiny:
                    TinyInfoText = gesture + ((commandName != null) ? " " + commandName : "");
                    break;
            }
        }




        // スライドショーの表示間隔
        public double SlideShowInterval => BookHub.SlideShowInterval;

        // スライドショー：次のスライドへ
        public void NextSlide()
        {
            BookHub.NextSlide();
        }



        // フォルダ読み込み
        public void Load(string path)
        {
            BookHub.RequestLoad(path, BookLoadOption.None, true);
        }

        // ドラッグ＆ドロップ取り込み失敗
        public void LoadError(string message)
        {
            EmptyPageMessage = message ?? "コンテンツの読み込みに失敗しました";
            BookHub?.Unload(true);
        }


        // 廃棄処理
        public void Dispose()
        {
            ModelContext.Terminate();
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsLimitMove { get; set; }

            [DataMember]
            public bool IsControlCenterImage { get; set; }

            [DataMember]
            public bool IsAngleSnap { get; set; }

            [DataMember]
            public bool IsViewStartPositionCenter { get; set; }

            [DataMember]
            public PageStretchMode StretchMode { get; set; }

            [DataMember]
            public BackgroundStyle Background { get; set; }

            [DataMember]
            public bool IsSliderDirectionReversed { get; set; }

            [DataMember(Order = 4)]
            public ShowMessageStyle NoticeShowMessageStyle { get; set; }

            [DataMember]
            public ShowMessageStyle CommandShowMessageStyle { get; set; }

            [DataMember]
            public ShowMessageStyle GestureShowMessageStyle { get; set; }

            [DataMember(Order = 4)]
            public ShowMessageStyle NowLoadingShowMessageStyle { get; set; }

            [DataMember(Order = 1)]
            public bool IsEnabledNearestNeighbor { get; set; }

            [DataMember(Order = 2)]
            public bool IsKeepScale { get; set; }

            [DataMember(Order = 2)]
            public bool IsKeepAngle { get; set; }

            [DataMember(Order = 4)]
            public bool IsKeepFlip { get; set; }

            [DataMember(Order = 2)]
            public bool IsLoadLastFolder { get; set; }

            [DataMember(Order = 2)]
            public bool IsDisableMultiBoot { get; set; }

            [DataMember(Order = 4)]
            public bool IsAutoPlaySlideShow { get; set; }

            [DataMember(Order = 2)]
            public bool IsHideMenu { get; set; }

            [DataMember(Order = 4)]
            public bool IsHideTitleBar { get; set; }

            [DataMember(Order = 4)]
            public bool IsFullScreen { get; set; }

            [DataMember(Order = 4)]
            public bool IsSaveFullScreen { get; set; }

            [DataMember(Order = 4)]
            public bool IsTopmost { get; set; }

            [DataMember(Order = 5)]
            public bool IsVisibleFileInfo { get; set; }

            [DataMember(Order = 5)]
            public FileInfoSetting FileInfoSetting { get; set; }

            [DataMember(Order = 5)]
            public string UserDownloadPath { get; set; }

            [DataMember(Order = 6)]
            public FolderListSetting FolderListSetting { get; set; }

            [DataMember(Order = 6)]
            public PanelColor PanelColor { get; set; }

            [DataMember(Order = 6)]
            public int PanelOpacity { get; set; }

            [DataMember(Order = 7)]
            public PanelType LeftPanel { get; set; }

            [DataMember(Order = 7)]
            public PanelType RightPanel { get; set; }

            [DataMember(Order = 7)]
            public double LeftPanelWidth { get; set; }

            [DataMember(Order = 7)]
            public double RightPanelWidth { get; set; }


            void Constructor()
            {
                IsLimitMove = true;
                IsSliderDirectionReversed = true;
                NoticeShowMessageStyle = ShowMessageStyle.Normal;
                CommandShowMessageStyle = ShowMessageStyle.Normal;
                GestureShowMessageStyle = ShowMessageStyle.Normal;
                NowLoadingShowMessageStyle = ShowMessageStyle.Normal;
                StretchMode = PageStretchMode.Uniform;
                Background = BackgroundStyle.Black;
                FileInfoSetting = new FileInfoSetting();
                FolderListSetting = new FolderListSetting();
                PanelColor = PanelColor.Dark;
                PanelOpacity = 100;
                LeftPanelWidth = 250;
                RightPanelWidth = 250;
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsLimitMove = this.IsLimitMove;
            memento.IsControlCenterImage = this.IsControlCenterImage;
            memento.IsAngleSnap = this.IsAngleSnap;
            memento.IsViewStartPositionCenter = this.IsViewStartPositionCenter;
            memento.StretchMode = this.StretchMode;
            memento.Background = this.Background;
            memento.IsSliderDirectionReversed = this.IsSliderDirectionReversed;
            memento.NoticeShowMessageStyle = this.NoticeShowMessageStyle;
            memento.CommandShowMessageStyle = this.CommandShowMessageStyle;
            memento.GestureShowMessageStyle = this.GestureShowMessageStyle;
            memento.NowLoadingShowMessageStyle = this.NowLoadingShowMessageStyle;
            memento.IsEnabledNearestNeighbor = this.IsEnabledNearestNeighbor;
            memento.IsKeepScale = this.IsKeepScale;
            memento.IsKeepAngle = this.IsKeepAngle;
            memento.IsKeepFlip = this.IsKeepFlip;
            memento.IsLoadLastFolder = this.IsLoadLastFolder;
            memento.IsDisableMultiBoot = this.IsDisableMultiBoot;
            memento.IsAutoPlaySlideShow = this.IsAutoPlaySlideShow;
            memento.IsHideMenu = this.IsHideMenu;
            memento.IsHideTitleBar = this.IsHideTitleBar;
            memento.IsFullScreen = this.IsFullScreen;
            memento.IsSaveFullScreen = this.IsSaveFullScreen;
            memento.IsTopmost = this.IsTopmost;
            memento.IsVisibleFileInfo = this.IsVisibleFileInfo;
            memento.FileInfoSetting = this.FileInfoSetting.Clone();
            memento.UserDownloadPath = this.UserDownloadPath;
            memento.FolderListSetting = this.FolderListSetting.Clone();
            memento.PanelColor = this.PanelColor;
            memento.PanelOpacity = this.PanelOpacity;
            memento.LeftPanel = this.LeftPanel;
            memento.RightPanel = this.RightPanel;
            memento.LeftPanelWidth = this.LeftPanelWidth;
            memento.RightPanelWidth = this.RightPanelWidth;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            this.IsLimitMove = memento.IsLimitMove;
            this.IsControlCenterImage = memento.IsControlCenterImage;
            this.IsAngleSnap = memento.IsAngleSnap;
            this.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
            this.StretchMode = memento.StretchMode;
            this.Background = memento.Background;
            this.IsSliderDirectionReversed = memento.IsSliderDirectionReversed;
            this.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
            this.CommandShowMessageStyle = memento.CommandShowMessageStyle;
            this.GestureShowMessageStyle = memento.GestureShowMessageStyle;
            this.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
            this.IsEnabledNearestNeighbor = memento.IsEnabledNearestNeighbor;
            this.IsKeepScale = memento.IsKeepScale;
            this.IsKeepAngle = memento.IsKeepAngle;
            this.IsKeepFlip = memento.IsKeepFlip;
            this.IsLoadLastFolder = memento.IsLoadLastFolder;
            this.IsDisableMultiBoot = memento.IsDisableMultiBoot;
            this.IsAutoPlaySlideShow = memento.IsAutoPlaySlideShow;
            this.IsHideMenu = memento.IsHideMenu;
            this.IsHideTitleBar = memento.IsHideTitleBar;
            this.IsSaveFullScreen = memento.IsSaveFullScreen;
            if (this.IsSaveFullScreen) this.IsFullScreen = memento.IsFullScreen;
            this.IsTopmost = memento.IsTopmost;
            this.IsVisibleFileInfo = memento.IsVisibleFileInfo;
            this.FileInfoSetting = memento.FileInfoSetting.Clone();
            this.UserDownloadPath = memento.UserDownloadPath;
            this.FolderListSetting = memento.FolderListSetting.Clone();
            this.PanelColor = memento.PanelColor;
            this.PanelOpacity = memento.PanelOpacity;
            this.LeftPanel = memento.LeftPanel;
            this.RightPanel = memento.RightPanel;
            this.LeftPanelWidth = memento.LeftPanelWidth;
            this.RightPanelWidth = memento.RightPanelWidth;


            ViewChanged?.Invoke(this, new ViewChangeArgs() { ResetViewTransform = true });
        }

        #endregion

    }
}
