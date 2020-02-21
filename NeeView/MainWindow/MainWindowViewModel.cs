using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace NeeView
{
    /// <summary>
    /// MainWindow : ViewModel
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        #region SidePanels

        /// <summary>
        /// SidePanelMargin property.
        /// メニュの自動非表示ON/OFFによるサイドパネル上部の余白
        /// </summary>
        public Thickness SidePanelMargin
        {
            get { return _SidePanelMargin; }
            set { if (_SidePanelMargin != value) { _SidePanelMargin = value; RaisePropertyChanged(); } }
        }

        //
        private Thickness _SidePanelMargin;

        //
        private void UpdateSidePanelMargin()
        {
            SidePanelMargin = new Thickness(0, _model.CanHideMenu ? 26 : 0, 0, _model.CanHidePageSlider ? 20 : 0);
        }


        /// <summary>
        /// CanvasWidth property.
        /// キャンバスサイズ。SidePanelから引き渡される
        /// </summary>
        public double CanvasWidth
        {
            get { return _CanvasWidth; }
            set { if (_CanvasWidth != value) { _CanvasWidth = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasWidth;


        /// <summary>
        /// CanvasHeight property.
        /// </summary>
        public double CanvasHeight
        {
            get { return _CanvasHeight; }
            set { if (_CanvasHeight != value) { _CanvasHeight = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasHeight;

        #endregion

        #region Window Icon

        // ウィンドウアイコン：標準
        private ImageSource _windowIconDefault;

        // ウィンドウアイコン：スライドショー再生中
        private ImageSource _windowIconPlay;

        // ウィンドウアイコン初期化
        private void InitializeWindowIcons()
        {
            _windowIconDefault = null;
            _windowIconPlay = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Play.ico", UriKind.RelativeOrAbsolute));
        }

        // 現在のウィンドウアイコン取得
        public ImageSource WindowIcon
            => SlideShow.Current.IsPlayingSlideShow ? _windowIconPlay : _windowIconDefault;

        #endregion

        #region コンテキストメニュー

        //
        private ContextMenu _contextMenu;
        public ContextMenu ContextMenu
        {
            get
            {
                if (_model.ContextMenuSetting.IsDarty)
                {
                    Debug.WriteLine($"new ContextMenu.");
                    _contextMenu = _model.ContextMenuSetting.ContextMenu;
                    _contextMenu?.UpdateInputGestureText();
                }
                return _contextMenu;
            }
        }

        public void UpdateContextMenu()
        {
            if (_model.ContextMenuSetting.IsDarty)
            {
                RaisePropertyChanged(nameof(ContextMenu));
            }
        }


        #endregion


        private MainWindowModel _model;
        private Visibility _busyVisibility = Visibility.Collapsed;
        private bool _isMenuAreaMouseOver;
        private bool _isStatusAreaMouseOver;


        /// <summary>
        /// コンストラクター
        /// </summary>
        public MainWindowViewModel(MainWindowModel model)
        {
            MenuAutoHideDescription = new BasicAutoHideDescription(MainWindow.Current.LayerMenuSocket);
            StatusAutoHideDescrption = new BasicAutoHideDescription(MainWindow.Current.LayerStatusArea);
            ThumbnailListusAutoHideDescrption = new BasicAutoHideDescription(MainWindow.Current.LayerThumbnailListSocket);

            // icon
            InitializeWindowIcons();

            // mainwindow model
            _model = model;

            _model.AddPropertyChanged(nameof(_model.ContextMenuSetting),
                (s, e) => UpdateContextMenu());

            _model.AddPropertyChanged(nameof(_model.CanHideMenu),
                (s, e) => UpdateSidePanelMargin());

            _model.AddPropertyChanged(nameof(_model.CanHidePageSlider),
                (s, e) =>
                {
                    UpdateSidePanelMargin();
                    RaisePropertyChanged(nameof(CanHideThumbnailList));
                });

            _model.AddPropertyChanged(nameof(_model.CanHidePanel),
                (s, e) => UpdateSidePanelMargin());

            _model.FocusMainViewCall += Model_FocusMainViewCall;

            // 初期化
            UpdateContextMenu();


            // SlideShow link to WindowIcon
            SlideShow.Current.AddPropertyChanged(nameof(SlideShow.IsPlayingSlideShow),
                (s, e) => RaisePropertyChanged(nameof(WindowIcon)));

            ContentRebuild.Current.AddPropertyChanged(nameof(ContentRebuild.IsBusy),
                (s, e) => UpdateBusyVisibility());

            BookOperation.Current.AddPropertyChanged(nameof(BookOperation.IsBusy),
                (s, e) => UpdateBusyVisibility());

            BookHub.Current.AddPropertyChanged(nameof(BookHub.IsLoading),
                (s, e) => UpdateBusyVisibility());

            ThumbnailList.Current.AddPropertyChanged(nameof(CanHideThumbnailList),
                (s, e) => RaisePropertyChanged(nameof(CanHideThumbnailList)));

            BookHub.Current.BookChanged +=
                (s, e) => CommandManager.InvalidateRequerySuggested();

            Config.Current.LocalApplicationDataRemoved +=
                (s, e) =>
                {
                    SaveData.Current.IsEnableSave = false; // 保存禁止
                    App.Current.MainWindow.Close();
                };


            // TODO: アプリの初期化処理で行うべき
            // ダウンロードフォルダー生成
            if (!System.IO.Directory.Exists(Temporary.Current.TempDownloadDirectory))
            {
                System.IO.Directory.CreateDirectory(Temporary.Current.TempDownloadDirectory);
            }
        }



        public event EventHandler FocusMainViewCall;


        public bool IsClosing { get; set; }

        /// <summary>
        /// BusyVisibility property.
        /// アクセス中マーク表示用
        /// </summary>
        public Visibility BusyVisibility
        {
            get { return _busyVisibility; }
            set { if (_busyVisibility != value) { _busyVisibility = value; RaisePropertyChanged(); } }
        }


        // for Binding
        public WindowShape WindowShape => WindowShape.Current;
        public WindowTitle WindowTitle => WindowTitle.Current;
        public ThumbnailList ThumbnailList => ThumbnailList.Current;
        public ContentCanvasBrush ContentCanvasBrush => ContentCanvasBrush.Current;
        public ImageEffect ImageEffect => ImageEffect.Current;
        public MouseInput MouseInput => NeeView.MouseInput.Current;
        public InfoMessage InfoMessage => InfoMessage.Current;
        public SidePanel SidePanel => SidePanel.Current;
        public ContentCanvas ContentCanvas => ContentCanvas.Current;
        public LoupeTransform LoupeTransform => LoupeTransform.Current;
        public ToastService ToastService => ToastService.Current;
        public App App => App.Current;


        public MainWindowModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public bool CanHideThumbnailList
        {
            get
            {
                return ThumbnailList.CanHideThumbnailList && !Model.CanHidePageSlider;
            }
        }

        /// <summary>
        /// Menu用AutoHideBehavior補足
        /// </summary>
        public BasicAutoHideDescription MenuAutoHideDescription { get; }

        /// <summary>
        /// FilmStrep, Slider 用 AutoHideBehavior 補足
        /// </summary>
        public BasicAutoHideDescription StatusAutoHideDescrption { get; }

        public BasicAutoHideDescription ThumbnailListusAutoHideDescrption { get; }

        /// <summary>
        /// メニューエリアマウスオーバー
        /// Viewから更新される
        /// </summary>
        public bool IsMenuAreaMouseOver
        {
            get { return _isMenuAreaMouseOver; }
            set
            {
                if (SetProperty(ref _isMenuAreaMouseOver, value))
                {
                    RaisePropertyChanged(nameof(IsFrontAreaMouseOver));
                }
            }
        }

        /// <summary>
        /// ステータスエリアマウスオーバー
        /// Viewから更新される
        /// </summary>
        public bool IsStatusAreaMouseOver
        {
            get { return _isStatusAreaMouseOver; }
            set
            {
                if (SetProperty(ref _isStatusAreaMouseOver, value))
                {
                    RaisePropertyChanged(nameof(IsFrontAreaMouseOver));
                }
            }
        }

        /// <summary>
        /// メニューエリア、ステータスエリアどちらかの上にマウスがある
        /// </summary>
        public bool IsFrontAreaMouseOver => IsMenuAreaMouseOver || IsStatusAreaMouseOver;


        private void Model_FocusMainViewCall(object sender, EventArgs e)
        {
            FocusMainViewCall?.Invoke(sender, e);
        }

        // 処理中表示の更新
        private void UpdateBusyVisibility()
        {
            ////Debug.WriteLine($"IsBusy: {BookHub.Current.IsLoading}, {BookOperation.Current.IsBusy}, {ContentRebuild.Current.IsBusy}");
            this.BusyVisibility = _model.IsVisibleBusy && (BookHub.Current.IsLoading || BookOperation.Current.IsBusy || ContentRebuild.Current.IsBusy) && !SlideShow.Current.IsPlayingSlideShow ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 起動時処理
        /// </summary>
        public void Loaded()
        {
            _model.Loaded();
        }

        /// <summary>
        /// ウィンドウがアクティブ化したときの処理
        /// </summary>
        public void Activated()
        {
            if (IsClosing) return;

            RoutedCommandTable.Current.InitializeInputGestures();
            UpdateContextMenu();
        }

        /// <summary>
        /// ウィンドウが非アクティブ化したときの処理
        /// </summary>
        public void Deactivated()
        {
            if (IsClosing) return;

            // NOTE: メインスレッドで行うとSevenZipSharpがCOM例外になるのであえてタスク化。なぜ！？
            Task.Run(() =>
            {
                try
                {
                    ArchiverManager.Current.UnlockAllArchives();
                }
                catch
                {
                }
            });
        }
    }
}
