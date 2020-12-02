using NeeView.Data;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, IHasDpiScale, IWindowStateControllable
    {
        public static MainWindow Current { get; private set; }

        private MainWindowViewModel _vm;
        private RoutedCommandBinding _routedCommandBinding;
        private ViewComponent _viewComponent;
        private DpiProvider _dpiProvider = new DpiProvider();


        #region コンストラクターと初期化処理

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MainWindow()
        {
            Interop.NVFpReset();

            InitializeComponent();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            Debug.WriteLine($"App.MainWndow.Initialize: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            Current = this;

            ContextMenuWatcher.Initialize();

            // Window状態初期化
            InitializeWindowShapeSnap();

            // 固定画像初期化
            Thumbnail.InitializeBasicImages();
            FileIconCollection.Current.InitializeAsync();

            // FpReset 念のため
            Interop.NVFpReset();

            // Drag&Drop設定
            //ContentDropManager.Current.SetDragDropEvent(MainView);

            // ViewComponent
            _viewComponent = ViewComponent.Current;
            _viewComponent.Initialize();

            RoutedCommandTable.Current.AddMouseInput(_viewComponent.MouseInput);
            RoutedCommandTable.Current.AddTouchInput(_viewComponent.TouchInput);

            MainViewManager.Current.Initialize(_viewComponent, this.MainViewSocket);

            // MainWindow : ViewModel
            _vm = new MainWindowViewModel(MainWindowModel.Current);
            this.DataContext = _vm;

            _vm.FocusMainViewCall += (s, e) => _viewComponent.RaiseFocusMainViewRequest();

            // コマンド初期化
            _routedCommandBinding = new RoutedCommandBinding(this, RoutedCommandTable.Current);

            // サイドパネル初期化
            MainLayoutPanelManager.Current.Initialize();


            // 各コントロールとモデルを関連付け
            this.PageSliderView.Source = PageSlider.Current;
            this.MediaControlView.Source = MediaControl.Current;
            this.ThumbnailListArea.Source = ThumbnailList.Current;
            this.AddressBar.Source = NeeView.AddressBar.Current;
            this.MenuBar.Source = NeeView.MenuBar.Current;

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHideMenu),
                (s, e) => DartyMenuAreaLayout());

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHidePageSlider),
                (s, e) => DartyPageSliderLayout());

            Config.Current.FilmStrip.AddPropertyChanged(nameof(FilmStripConfig.IsEnabled),
                (s, e) => DartyThumbnailListLayout());

            Config.Current.FilmStrip.AddPropertyChanged(nameof(FilmStripConfig.IsHideFilmStrip),
                (s, e) => DartyThumbnailListLayout());

            ThumbnailList.Current.VisibleEvent +=
                ThumbnailList_Visible;

            _viewComponent.ContentCanvas.AddPropertyChanged(nameof(ContentCanvas.IsMediaContent),
                (s, e) => DartyPageSliderLayout());

            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.IsFullScreen),
                (s, e) => FullScreenChanged());


            // mouse drag
            DragActionTable.Current.SetTarget(_viewComponent.DragTransformControl);

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

            // 開発用初期化
            Debug_Initialize();

            Debug.WriteLine($"App.MainWndow.Initialize.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }


        /// <summary>
        /// Window状態初期設定 
        /// TODO: もっと前に処理できる。起動オプションの反映タイミングぐらい。
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
            WindowShape.Current.IsEnabled = true;
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
                Config.Current.Window.WindowPlacement = WindowPlacementTools.StoreWindowPlacement(this, Config.Current.Window.IsRestoreAeroSnapPlacement);
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

            var placement = Config.Current.Window.WindowPlacement;
            if (placement == null || !placement.IsValid()) return;

            var windowState = (Config.Current.Window.State == WindowStateEx.Maximized || Config.Current.Window.State == WindowStateEx.FullScreen) ? WindowState.Maximized : WindowState.Normal;
            var newPlacement = new WindowPlacement(windowState, placement.Left, placement.Top, placement.Width, placement.Height);

            WindowPlacementTools.RestoreWindowPlacement(this, newPlacement);
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

            MainViewManager.Current.Update();

            // 一瞬手前に表示
            WindowShape.Current.OneTopmost();

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

            WindowShape.Current.InitializeStateChangeAction();

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

        // ウィンドウ最大化(Toggle)
        public void MainWindow_Maximize()
        {
            ToggleMaximize();
        }

        // ウィンドウ最小化
        public void MainWindow_Minimize()
        {
            ToggleMinimize();
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
            // DragMove終了直後のマウス座標が不正(0,0)になるのようなので、この場合は無効にする
            var windowPoint = e.GetPosition(this);
            if (windowPoint.X == 0.0 && windowPoint.Y == 0.0)
            {
                Debug.WriteLine("Wrong cursor position!");
                e.Handled = true;
            }
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
            var isChanged = _dpiProvider.SetDip(e.NewDpi);
            if (!isChanged) return;

            //
            this.MenuBar.WindowCaptionButtons.UpdateStrokeThickness(e.NewDpi);

            // Window Border
            WindowShape.Current?.UpdateWindowBorderThickness();
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
            MainLayoutPanelManager.Current?.Store();
            MainLayoutPanelManager.Current?.SetIsStoreEnabled(false);

            // メインビューの保存
            MainViewManager.Current?.Store();
            MainViewManager.Current?.SetIsStoreEnabled(false);

            // ウィンドウ座標の保存
            StoreWindowPlacement();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            App.Current.DisableUnhandledException();

            ApplicationDisposer.Current.Dispose();

            CompositionTarget.Rendering -= OnRendering;

            // タイマー停止
            ////_nonActiveTimer.Stop();

            Debug.WriteLine("Window.Closed done.");
        }

        #endregion

        #region メニューエリア、ステータスエリアマウスオーバー監視

        public bool _isDockMenuMouseOver;
        public bool _isLayerMenuMuseOver;

        private void UpdateMenuAreaMouseOver()
        {
            _vm.IsMenuAreaMouseOver = _isDockMenuMouseOver || _isLayerMenuMuseOver;
        }

        private void DockMenuSocket_MouseEnter(object sender, MouseEventArgs e)
        {
            _isDockMenuMouseOver = true;
            UpdateMenuAreaMouseOver();
        }

        private void DockMenuSocket_MouseLeave(object sender, MouseEventArgs e)
        {
            _isDockMenuMouseOver = false;
            UpdateMenuAreaMouseOver();
        }

        private void LayerMenuSocket_MouseEnter(object sender, MouseEventArgs e)
        {
            _isLayerMenuMuseOver = true;
            UpdateMenuAreaMouseOver();
        }

        private void LayerMenuSocket_MouseLeave(object sender, MouseEventArgs e)
        {
            _isLayerMenuMuseOver = false;
            UpdateMenuAreaMouseOver();
        }

        public bool _isDockStatusMouseOver;
        public bool _isLayeStatusMuseOver;

        private void UpdateStatusAreaMouseOver()
        {
            _vm.IsStatusAreaMouseOver = _isDockStatusMouseOver || _isLayeStatusMuseOver;
        }

        private void DockStatusArea_MouseEnter(object sender, MouseEventArgs e)
        {
            _isDockStatusMouseOver = true;
            UpdateStatusAreaMouseOver();
        }

        private void DockStatusArea_MouseLeave(object sender, MouseEventArgs e)
        {
            _isDockStatusMouseOver = false;
            UpdateStatusAreaMouseOver();
        }

        private void LayerStatusArea_MouseEnter(object sender, MouseEventArgs e)
        {
            _isLayeStatusMuseOver = true;
            UpdateStatusAreaMouseOver();
        }

        private void LayerStatusArea_MouseLeave(object sender, MouseEventArgs e)
        {
            _isLayeStatusMuseOver = false;
            UpdateStatusAreaMouseOver();
        }

        #endregion

        #region レイアウト管理

        private bool _isDartyMenuAreaLayout;
        private bool _isDartyPageSliderLayout;
        private bool _isDartyThumbnailListLayout;

        /// <summary>
        /// フルスクリーン状態が変更されたときの処理
        /// </summary>
        private void FullScreenChanged()
        {
            DartyWindowLayout();

            // フルスクリーン解除でフォーカスが表示されたパネルに移動してしまう現象を回避
            if (!WindowShape.Current.IsFullScreen && Config.Current.Panels.IsHidePanelInFullscreen)
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
            bool isMenuDock = !Config.Current.MenuBar.IsHideMenu && !WindowShape.Current.IsFullScreen;

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
                this.LayerStatusAreaPadding.Visibility = Visibility.Visible;
            }
            else
            {
                this.DockPageSliderSocket.Content = null;
                this.LayerPageSliderSocket.Content = this.SliderArea;
                this.LayerStatusAreaPadding.Visibility = Visibility.Collapsed;
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
                this.PageSliderView.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// フィルムストリップレイアウト更新
        /// </summary>
        private void UpdateThumbnailListLayout()
        {
            if (!_isDartyThumbnailListLayout) return;
            _isDartyThumbnailListLayout = false;

            bool isPageSliderDock = !Config.Current.Slider.IsHidePageSlider && !WindowShape.Current.IsFullScreen;
            bool isThimbnailListDock = !Config.Current.FilmStrip.IsHideFilmStrip && isPageSliderDock;

            if (isThimbnailListDock)
            {
                this.LayerThumbnailListSocket.Content = null;
                this.DockThumbnailListSocket.Content = this.ThumbnailListArea;
            }
            else
            {
                this.DockThumbnailListSocket.Content = null;
                this.LayerThumbnailListSocket.Content = this.ThumbnailListArea;
            }

            // フィルムストリップ
            this.ThumbnailListArea.Visibility = Config.Current.FilmStrip.IsEnabled && !_viewComponent.ContentCanvas.IsMediaContent ? Visibility.Visible : Visibility.Collapsed;
            this.ThumbnailListArea.DartyThumbnailList();
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

        #endregion

        #region IHasDpiScale support

        public DpiScale GetDpiScale()
        {
            return _dpiProvider.RawDpi;
        }

        #endregion

        #region IWindowStateControllable

        public void ToggleMinimize()
        {
            SystemCommands.MinimizeWindow(this);
        }

        public void ToggleMaximize()
        {
            if (this.WindowState != WindowState.Maximized)
            {
                SystemCommands.MaximizeWindow(this);
            }
            else
            {
                SystemCommands.RestoreWindow(this);
            }
        }

        public void ToggleFullScreen()
        {
            WindowShape.Current.ToggleFullScreen();
        }

        #endregion IWindowStateControllable

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
