// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
using System.Windows.Documents;
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
            Current = this;

            InitializeComponent();

            // Window状態初期化、復元
            InitializeWindowShape();

            // Models初期化
            var models = new Models(this);
            models.StartEngine();

            // MainWindow : ViewModel
            _vm = new MainWindowViewModel(models.MainWindowModel);
            this.DataContext = _vm;

            // 各コントロールとモデルを関連付け
            this.SliderArea.Source = models.PageSlider;
            this.SliderArea.FocusTo = this.MainView;
            this.ThumbnailListArea.Source = models.ThumbnailList;
            this.AddressBar.Source = models.AddressBar;
            this.MenuBar.Source = models.MenuBar;
            this.NowLoadingView.Source = models.NowLoading;


            // コマンド初期化
            InitializeCommand();
            InitializeCommandBindings();

            // レイヤー表示管理初期化
            InitializeLayerVisibility();

            //
            models.MainWindowModel.AddPropertyChanged(nameof(MainWindowModel.IsHideMenu),
                (s, e) => DartyMenuAreaLayout());

            models.MainWindowModel.AddPropertyChanged(nameof(MainWindowModel.IsHidePageSlider),
                (s, e) => DartyPageSliderLayout());

            models.MainWindowModel.AddPropertyChanged(nameof(MainWindowModel.IsPanelVisibleLocked),
                (s, e) => UpdateControlsVisibility());

            models.ThumbnailList.AddPropertyChanged(nameof(ThumbnailList.IsEnableThumbnailList),
                (s, e) => DartyThumbnailListLayout());

            models.ThumbnailList.AddPropertyChanged(nameof(ThumbnailList.IsHideThumbnailList),
                (s, e) => DartyThumbnailListLayout());

            models.SidePanel.ResetFocus +=
                (s, e) => ResetFocus();

            this.AddressBar.IsAddressTextBoxFocusedChanged +=
                (s, e) => UpdateMenuLayerVisibility();

            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.IsFullScreen),
                (s, e) => FullScreenChanged());


            // mouse input
            var mouse = MouseInput.Current;

            // mouse drag
            DragActionTable.Current.SetTarget(mouse.Drag);


            // render transform
            var transformView = new TransformGroup();
            transformView.Children.Add(DragTransform.Current.TransformView);
            transformView.Children.Add(mouse.Loupe.TransformView);
            this.MainContent.RenderTransform = transformView;
            this.MainContent.RenderTransformOrigin = new Point(0.5, 0.5);

            var transformCalc = new TransformGroup();
            transformCalc.Children.Add(DragTransform.Current.TransformCalc);
            transformCalc.Children.Add(mouse.Loupe.TransformCalc);
            this.MainContentShadow.RenderTransform = transformCalc;
            this.MainContentShadow.RenderTransformOrigin = new Point(0.5, 0.5);


            // initialize routed commands
            RoutedCommandTable.Current.InitializeInputGestures();

            // mouse event capture for active check
            this.MainView.PreviewMouseMove += MainView_PreviewMouseMove;
            this.MainView.PreviewMouseDown += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseUp += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseWheel += MainView_PreviewMouseAction;

            // timer 
            InitializeNonActiveTimer();


            // moue event for window
            this.PreviewMouseMove += MainWindow_PreviewMouseMove;
            this.PreviewMouseUp += MainWindow_PreviewMouseUp;
            this.PreviewMouseDown += MainWindow_PreviewMouseDown;
            this.PreviewMouseWheel += MainWindow_PreviewMouseWheel;
            this.PreviewStylusDown += MainWindow_PreviewStylusDown;

            // cancel rename triggers
            this.MouseLeftButtonDown += (s, e) => this.RenameManager.Stop();
            this.MouseRightButtonDown += (s, e) => this.RenameManager.Stop();
            this.Deactivated += (s, e) => this.RenameManager.Stop();

            // frame event
            CompositionTarget.Rendering += new EventHandler(OnRendering);

            // 開発用初期化
            Debug_Initialize();
        }

        /// <summary>
        /// Window状態初期化
        /// </summary>
        private void InitializeWindowShape()
        {
            // window
            var windowShape = new WindowShape(this);

            // セカンドプロセスはウィンドウ形状を継承しない
            if (Config.Current.IsSecondProcess) return;

            var memento = SaveData.Current.Setting.WindowShape;
            if (memento == null) return;

            memento = memento.Clone();

            bool isFullScreened = memento.State == WindowStateEx.FullScreen;

            if (App.Current.Option.IsResetPlacement == SwitchOption.on || !App.Current.IsSaveWindowPlacement)
            {
                memento.State = WindowStateEx.Normal;
                memento.WindowRect = Rect.Empty;
            }

            if (isFullScreened)
            {
                memento.State = App.Current.IsSaveFullScreen ? WindowStateEx.FullScreen : WindowStateEx.Normal;
            }

            if (App.Current.Option.IsFullScreen == SwitchOption.on)
            {
                memento.State = WindowStateEx.FullScreen;
            }

            // このタイミングでのChrome適用はMaximizedの場合にフルスクリーンになってしまうので保留する
            if (memento.State != WindowStateEx.Maximized)
            {
                windowShape.WindowChromeFrame = App.Current.WindowChromeFrame;
            }

            windowShape.Restore(memento);
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

            // move page with cursor position
            commandTable[CommandType.MovePageWithCursor].CanExecute =
                () => BookOperation.Current.IsValid;
            commandTable[CommandType.MovePageWithCursor].Execute =
                (s, e) => _vm.MovePageWithCursor(this.MainView);
            commandTable[CommandType.MovePageWithCursor].ExecuteMessage =
                (e) => _vm.MovePageWithCursorMessage(this.MainView);

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
                    this.CommandBindings.Add(new CommandBinding(commands[type], (t, e) => RoutedCommandTable.Current.Execute(type, e.Source, e.Parameter),
                        (t, e) => e.CanExecute = commandTable[type].CanExecute()));
                }
                else
                {
                    this.CommandBindings.Add(new CommandBinding(commands[type], (t, e) => RoutedCommandTable.Current.Execute(type, e.Source, e.Parameter),
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
        private DispatcherTimer _timer;

        // 非アクティブ時間チェック用
        private DateTime _lastActionTime;
        private Point _lastActionPoint;

        // 非アクティブになる時間(秒)
        private const double _activeTimeLimit = 2.0;

        // 一定時間操作がなければカーソルを非表示にする仕組み
        // 初期化
        private void InitializeNonActiveTimer()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(0.2);
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);
            _timer.Start();
        }

        // タイマー処理
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                _lastActionTime = DateTime.Now;
                return;
            }

            // 非アクティブ時間が続いたらマウスカーソルを非表示にする
            if ((DateTime.Now - _lastActionTime).TotalSeconds > _activeTimeLimit)
            {
                SetMouseVisible(false);
                _lastActionTime = DateTime.Now;
            }
        }

        // マウス移動で非アクティブ時間リセット
        private void MainView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var nowPoint = e.GetPosition(this.MainView);

            if (Math.Abs(nowPoint.X - _lastActionPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(nowPoint.Y - _lastActionPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _lastActionTime = DateTime.Now;
                _lastActionPoint = nowPoint;
                SetMouseVisible(true);
            }
        }

        // マウスアクションで非アクティブ時間リセット
        private void MainView_PreviewMouseAction(object sender, MouseEventArgs e)
        {
            ////Debug.WriteLine($"MainWindow:ButtonAction: {e.LeftButton}");

            _lastActionTime = DateTime.Now;
            SetMouseVisible(true);
        }

        // マウスカーソル表示ON/OFF
        public void SetMouseVisible(bool isVisible)
        {
            if (isVisible)
            {
                if (this.MainView.Cursor == Cursors.None && !MouseInput.Current.IsLoupeMode)
                {
                    this.MainView.Cursor = null;
                }
            }
            else
            {
                if (this.MainView.Cursor == null)
                {
                    this.MainView.Cursor = Cursors.None;
                }
            }
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


        // ウィンドウ表示開始
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 一瞬手前に表示
            WindowShape.Current.OneTopmost();

            // レイアウト更新
            DartyWindowLayout();

            // WinProc登録
            ContentRebuild.Current.InitinalizeWinProc(this);

            _vm.Loaded();
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
            MouseInput.Current.Drag.SnapView();
        }


        // 
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 単キーのショートカットを有効にする。
            // TextBoxなどのイベント処理でこのフラグをfalseにすることで短キーのショートカットを無効にして入力を優先させる
            KeyExGesture.AllowSingleKey = true;

            // 自動非表示ロック解除
            _vm.Model.LeaveVisibleLocked();
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

            // ウィンドウサイズのDPI非追従
            if (App.Current.IsIgnoreWindowDpi && this.WindowState == WindowState.Normal)
            {
                var newWidth = Math.Floor(this.Width * e.OldDpi.DpiScaleX / e.NewDpi.DpiScaleX);
                var newHeight = Math.Floor(this.Height * e.OldDpi.DpiScaleY / e.NewDpi.DpiScaleY);

                // 反映タイミングをずらす
                App.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Width = newWidth;
                    this.Height = newHeight;
                }));
            }
        }

        //
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 閉じる前にウィンドウサイズ保存
            WindowShape.Current.CreateSnapMemento();
        }

        //
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            //
            Models.Current.StopEngine();

            // タイマー停止
            _timer.Stop();

            // 設定保存
            SaveData.Current.SaveSetting();

            // テンポラリファイル破棄
            Temporary.RemoveTempFolder();

            // キャッシュDBを閉じる
            ThumbnailCache.Current.Dispose();

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
            this.Dispatcher.BeginInvoke((Action)(() => this.MainView.Focus()));
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
            _isDartyThumbnailListLayout = true; // サムネイルリストも更新
        }

        /// <summary>
        /// サムネイルリスト更新要求
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
        /// メニューエリアレイアウト苦心
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
            bool isPageSliderDock = !MainWindowModel.Current.IsHidePageSlider && !WindowShape.Current.IsFullScreen;

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


            // ステータスレイヤー
            StatusLayerVisibility.SetDelayVisibility(Visibility.Collapsed, 0);
        }

        /// <summary>
        /// サムネイルリストレイアウト更新
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

            // サムネイルリスト
            this.ThumbnailListArea.Visibility = ThumbnailList.Current.IsEnableThumbnailList ? Visibility.Visible : Visibility.Collapsed;
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

        // [開発用] 設定初期化
        [Conditional("DEBUG")]
        private void Debug_Initialize()
        {
            this.RootDockPanel.Children.Insert(0, new DevPageList());
            this.RootDockPanel.Children.Insert(1, new DevInfo());

            this.PreviewKeyDown += Debug_PreviewKeyDown;
        }

        // [開発用] 開発操作
        private void Debug_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F12)
            {
                Debug_CheckFocus();
            }
        }

        // [開発用] 現在のフォーカスを取得
        private void Debug_CheckFocus()
        {
            var element = FocusManager.GetFocusedElement(this);
            var fwelement = element as FrameworkElement;
            Debug.WriteLine($"FOCUS: {element}({element?.GetType()})({fwelement?.Name})");
        }

        #endregion

    }

}
