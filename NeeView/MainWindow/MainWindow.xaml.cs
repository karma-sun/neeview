using NeeView.Data;
using NeeView.Native;
using NeeView.Threading;
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
    public partial class MainWindow : Window
    {
        public static MainWindow Current { get; private set; }

        private MainWindowViewModel _vm;


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

            var setting = SaveData.Current.UserSettingTemp;

            // Window状態初期化、復元
            InitializeWindowShapeSnap(setting.WindowShape);
            InitializeWindowPlacement(setting.WindowPlacement);

            // 固定画像初期化
            Thumbnail.InitializeBasicImages();
            FileIconCollection.Current.InitializeAsync();

            // サイドパネルの初期化
            SidePanel.Current.Initialize();

            // Drag&Drop設定
            ContentDropManager.Current.SetDragDropEvent(MainView);

            // MainWindow : ViewModel
            _vm = new MainWindowViewModel(MainWindowModel.Current);
            this.DataContext = _vm;

            // 各コントロールとモデルを関連付け
            this.PageSliderView.Source = PageSlider.Current;
            this.PageSliderView.FocusTo = this.MainView;
            this.MediaControlView.Source = MediaControl.Current;
            this.ThumbnailListArea.Source = ThumbnailList.Current;
            this.AddressBar.Source = NeeView.AddressBar.Current;
            this.MenuBar.Source = NeeView.MenuBar.Current;
            this.NowLoadingView.Source = NowLoading.Current;


            // コマンド初期化
            InitializeCommand();
            InitializeCommandBindings();

            // レイヤー表示管理初期化
            InitializeLayerVisibility();

            //
            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.IsHideMenu),
                (s, e) => DartyMenuAreaLayout());

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.CanHidePageSlider),
                (s, e) => DartyPageSliderLayout());

            MainWindowModel.Current.AddPropertyChanged(nameof(MainWindowModel.IsPanelVisibleLocked),
                (s, e) => UpdateControlsVisibility());

            ThumbnailList.Current.AddPropertyChanged(nameof(ThumbnailList.IsEnableThumbnailList),
                (s, e) => DartyThumbnailListLayout());

            ThumbnailList.Current.AddPropertyChanged(nameof(ThumbnailList.IsHideThumbnailList),
                (s, e) => DartyThumbnailListLayout());

            SidePanel.Current.ResetFocus +=
                (s, e) => ResetFocus();

            ContentCanvas.Current.AddPropertyChanged(nameof(ContentCanvas.IsMediaContent),
                (s, e) => DartyPageSliderLayout());

            this.AddressBar.IsAddressTextBoxFocusedChanged +=
                (s, e) => UpdateMenuLayerVisibility();

            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.IsFullScreen),
                (s, e) => FullScreenChanged());


            // mouse input
            var mouse = MouseInput.Current;

            // mouse drag
            DragActionTable.Current.SetTarget(DragTransformControl.Current);


            // render transform
            var transformView = new TransformGroup();
            transformView.Children.Add(DragTransform.Current.TransformView);
            transformView.Children.Add(LoupeTransform.Current.TransformView);
            this.MainContent.RenderTransform = transformView;
            this.MainContent.RenderTransformOrigin = new Point(0.5, 0.5);

            var transformCalc = new TransformGroup();
            transformCalc.Children.Add(DragTransform.Current.TransformCalc);
            transformCalc.Children.Add(LoupeTransform.Current.TransformCalc);
            this.MainContentShadow.RenderTransform = transformCalc;
            this.MainContentShadow.RenderTransformOrigin = new Point(0.5, 0.5);


            // initialize routed commands
            RoutedCommandTable.Current.InitializeInputGestures();

            // mouse event capture for active check
            this.MainView.PreviewMouseMove += MainView_PreviewMouseMove;
            this.MainView.PreviewMouseDown += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseUp += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseWheel += MainView_PreviewMouseAction;
            this.MainView.MouseEnter += MainView_MouseEnter;

            // timer 
            InitializeNonActiveTimer();


            // moue event for window
            this.PreviewMouseMove += MainWindow_PreviewMouseMove;
            this.PreviewMouseUp += MainWindow_PreviewMouseUp;
            this.PreviewMouseDown += MainWindow_PreviewMouseDown;
            this.PreviewMouseWheel += MainWindow_PreviewMouseWheel;
            this.PreviewStylusDown += MainWindow_PreviewStylusDown;

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
        /// Window座標初期化
        /// </summary>
        private void InitializeWindowPlacement(WindowPlacement.Memento memento)
        {
            // 座標を復元しない
            if (App.Current.Option.IsResetPlacement == SwitchOption.on || !App.Current.IsSaveWindowPlacement) return;

            // セカンドプロセスはウィンドウ形状を継承しない
            if (Config.Current.IsSecondProcess && !App.Current.IsRestoreSecondWindow) return;

            WindowPlacement.Current.IsMaximized = WindowShape.Current.SnapMemento != null
                ? WindowShape.Current.SnapMemento.State == WindowStateEx.Maximized || WindowShape.Current.SnapMemento.State == WindowStateEx.FullScreen
                : false;

            WindowPlacement.Current.Restore(memento);
        }

        /// <summary>
        /// Window状態初期設定
        /// </summary>
        private void InitializeWindowShapeSnap(WindowShape.Memento memento)
        {
            if (memento == null) return;

            var customMemento = memento.Clone();

            if (App.Current.Option.IsResetPlacement == SwitchOption.on || !App.Current.IsSaveWindowPlacement)
            {
                customMemento.State = WindowStateEx.Normal;
            }

            if (customMemento.State == WindowStateEx.FullScreen)
            {
                customMemento.State = App.Current.IsSaveFullScreen ? WindowStateEx.FullScreen : WindowStateEx.Normal;
            }

            // セカンドプロセスはウィンドウ形状を継承しない
            if (Config.Current.IsSecondProcess && !App.Current.IsRestoreSecondWindow)
            {
                customMemento.State = WindowStateEx.Normal;
            }

            if (App.Current.Option.IsFullScreen == SwitchOption.on)
            {
                customMemento.State = WindowStateEx.FullScreen;
            }

            WindowShape.Current.SnapMemento = customMemento;
        }


        /// <summary>
        /// Window状態初期化
        /// </summary>
        private void InitializeWindowShape()
        {
            var windowShape = WindowShape.Current;

            windowShape.WindowChromeFrame = App.Current.WindowChromeFrame;
            windowShape.Restore(windowShape.SnapMemento);
            windowShape.IsEnabled = true;
        }

        #endregion

        #region コマンドバインディング

        // MainWindow依存コマンド登録
        public void InitializeCommand()
        {
            var commandTable = CommandTable.Current;

            // MainWindow:View依存コマンド登録
            commandTable[CommandType.CloseApplication].Execute =
                (s, e) => this.Close();
            commandTable[CommandType.ToggleWindowMinimize].Execute =
                (s, e) => MainWindow_Minimize();
            commandTable[CommandType.ToggleWindowMaximize].Execute =
                (s, e) => MainWindow_Maximize();

            // print
            commandTable[CommandType.Print].Execute =
                (s, e) => ContentCanvas.Current.Print(this, this.PageContents, this.MainContent.RenderTransform, this.MainView.ActualWidth, this.MainView.ActualHeight);

            // context menu
            commandTable[CommandType.OpenContextMenu].Execute =
                (s, e) => OpenContextMenu();

            //  コマンド実行後処理
            RoutedCommandTable.Current.CommandExecuted += RoutedCommand_CommandExecuted;
        }


        // コマンド実行後処理
        private void RoutedCommand_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            // ダブルクリックでコマンド実行後のMouseButtonUpイベントをキャンセルする
            if (e.Gesture is MouseGesture mouse)
            {
                switch (mouse.MouseAction)
                {
                    case MouseAction.LeftDoubleClick:
                    case MouseAction.RightDoubleClick:
                    case MouseAction.MiddleDoubleClick:
                        _skipMouseButtonUp = true;
                        break;
                }
            }
            else if (e.Gesture is MouseExGesture mouseEx)
            {
                switch (mouseEx.MouseExAction)
                {
                    case MouseExAction.LeftDoubleClick:
                    case MouseExAction.RightDoubleClick:
                    case MouseExAction.MiddleDoubleClick:
                    case MouseExAction.XButton1DoubleClick:
                    case MouseExAction.XButton2DoubleClick:
                        _skipMouseButtonUp = true;
                        break;
                }
            }
        }

        // コマンド：コンテキストメニューを開く
        private void OpenContextMenu()
        {
            if (this.MainViewPanel.ContextMenu != null)
            {
                this.MainViewPanel.ContextMenu.DataContext = _vm;
                this.MainViewPanel.ContextMenu.PlacementTarget = this.MainViewPanel;
                this.MainViewPanel.ContextMenu.Placement = PlacementMode.MousePoint;
                this.MainViewPanel.ContextMenu.IsOpen = true;
            }
        }

        // RoutedCommand バインディング
        public void InitializeCommandBindings()
        {
            var commandTable = CommandTable.Current;
            var commands = RoutedCommandTable.Current.Commands;

            // コマンドバインド作成
            foreach (CommandType type in Enum.GetValues(typeof(CommandType)))
            {
                if (commandTable[type].CanExecute != null)
                {
                    this.CommandBindings.Add(new CommandBinding(commands[type], (sender, e) => RoutedCommandTable.Current.Execute(sender, e, type),
                        (sender, e) => e.CanExecute = commandTable[type].CanExecute()));
                }
                else
                {
                    this.CommandBindings.Add(new CommandBinding(commands[type], (sender, e) => RoutedCommandTable.Current.Execute(sender, e, type),
                        CanExecute));
                }
            }
        }

        // ロード中のコマンドを無効にする CanExecute
        private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !NowLoading.Current.IsDispNowLoading;
        }

        #endregion

        #region タイマーによる非アクティブ監視

        // タイマーディスパッチ
        private DispatcherTimer _nonActiveTimer;

        // 非アクティブ時間チェック用
        private DateTime _lastActionTime;
        private Point _lastActionPoint;
        private double _cursorMoveDistance;

        // 一定時間操作がなければカーソルを非表示にする仕組み
        // 初期化
        private void InitializeNonActiveTimer()
        {
            _nonActiveTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            _nonActiveTimer.Interval = TimeSpan.FromSeconds(0.2);
            _nonActiveTimer.Tick += new EventHandler(DispatcherTimer_Tick);

            _vm.Model.AddPropertyChanged(nameof(MainWindowModel.IsCursorHideEnabled), (s, e) => UpdateNonActiveTimerActivity());
            UpdateNonActiveTimerActivity();
        }

        private void UpdateNonActiveTimerActivity()
        {
            if (_vm.Model.IsCursorHideEnabled)
            {
                _nonActiveTimer.Start();
            }
            else
            {
                _nonActiveTimer.Stop();
            }

            SetCursorVisible(true);
        }

        // タイマー処理
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            // 非アクティブ時間が続いたらマウスカーソルを非表示にする
            if (IsCursurVisibled() && (DateTime.Now - _lastActionTime).TotalSeconds > _vm.Model.CursorHideTime)
            {
                SetCursorVisible(false);
            }
        }

        // マウス移動
        private void MainView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var nowPoint = e.GetPosition(this.MainView);

            if (IsCursurVisibled())
            {
                _cursorMoveDistance = 0.0;
            }
            else
            {
                _cursorMoveDistance += Math.Abs(nowPoint.X - _lastActionPoint.X) + Math.Abs(nowPoint.Y - _lastActionPoint.Y);
                if (_cursorMoveDistance > _vm.Model.CursorHideReleaseDistance)
                {
                    SetCursorVisible(true);
                }
            }

            _lastActionPoint = nowPoint;
            _lastActionTime = DateTime.Now;
        }

        // マウスアクション
        private void MainView_PreviewMouseAction(object sender, MouseEventArgs e)
        {
            if (_vm.Model.IsCursorHideReleaseAction)
            {
                SetCursorVisible(true);
            }

            _cursorMoveDistance = 0.0;
            _lastActionTime = DateTime.Now;
        }

        // 表示領域にマウスが入った
        private void MainView_MouseEnter(object sender, MouseEventArgs e)
        {
            SetCursorVisible(true);
        }

        // マウスカーソル表示ON/OFF
        private void SetCursorVisible(bool isVisible)
        {
            ////Debug.WriteLine($"Cursur: {isVisible}");
            _cursorMoveDistance = 0.0;
            _lastActionTime = DateTime.Now;

            isVisible = isVisible | !_vm.Model.IsCursorHideEnabled;
            if (isVisible)
            {
                if (this.MainView.Cursor == Cursors.None && !MouseInput.Current.IsLoupeMode)
                {
                    this.MainView.Cursor = null;
                }
            }
            else
            {
                if (this.MainView.Cursor == null && this.IsActive)
                {
                    this.MainView.Cursor = Cursors.None;
                }
            }
        }

        /// <summary>
        /// カーソル表示判定
        /// </summary>
        private bool IsCursurVisibled()
        {
            return this.MainView.Cursor != Cursors.None || MouseInput.Current.IsLoupeMode;
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

        #region ウィンドウイベント処理


        /// <summary>
        /// マウスボタンUPイベントキャンセル
        /// </summary>
        private bool _skipMouseButtonUp;

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

            WindowShape.Current.SetHook();

            // NOTE: Chromeの変更を行った場合、Loadedイベントが発生する
            InitializeWindowShape();

            Debug.WriteLine($"App.MainWndow.SourceInitialized.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }

        // ウィンドウ表示開始
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"App.MainWndow.Loaded: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            // 一瞬手前に表示
            WindowShape.Current.OneTopmost();

            // レイアウト更新
            DartyWindowLayout();

            // WinProc登録
            WindowMessage.Current.Initialize(this);

            _vm.Loaded();

            Debug.WriteLine($"App.MainWndow.Loaded.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }

        // ウィンドウコンテンツ表示開始
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Debug.WriteLine($"App.MainWndow.ContentRendered: {App.Current.Stopwatch.ElapsedMilliseconds}ms");

            WindowShape.Current.InitializeStateChangeAction();

            // version information
            ////if (App.Current.SettingVersion != 0 && App.Current.SettingVersion < Config.Current.ProductVersionNumber && Config.Current.ProductVersionNumber == Config.GenerateProductVersionNumber(32, 0, 0))
            ////{
            ////    ToastService.Current.Show(new Toast(Properties.Resources.Ver320Note, Properties.Resources.Ver320, ToastIcon.Information, TimeSpan.FromSeconds(15.0)));
            ////}

            Debug.WriteLine($"App.MainWndow.ContentRendered.Done: {App.Current.Stopwatch.ElapsedMilliseconds}ms");
        }

        // ウィンドウアクティブ
        private void MainWindow_Activated(object sender, EventArgs e)
        {
            SetCursorVisible(true);
            _vm.Activated();
        }

        // ウィンドウ非アクティブ
        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            SetCursorVisible(true);
            _vm.Deactivated();
        }

        // ウィンドウ最大化(Toggle)
        private void MainWindow_Maximize()
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

        // ウィンドウ最小化
        private void MainWindow_Minimize()
        {
            SystemCommands.MinimizeWindow(this);
        }


        /// <summary>
        /// ウィンドウ状態変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            // ルーペ解除
            MouseInput.Current.IsLoupeMode = false;
        }


        // ウィンドウサイズが変化したらコンテンツサイズも追従する
        private void MainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContentCanvas.Current.SetViewSize(this.MainView.ActualWidth, this.MainView.ActualHeight);

            // スナップ
            DragTransformControl.Current.SnapView();
        }


        // 
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 単キーのショートカットを有効にする。
            // TextBoxなどのイベント処理でこのフラグをfalseにすることで短キーのショートカットを無効にして入力を優先させる
            KeyExGesture.AllowSingleKey = true;

            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();

            // 一部 IMEKey のっとり
            if (e.Key == Key.ImeProcessed && e.ImeProcessedKey.IsImeKey())
            {
                RoutedCommandTable.Current.ExecuteImeKeyGestureCommand(sender, e);
            }
        }

        // 
        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        // 
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // ALTキーのメニュー操作無効 (Alt+F4は常に有効)
            if (!_vm.Model.IsAccessKeyEnabled && (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey != Key.F4))
            {
                e.Handled = true;
            }
        }


        //
        private void MainWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // ダブルクリック後のイベントキャンセル
            if (_skipMouseButtonUp)
            {
                ///Debug.WriteLine("Skip MuseUpEvent");
                _skipMouseButtonUp = false;
                e.Handled = true;
            }
        }


        //
        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();
        }

        //
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


        //
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

        //
        private void MainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            // パネル表示状態更新
            UpdateControlsVisibility();
        }


        /// <summary>
        /// DPI変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            var isChanged = Config.Current.SetDip(e.NewDpi);
            if (!isChanged) return;

            //
            this.MenuBar.WindowCaptionButtons.UpdateStrokeThickness(e.NewDpi);

            // 背景更新
            ContentCanvasBrush.Current.UpdateBackgroundBrush();

            // Window Border
            WindowShape.Current?.UpdateWindowBorderThickness();

