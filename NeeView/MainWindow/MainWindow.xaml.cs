﻿using NeeView.Data;
using NeeView.Native;
using NeeView.Threading;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, IDpiScaleProvider, IHasWindowController, INotifyMouseHorizontalWheelChanged, IHasRenameManager, IMainViewWindow
    {
        public static MainWindow Current { get; private set; }

        private MainWindowViewModel _vm;
        private RoutedCommandBinding _routedCommandBinding;
        private MainViewComponent _viewComponent;
        private DpiScaleProvider _dpiProvider = new DpiScaleProvider();

        private WindowChromeAccessor _windowChromeAccessor;
        private WindowStateManager _windowStateManager;
        private WindowShape _windowShape;
        private WindowController _windowController;


        public MainWindow()
        {
            Interop.NVFpReset();

            InitializeComponent();

            // TextBox の ContextMenu のスタイルを変更する ... やりすぎ？
            // ThemeProfile.InitializeEditorContextMenuStyle(this);
        }


        public event MouseWheelEventHandler MouseHorizontalWheelChanged;


        public WindowStateManager WindowStateManager => _windowStateManager;
        public WindowController WindowController => _windowController;


        #region 初期化処理

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            Debug.WriteLine($"App.MainWndow.Initialize: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            Current = this;

            DragDropHelper.AttachDragOverTerminator(this);

            // Window状態初期化
            InitializeWindowShapeSnap();

            _windowChromeAccessor = new WindowChromeAccessor(this);
            _windowStateManager = new WindowStateManager(this, new WindowStateManagerDependency(_windowChromeAccessor, TabletModeWatcher.Current));
            _windowShape = new WindowShape(_windowStateManager, _windowChromeAccessor);
            _windowController = new WindowController(_windowStateManager, _windowShape);

            ContextMenuWatcher.Initialize();

            var mouseHorizontalWheel = new MouseHorizontalWheelService(this);
            mouseHorizontalWheel.MouseHorizontalWheelChanged += (s, e) => MouseHorizontalWheelChanged?.Invoke(s, e);

            // 固定画像初期化
            Thumbnail.InitializeBasicImages();
            FileIconCollection.Current.InitializeAsync();

            // FpReset 念のため
            Interop.NVFpReset();

            // Drag&Drop設定
            //ContentDropManager.Current.SetDragDropEvent(MainView);

            // ViewComponent
            _viewComponent = MainViewComponent.Current;
            _viewComponent.Initialize();

            RoutedCommandTable.Current.AddMouseInput(_viewComponent.MouseInput);
            RoutedCommandTable.Current.AddTouchInput(_viewComponent.TouchInput);

            MainViewManager.Current.Initialize(_viewComponent, this.MainViewSocket);

            MainWindowModel.Current.Initialize(_windowShape, _windowStateManager);

            // MainWindow : ViewModel
            _vm = new MainWindowViewModel(MainWindowModel.Current);
            this.DataContext = _vm;

            _vm.FocusMainViewCall += (s, e) => _viewComponent.RaiseFocusMainViewRequest();

            // コマンド初期化
            _routedCommandBinding = new RoutedCommandBinding(this, RoutedCommandTable.Current);

            // サイドパネル初期化
            CustomLayoutPanelManager.Current.Initialize();

            // 各コントロールとモデルを関連付け
            this.PageSliderView.Source = PageSlider.Current;
            this.MediaControlView.Source = MediaControl.Current;
            this.ThumbnailListArea.Source = ThumbnailList.Current;
            this.MenuBar.Source = new MenuBar(_windowStateManager);
            this.AddressBar.Source = new AddressBar();

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHideMenu),
                (s, e) => DartyMenuAreaLayout());

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHideMenu),
                (s, e) => DartyMenuAreaLayout());

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHidePageSlider),
                (s, e) => DartyPageSliderLayout());

            Config.Current.Slider.AddPropertyChanged(nameof(SliderConfig.IsEnabled),
                (s, e) => DartyPageSliderLayout());

            Config.Current.FilmStrip.AddPropertyChanged(nameof(FilmStripConfig.IsEnabled),
                (s, e) => DartyThumbnailListLayout());

            Config.Current.FilmStrip.AddPropertyChanged(nameof(FilmStripConfig.IsHideFilmStrip),
                (s, e) => DartyThumbnailListLayout());

            ThumbnailList.Current.VisibleEvent +=
                ThumbnailList_Visible;

            _viewComponent.ContentCanvas.AddPropertyChanged(nameof(ContentCanvas.IsMediaContent),
                (s, e) => DartyPageSliderLayout());

            _windowShape.AddPropertyChanged(nameof(WindowShape.AutoHideMode),
                (s, e) => AutoHideModeChanged());

            // initialize routed commands
            RoutedCommandTable.Current.UpdateInputGestures();

            // watch menu bar visibility
            this.MenuArea.IsVisibleChanged += (s, e) => Config.Current.MenuBar.IsVisible = this.MenuArea.IsVisible;

            // watchi slider visibility
            this.SliderArea.IsVisibleChanged += (s, e) => Config.Current.Slider.IsVisible = this.SliderArea.IsVisible;

            // moue event for window
            this.PreviewMouseMove += MainWindow_PreviewMouseMove;
            this.PreviewMouseUp += MainWindow_PreviewMouseUp;
            this.PreviewMouseDown += MainWindow_PreviewMouseDown;
            this.PreviewMouseWheel += MainWindow_PreviewMouseWheel;
            this.PreviewStylusDown += MainWindow_PreviewStylusDown;

            // mouse acticate
            this.MouseDown += MainWindow_MouseDown;

            // key event for window
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            this.PreviewKeyUp += MainWindow_PreviewKeyUp;
            this.KeyDown += MainWindow_KeyDown;

            // cancel rename triggers
            this.MouseLeftButtonDown += (s, e) => this.RenameManager.Stop();
            this.MouseRightButtonDown += (s, e) => this.RenameManager.Stop();
            this.Deactivated += (s, e) => this.RenameManager.Stop();

            // frame event
            CompositionTarget.Rendering += OnRendering;

            // message layer space
            InitializeMessageLayerSpace();

            // page caption
            InitializePageCaption();

            // 開発用初期化
            Debug_Initialize();

            Debug.WriteLine($"App.MainWndow.Initialize.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }


        /// <summary>
        /// Window状態初期設定 
        /// </summary>
        private void InitializeWindowShapeSnap()
        {
            var state = Config.Current.Window.State;

            if (App.Current.Option.IsResetPlacement == SwitchOption.on)
            {
                state = WindowStateEx.Normal;
            }
            else if (state == WindowStateEx.FullScreen)
            {
                state = Config.Current.StartUp.IsRestoreFullScreen ? WindowStateEx.FullScreen : WindowStateEx.Normal;
            }
            else if (state == WindowStateEx.Minimized)
            {
                state = WindowStateEx.Normal;
            }
            else if (!Config.Current.StartUp.IsRestoreWindowPlacement)
            {
                state = WindowStateEx.Normal;
            }

            // セカンドプロセスはウィンドウ形状を継承しない
            if (Environment.IsSecondProcess && !Config.Current.StartUp.IsRestoreSecondWindowPlacement)
            {
                state = WindowStateEx.Normal;
            }

            if (App.Current.Option.WindowState.HasValue)
            {
                state = App.Current.Option.WindowState.ToWindowStateEx();
            }

            Config.Current.Window.State = state;
        }


        /// <summary>
        /// Window状態初期化
        /// </summary>
        private void InitializeWindowShape()
        {
            _windowShape.IsEnabled = true;
        }

        #endregion


        #region マウスによるウィンドウアクティブ監視

        public bool IsMouseActivate { get; private set; }

        public void SetMouseActivage()
        {
            if (!IsActive)
            {
                IsMouseActivate = true;
                var async = ResetMouseActivateAsync(100);
            }
        }

        private async Task ResetMouseActivateAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
            IsMouseActivate = false;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseActivate = false;
        }

        #endregion

        #region ウィンドウ状態コマンド

        /// <summary>
        /// ウィンドウ最小化コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ////FocusMainView();
            _viewComponent.RaiseFocusMainViewRequest();
            SystemCommands.MinimizeWindow(this);
        }

        /// <summary>
        /// 通常ウィンドウ化コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestoreWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        /// <summary>
        /// ウィンドウ最大化コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaximizeWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        /// <summary>
        /// ウィンドウ終了コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindowCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        #endregion

        #region ウィンドウ座標保存

        // ウィンドウ座標保存
        public void StoreWindowPlacement()
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero) return;

            try
            {
                Config.Current.Window.LastState = _windowStateManager.ResumeState;
                Config.Current.Window.WindowPlacement = _windowStateManager.StoreWindowPlacement(Config.Current.Window.IsRestoreAeroSnapPlacement);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // ウィンドウ座標復元
        public void RestoreWindowPlacement()
        {
            // 座標を復元しない
            if (App.Current.Option.IsResetPlacement == SwitchOption.on || !Config.Current.StartUp.IsRestoreWindowPlacement) return;

            // セカンドプロセスはウィンドウ形状を継承しない
            if (Environment.IsSecondProcess && !Config.Current.StartUp.IsRestoreSecondWindowPlacement) return;

            var state = Config.Current.Window.State;
            var placement = Config.Current.Window.WindowPlacement;

            if (placement != null && placement.IsValid())
            {
                placement = placement.WithState(state.ToWindowState(), state.IsFullScreen());

                _windowStateManager.ResumeState = Config.Current.Window.LastState;
                _windowStateManager.RestoreWindowPlacement(placement);
            }
            else
            {
                _windowStateManager.SetWindowState(state);
            }
        }

        #endregion

        #region ウィンドウイベント処理

        /// <summary>
        /// フレーム処理
        /// </summary>
        private void OnRendering(object sender, EventArgs e)
        {
            LayoutFrame();
        }


        // ウィンドウソース初期化後イベント
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            Debug.WriteLine($"App.MainWndow.SourceInitialized: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            // NOTE: Chromeの変更を行った場合、Loadedイベントが発生する。WindowPlacementの処理順番に注意
            InitializeWindowShape();

            // ウィンドウ座標の復元
            RestoreWindowPlacement();

            Debug.WriteLine($"App.MainWndow.SourceInitialized.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }

        // ウィンドウ表示開始
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"App.MainWndow.Loaded: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            _dpiProvider.SetDipScale(VisualTreeHelper.GetDpi(this));

            MainViewManager.Current.Update();

            this.SetBinding(Window.TopmostProperty, new Binding(nameof(WindowShape.IsTopmost)) { Source = _windowShape });

            // レイアウト更新
            DartyWindowLayout();

            // WinProc登録
            WindowMessage.Current.Initialize(this);
            TabletModeWatcher.Current.Initialize(this);

            _vm.Loaded();

            App.Current.IsMainWindowLoaded = true;

            Trace.WriteLine($"App.MainWndow.Loaded.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }

        // ウィンドウコンテンツ表示開始
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Debug.WriteLine($"App.MainWndow.ContentRendered: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            _vm.ContentRendered();

            Debug.WriteLine($"App.MainWndow.ContentRendered.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }

        // ウィンドウアクティブ
        private void MainWindow_Activated(object sender, EventArgs e)
        {
            ////SetCursorVisible(true);
            _vm.Activated();
        }

        // ウィンドウ非アクティブ
        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            ////SetCursorVisible(true);
            _vm.Deactivated();
        }

        /// <summary>
        /// ウィンドウ状態変更イベント処理
        /// </summary>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // ALTキーのメニュー操作無効 (Alt+F4は常に有効)
            if (!Config.Current.Command.IsAccessKeyEnabled && (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey != Key.F4))
            {
                e.Handled = true;
            }
        }

        private void MainWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();
        }

        private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();
        }

        private void MainWindow_PreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();
        }

        private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // NOTE: フルスクリーンのときに左上の(0,0)座標が取得できなくなるので下記処理を無効化 (2021-01-04)
            // NOTE: 下記処理をしないことによる不具合が確認できないため、無効化してしばらく様子見する
#if false
            // WindowChromeでのウィンドウ移動直後のマウス座標が不正(0,0)になるのようなので、この場合は無効にする
            var windowPoint = e.GetPosition(this);
            if (windowPoint.X == 0.0 && windowPoint.Y == 0.0)
            {
                Debug.WriteLine($"Wrong cursor position!");
                e.Handled = true;
            }
#endif
        }

        private void MainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
        }


        /// <summary>
        /// DPI変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            var isChanged = _dpiProvider.SetDipScale(e.NewDpi);
            if (!isChanged) return;

            this.MenuBar.WindowCaptionButtons.UpdateStrokeThickness(e.NewDpi);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.IsClosing = true;

            RoutedCommandTable.Current.Dispose();

            // 設定ウィンドウの保存動作を無効化
            if (Setting.SettingWindow.Current != null)
            {
                Setting.SettingWindow.Current.AllowSave = false;
            }

            // パネルレイアウトの保存
            CustomLayoutPanelManager.Current?.Store();
            CustomLayoutPanelManager.Current?.SetIsStoreEnabled(false);

            // メインビューの保存
            MainViewManager.Current?.Store();
            MainViewManager.Current?.SetIsStoreEnabled(false);

            // ウィンドウ座標の保存
            StoreWindowPlacement();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;

            App.Current.DisableUnhandledException();

            ApplicationDisposer.Current.Dispose();

            // タイマー停止
            ////_nonActiveTimer.Stop();

            Debug.WriteLine("Window.Closed done.");
        }

        #endregion

        #region メニューエリア、ステータスエリアマウスオーバー監視

        public bool IsMenuAreaMouseOver()
        {
            return this.MenuArea.IsMouseOver;
        }

        public bool IsStatusAreaMouseOver()
        {
            return this.DockStatusArea.IsMouseOver || this.LayerStatusArea.IsMouseOver;
        }

        #endregion

        #region レイアウト管理

        private bool _isDartyMenuAreaLayout;
        private bool _isDartyPageSliderLayout;
        private bool _isDartyThumbnailListLayout;

        /// <summary>
        /// 自動非表示モードが変更されたときの処理
        /// </summary>
        private void AutoHideModeChanged()
        {
            DartyWindowLayout();

            // 解除でフォーカスが表示されたパネルに移動してしまう現象を回避
            if (!_windowShape.AutoHideMode && Config.Current.Panels.IsHidePanelInAutoHideMode)
            {
                _viewComponent.RaiseFocusMainViewRequest();
            }
        }

        /// <summary>
        /// レイアウト更新要求
        /// </summary>
        private void DartyWindowLayout()
        {
            _isDartyMenuAreaLayout = true;
            _isDartyPageSliderLayout = true;
            _isDartyThumbnailListLayout = true;
        }

        /// <summary>
        /// メニューエリアレイアウト更新要求
        /// </summary>
        private void DartyMenuAreaLayout()
        {
            _isDartyMenuAreaLayout = true;
        }

        /// <summary>
        /// スライダーレイアウト更新要求
        /// </summary>
        private void DartyPageSliderLayout()
        {
            _isDartyPageSliderLayout = true;
            _isDartyThumbnailListLayout = true; // フィルムストリップも更新
        }

        /// <summary>
        /// フィルムストリップ更新要求
        /// </summary>
        private void DartyThumbnailListLayout()
        {
            _isDartyThumbnailListLayout = true;
        }


        /// <summary>
        /// レイアウト更新フレーム処理
        /// </summary>
        private void LayoutFrame()
        {
            UpdateMenuAreaLayout();
            UpdateStatusAreaLayout();
        }

        /// <summary>
        /// メニューエリアレイアウト更新。
        /// </summary>
        private void UpdateMenuAreaLayout()
        {
            if (!_isDartyMenuAreaLayout) return;
            _isDartyMenuAreaLayout = false;

            // menu hide
            bool isMenuDock = !MainWindowModel.Current.CanHideMenu;

            if (isMenuDock)
            {
                this.LayerMenuSocket.Content = null;
                this.DockMenuSocket.Content = this.MenuArea;
            }
            else
            {
                this.DockMenuSocket.Content = null;
                this.LayerMenuSocket.Content = this.MenuArea; ;
            }
        }

        /// <summary>
        /// ステータスエリアレイアウト更新。
        /// </summary>
        private void UpdateStatusAreaLayout()
        {
            UpdatePageSliderLayout();
            UpdateThumbnailListLayout();
        }

        /// <summary>
        /// ページスライダーレイアウト更新。
        /// </summary>
        private void UpdatePageSliderLayout()
        {
            if (!_isDartyPageSliderLayout) return;
            _isDartyPageSliderLayout = false;

            // menu hide
            bool isPageSliderDock = !MainWindowModel.Current.CanHidePageSlider;

            if (isPageSliderDock)
            {
                this.LayerPageSliderSocket.Content = null;
                this.DockPageSliderSocket.Content = this.SliderArea;
                this.MediaControlView.IsBackgroundOpacityEnabled = false;
                this.PageSliderView.IsBackgroundOpacityEnabled = false;
            }
            else
            {
                this.DockPageSliderSocket.Content = null;
                this.LayerPageSliderSocket.Content = this.SliderArea;
                this.MediaControlView.IsBackgroundOpacityEnabled = true;
                this.PageSliderView.IsBackgroundOpacityEnabled = true;
            }

            // visibility
            if (_viewComponent.ContentCanvas.IsMediaContent)
            {
                this.MediaControlView.Visibility = Visibility.Visible;
                this.PageSliderView.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.MediaControlView.Visibility = Visibility.Collapsed;
                this.PageSliderView.Visibility = Config.Current.Slider.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// フィルムストリップレイアウト更新
        /// </summary>
        private void UpdateThumbnailListLayout()
        {
            if (!_isDartyThumbnailListLayout) return;
            _isDartyThumbnailListLayout = false;

            bool isPageSliderDock = !MainWindowModel.Current.CanHidePageSlider;
            bool isThimbnailListDock = !Config.Current.FilmStrip.IsHideFilmStrip && isPageSliderDock;

            if (isThimbnailListDock)
            {
                this.LayerThumbnailListSocket.Content = null;
                this.DockThumbnailListSocket.Content = this.ThumbnailListArea;
                this.ThumbnailListArea.IsBackgroundOpacityEnabled = false;
            }
            else
            {
                this.DockThumbnailListSocket.Content = null;
                this.LayerThumbnailListSocket.Content = this.ThumbnailListArea;
                this.ThumbnailListArea.IsBackgroundOpacityEnabled = true;
            }

            // フィルムストリップ
            this.ThumbnailListArea.Visibility = Config.Current.FilmStrip.IsEnabled && !_viewComponent.ContentCanvas.IsMediaContent ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ThumbnailList_Visible(object sender, VisibleEventArgs e)
        {
            _vm.StatusAutoHideDescrption.VisibleOnce();
            _vm.ThumbnailListusAutoHideDescrption.VisibleOnce();

            if (e.IsFocus)
            {
                ThumbnailList.Current.FocusAtOnce();
            }
        }

        private void InitializeMessageLayerSpace()
        {
            this.DockStatusArea.SizeChanged += (s, e) => { if (e.HeightChanged) { UpdateMessageLayerSpace(); } };

            UpdateMessageLayerSpace();
        }

        private void UpdateMessageLayerSpace()
        {
            this.MessageLayerSpace.Height = Math.Max(this.DockStatusArea.ActualHeight, 30.0) + 10.0;
        }

        #endregion レイアウト管理

        #region ページタイトル管理

        private DelayVisibility _pageCaptionVisibility;

        private void InitializePageCaption()
        {
            this.DockStatusArea.SizeChanged += (s, e) => UpdatePageCaptionLayout();
            this.DockStatusArea.MouseEnter += (s, e) => UpdatePageCaptionVisibility();
            this.DockStatusArea.MouseLeave += (s, e) => UpdatePageCaptionVisibility();

            this.LayerStatusArea.IsVisibleChanged += (s, e) => UpdatePageCaptionLayout();
            this.LayerStatusArea.SizeChanged += (s, e) => UpdatePageCaptionLayout();
            this.LayerStatusArea.MouseEnter += (s, e) => UpdatePageCaptionVisibility();
            this.LayerStatusArea.MouseLeave += (s, e) => UpdatePageCaptionVisibility();

            PageTitle.Current.AddPropertyChanged(nameof(PageTitle.Title), (s, e) => UpdatePageCaptionVisibility());

            _pageCaptionVisibility = new DelayVisibility();
            _pageCaptionVisibility.Changed += PageCaptionVisibility_Changed;

            this.PageCaption.Visibility = _pageCaptionVisibility.Visibility;
            UpdatePageCaptionLayout();
            UpdatePageCaptionVisibility();
        }

        private void PageCaptionVisibility_Changed(object sender, EventArgs e)
        {
            this.PageCaption.Visibility = _pageCaptionVisibility.Visibility;
        }

        private void UpdatePageCaptionLayout()
        {
            const double margin = 5.0;
            var space = Math.Max(this.LayerStatusArea.IsVisible ? this.LayerStatusArea.ActualHeight : 0.0, this.DockStatusArea.ActualHeight);
            this.PageCaption.Margin = new Thickness(margin, margin, margin, margin + space);
        }

        private void UpdatePageCaptionVisibility()
        {
            if (ContextMenuWatcher.TargetElement != null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(PageTitle.Current.Title))
            {
                _pageCaptionVisibility.SetDelayVisibility(Visibility.Collapsed, 0, NeeView.Windows.Data.DelayValueOverwriteOption.Force);
            }
            else
            {
                var isVisible = Config.Current.PageTitle.IsEnabled && (this.LayerStatusArea.IsMouseOver || this.DockStatusArea.IsMouseOver);
                if (isVisible)
                {
                    _pageCaptionVisibility.SetDelayVisibility(Visibility.Visible, 0);
                }
                else
                {
                    _pageCaptionVisibility.SetDelayVisibility(Visibility.Collapsed, (int)(Config.Current.AutoHide.AutoHideDelayTime * 1000));
                }
            }
        }

        #endregion ページタイトル管理

        #region IHasDpiScale

        public DpiScale GetDpiScale()
        {
            return _dpiProvider.DpiScale;
        }

        #endregion IHasDpiScale

        #region IHasRenameManager

        public RenameManager GetRenameManager()
        {
            return this.RenameManager;
        }

        #endregion IHasRenameManager

        #region [開発用]

        public MainWindowViewModel ViewModel => _vm;

        // [開発用] 設定初期化
        [Conditional("DEBUG")]
        private void Debug_Initialize()
        {
            DebugGesture.Initialize();
        }


        #endregion

    }

}