#if false
            // ウィンドウサイズのDPI非追従
            if (App.Current.IsIgnoreWindowDpi && this.WindowState == WindowState.Normal)
            {
                var newWidth = Math.Floor(this.Width * e.OldDpi.DpiScaleX / e.NewDpi.DpiScaleX);
                var newHeight = Math.Floor(this.Height * e.OldDpi.DpiScaleY / e.NewDpi.DpiScaleY);

                // 反映タイミングをずらす
                AppDispatcher.BeginInvoke((Action)(() =>
                {
                    this.Width = newWidth;
                    this.Height = newHeight;
                }));
            }
#endif
        }

        //
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.IsClosing = true;

            // 閉じる前にウィンドウサイズ保存
            WindowShape.Current.CreateSnapMemento();

            // 設定ウィンドウの保存動作を無効化
            if (Setting.SettingWindow.Current != null)
            {
                Setting.SettingWindow.Current.AllowSave = false;
            }
        }

        //
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            App.Current.DisableUnhandledException();

            ContentCanvas.Current.Dispose();
            ApplicationDisposer.Current.Dispose();

            //
            CompositionTarget.Rendering -= OnRendering;

            // タイマー停止
            _nonActiveTimer.Stop();

            // 設定保存
            SaveDataSync.Current.Flush();
            SaveDataSync.Current.SaveUserSetting(false);
            SaveDataSync.Current.SaveHistory();
            SaveDataSync.Current.SaveBookmark(false);
            SaveDataSync.Current.SavePagemark(false);
            SaveDataSync.Current.RemoveBookmarkIfNotSave();
            SaveDataSync.Current.RemovePagemarkIfNotSave();

            // キャッシュ等の削除
            App.Current.CloseTemporary();

            Debug.WriteLine("Window.Closed done.");
            //Environment.Exit(0);
        }

        #endregion

        #region メニューエリア、ステータスエリアマウスオーバー監視

        public bool _isDockMenuMouseOver;
        public bool _isLayerMenuMuseOver;

        private void UpdateMenuAreaMouseOver()
        {
            _vm.Model.IsMenuAreaMouseOver = _isDockMenuMouseOver || _isLayerMenuMuseOver;
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
            _vm.Model.IsStatusAreaMouseOver = _isDockStatusMouseOver || _isLayeStatusMuseOver;
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
            if (!WindowShape.Current.IsFullScreen && MainWindowModel.Current.IsHidePanelInFullscreen)
            {
                ResetFocus();
            }
        }

        /// <summary>
        /// MainViewにフォーカスを移動する
        /// </summary>
        private void ResetFocus()
        {
            AppDispatcher.BeginInvoke(() =>
            {
                this.MainView.Focus();
            });
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
            UpdatePageSliderLayout();
            UpdateThumbnailListLayout();
        }

        /// <summary>
        /// メニューエリアレイアウト更新。
        /// </summary>
        private void UpdateMenuAreaLayout()
        {
            if (!_isDartyMenuAreaLayout) return;
            _isDartyMenuAreaLayout = false;

            // menu hide
            bool isMenuDock = !MainWindowModel.Current.IsHideMenu && !WindowShape.Current.IsFullScreen;

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

            // メニューレイヤー
            MenuLayerVisibility.SetDelayVisibility(Visibility.Collapsed, 0);
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
            }
            else
            {
                this.DockPageSliderSocket.Content = null;
                this.LayerPageSliderSocket.Content = this.SliderArea;
            }

            // visibility
            if (ContentCanvas.Current.IsMediaContent)
            {
                this.MediaControlView.Visibility = Visibility.Visible;
                this.PageSliderView.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.MediaControlView.Visibility = Visibility.Collapsed;
                this.PageSliderView.Visibility = Visibility.Visible;
            }

            // ステータスレイヤー
            StatusLayerVisibility.SetDelayVisibility(Visibility.Collapsed, 0);
        }

        /// <summary>
        /// フィルムストリップレイアウト更新
        /// </summary>
        private void UpdateThumbnailListLayout()
        {
            if (!_isDartyThumbnailListLayout) return;
            _isDartyThumbnailListLayout = false;

            bool isPageSliderDock = !MainWindowModel.Current.IsHidePageSlider && !WindowShape.Current.IsFullScreen;
            bool isThimbnailListDock = !ThumbnailList.Current.IsHideThumbnailList && isPageSliderDock;

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
            this.ThumbnailListArea.Visibility = ThumbnailList.Current.IsEnableThumbnailList && !ContentCanvas.Current.IsMediaContent ? Visibility.Visible : Visibility.Collapsed;
            this.ThumbnailListArea.DartyThumbnailList();
        }

        #endregion

        #region レイヤー表示状態

        // 初期化
        private void InitializeLayerVisibility()
        {
            InitializeMenuLayerVisibility();
            InitializeStatusLayerVisibility();

            this.Root.MouseMove += Root_MouseMove;
        }


        // ViewAreaでのマウス移動
        private void Root_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateControlsVisibility();
        }

        /// <summary>
        /// レイヤーの表示状態更新
        /// </summary>
        private void UpdateControlsVisibility()
        {
            UpdateMenuLayerVisibility();
            UpdateStatusLayerVisibility();
        }

        /// <summary>
        /// メニューレイヤー表示状態
        /// </summary>
        public DelayVisibility MenuLayerVisibility { get; set; }

        /// <summary>
        /// メニューレイヤーの表示状態管理初期化
        /// </summary>
        private void InitializeMenuLayerVisibility()
        {
            this.MenuLayerVisibility = new DelayVisibility() { DefaultDelayTime = App.Current.AutoHideDelayTime };
            this.MenuLayerVisibility.Changed += (s, e) =>
            {
                this.LayerMenuSocket.Visibility = MenuLayerVisibility.Visibility;
            };

            App.Current.AddPropertyChanged(nameof(App.AutoHideDelayTime), (s, e) =>
            {
                MenuLayerVisibility.DefaultDelayTime = App.Current.AutoHideDelayTime;
            });
        }

        /// <summary>
        /// メニューレイヤーの表示状態更新
        /// </summary>
        private void UpdateMenuLayerVisibility()
        {
            const double visibleMargin = 16;

            if (MainWindowModel.Current.CanHideMenu && !_vm.Model.IsPanelVisibleLocked)
            {
                var point = Mouse.GetPosition(this.Root);
                bool isVisible = this.AddressBar.AddressTextBox.IsFocused || this.LayerMenuSocket.IsMouseOver || point.Y < (MenuLayerVisibility.Visibility == Visibility.Visible ? this.LayerMenuSocket.ActualHeight : 0) + visibleMargin && this.IsMouseOver;
                MenuLayerVisibility.Set(isVisible ? Visibility.Visible : Visibility.Collapsed);
            }
            else
            {
                MenuLayerVisibility.Set(Visibility.Visible);
            }
        }



        /// <summary>
        /// ステータスレイヤー表示状態
        /// </summary>
        public DelayVisibility StatusLayerVisibility { get; set; }

        /// <summary>
        /// ステータスレイヤーの表示状態管理初期化
        /// </summary>
        private void InitializeStatusLayerVisibility()
        {
            this.StatusLayerVisibility = new DelayVisibility() { DefaultDelayTime = App.Current.AutoHideDelayTime };
            this.StatusLayerVisibility.Changed += (s, e) =>
            {
                this.LayerStatusArea.Visibility = StatusLayerVisibility.Visibility;
                if (StatusLayerVisibility.Visibility == Visibility.Visible && this.ThumbnailListArea.IsVisible)
                {
                    this.ThumbnailListArea.UpdateThumbnailList();
                }
            };

            App.Current.AddPropertyChanged(nameof(App.AutoHideDelayTime), (s, e) =>
            {
                StatusLayerVisibility.DefaultDelayTime = App.Current.AutoHideDelayTime;
            });
        }

        /// <summary>
        /// ステータスレイヤーの表示状態更新
        /// </summary>
        private void UpdateStatusLayerVisibility()
        {
            const double visibleMargin = 15;

            if (_vm.Model.IsPanelVisibleLocked)
            {
                StatusLayerVisibility.Set(Visibility.Visible);
            }
            else if (this.LayerStatusArea.Visibility == Visibility.Visible)
            {
                var point = Mouse.GetPosition(this.LayerStatusArea);
                bool isVisible = this.LayerStatusArea.IsFocused || this.LayerStatusArea.IsMouseOver || point.Y > -visibleMargin && this.IsMouseOver;
                StatusLayerVisibility.Set(isVisible ? Visibility.Visible : Visibility.Collapsed);
            }
            else
            {
                var point = Mouse.GetPosition(this.RootBottom);
                bool isVisible = point.Y > -30.0 && this.IsMouseOver;
                StatusLayerVisibility.Set(isVisible ? Visibility.Visible : Visibility.Collapsed);
            }
        }


        #endregion

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
