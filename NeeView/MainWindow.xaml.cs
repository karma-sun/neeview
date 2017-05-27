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
        private MainWindowViewModel _vm;


        /// <summary>
        /// コンストラクター
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            // Preferenceの復元は最優先
            Preference.Current.Restore(App.Setting.PreferenceMemento);

            // Window状態初期化、復元
            InitializeWindowShape();

            // Models初期化
            var models = new Models(this);

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
            InitializeCommandBindings();

            // レイヤー表示管理初期化
            InitializeLayerVisibility();


            // TODO: 定義場所の変更を検討
            App.Config.LocalApplicationDataRemoved +=
                (s, e) =>
                {
                    SaveData.Current.IsEnableSave = false; // 保存禁止
                    this.Close();
                };

            // コマンド変更でショートカット変更
            models.CommandTable.Changed +=
                (s, e) => InitializeInputGestures();

            //
            models.MainWindowModel.AddPropertyChanged(nameof(MainWindowModel.IsHideMenu),
                (s, e) => UpdateMenuAreaLayout());

            models.MainWindowModel.AddPropertyChanged(nameof(MainWindowModel.IsHidePageSlider),
                (s, e) => UpdateMenuAreaLayout());

            models.ThumbnailList.AddPropertyChanged(nameof(ThumbnailList.IsEnableThumbnailList),
                (s, e) => UpdateThumbnailListLayout());

            models.ThumbnailList.AddPropertyChanged(nameof(ThumbnailList.IsHideThumbnailList),
                (s, e) => UpdateThumbnailListLayout());

            models.SidePanel.ResetFocus +=
                (s, e) => this.MainView.Focus();

            this.AddressBar.IsAddressTextBoxFocusedChanged +=
                (s, e) => UpdateMenuLayerVisibility();


            // mouse input
            var mouse = MouseInput.Current;

            // mouse drag
            DragActionTable.Current.SetTarget(mouse.Drag);


            // render transform
            var transformView = new TransformGroup();
            transformView.Children.Add(mouse.Drag.TransformView);
            transformView.Children.Add(mouse.Loupe.TransformView);
            this.MainContent.RenderTransform = transformView;
            this.MainContent.RenderTransformOrigin = new Point(0.5, 0.5);

            var transformCalc = new TransformGroup();
            transformCalc.Children.Add(mouse.Drag.TransformCalc);
            transformCalc.Children.Add(mouse.Loupe.TransformCalc);
            this.MainContentShadow.RenderTransform = transformCalc;
            this.MainContentShadow.RenderTransformOrigin = new Point(0.5, 0.5);


            // initialize routed commands
            InitializeInputGestures();

            // mouse event capture for active check
            this.MainView.PreviewMouseMove += MainView_PreviewMouseMove;
            this.MainView.PreviewMouseDown += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseUp += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseWheel += MainView_PreviewMouseAction;

            // timer 
            InitializeNonActiveTimer();


            // moue event for window shape
            this.PreviewMouseMove += MainWindow_PreviewMouseMove;

            // cancel rename triggers
            this.MouseLeftButtonDown += (s, e) => this.RenameManager.Stop();
            this.MouseRightButtonDown += (s, e) => this.RenameManager.Stop();
            this.Deactivated += (s, e) => this.RenameManager.Stop();

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

            var memento = App.Setting.WindowShape;
            if (memento == null) return;

            memento = memento.Clone();

            if (App.Options["--reset-placement"].IsValid || !App.Current.IsSaveWindowPlacement)
            {
                memento.State = WindowStateEx.Normal;
                memento.WindowRect = Rect.Empty;
            }

            if (memento.State == WindowStateEx.FullScreen && !App.Current.IsSaveFullScreen)
            {
                memento.State = WindowStateEx.Normal;
            }

            if (App.Options["--fullscreen"].IsValid)
            {
                memento.State = WindowStateEx.FullScreen;
            }

            // このタイミングでのChrome適用はMaximizedの場合にフルスクリーンになってしまうので保留する
            if (memento.State != WindowStateEx.Maximized)
            {
                windowShape.WindowChromeFrame = Preference.Current.window_chrome_frame;
            }

            windowShape.Restore(memento);

            //
            windowShape.AddPropertyChanged(nameof(WindowShape.IsFullScreen),
                (s, e) => UpdateWindowLayout());
        }


        #region コマンドバインディング

        // RoutedCommand バインディング
        public void InitializeCommandBindings()
        {
            var mouse = MouseInput.Current;
            var commandTable = CommandTable.Current;

            // View系コマンド登録
            commandTable[CommandType.CloseApplication].Execute =
                (s, e) => this.Close();
            commandTable[CommandType.ToggleWindowMinimize].Execute =
                (s, e) => MainWindow_Minimize();
            commandTable[CommandType.ToggleWindowMaximize].Execute =
                (s, e) => MainWindow_Maximize();


            // mouse系
            // TODO : Mouse系はMouseInputManagerで処理できないか？
            commandTable[CommandType.ViewScrollUp].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScrollCommandParameter)commandTable[CommandType.ViewScrollUp].Parameter;
                    mouse.Drag.ScrollUp(parameter.Scroll / 100.0);
                };
            commandTable[CommandType.ViewScrollDown].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScrollCommandParameter)commandTable[CommandType.ViewScrollDown].Parameter;
                    mouse.Drag.ScrollDown(parameter.Scroll / 100.0);
                };
            commandTable[CommandType.ViewScaleUp].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScaleCommandParameter)commandTable[CommandType.ViewScaleUp].Parameter;
                    mouse.Drag.ScaleUp(parameter.Scale / 100.0);
                };
            commandTable[CommandType.ViewScaleDown].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScaleCommandParameter)commandTable[CommandType.ViewScaleDown].Parameter;
                    mouse.Drag.ScaleDown(parameter.Scale / 100.0);
                };
            commandTable[CommandType.ViewRotateLeft].Execute =
                (s, e) =>
                {
                    var parameter = (ViewRotateCommandParameter)commandTable[CommandType.ViewRotateLeft].Parameter;
                    if (parameter.IsStretch) mouse.Drag.ResetDefault();
                    mouse.Drag.Rotate(-parameter.Angle);
                    if (parameter.IsStretch) ContentCanvas.Current.UpdateContentSize(mouse.Drag.Angle);
                };
            commandTable[CommandType.ViewRotateRight].Execute =
                (s, e) =>
                {
                    var parameter = (ViewRotateCommandParameter)commandTable[CommandType.ViewRotateRight].Parameter;
                    if (parameter.IsStretch) mouse.Drag.ResetDefault();
                    mouse.Drag.Rotate(+parameter.Angle);
                    if (parameter.IsStretch) ContentCanvas.Current.UpdateContentSize(mouse.Drag.Angle);
                };
            commandTable[CommandType.ToggleViewFlipHorizontal].Execute =
                (s, e) => mouse.Drag.ToggleFlipHorizontal();
            commandTable[CommandType.ViewFlipHorizontalOn].Execute =
                (s, e) => mouse.Drag.FlipHorizontal(true);
            commandTable[CommandType.ViewFlipHorizontalOff].Execute =
                (s, e) => mouse.Drag.FlipHorizontal(false);

            commandTable[CommandType.ToggleViewFlipVertical].Execute =
                (s, e) => mouse.Drag.ToggleFlipVertical();
            commandTable[CommandType.ViewFlipVerticalOn].Execute =
                (s, e) => mouse.Drag.FlipVertical(true);
            commandTable[CommandType.ViewFlipVerticalOff].Execute =
                (s, e) => mouse.Drag.FlipVertical(false);

            commandTable[CommandType.ViewReset].Execute =
                (s, e) => ContentCanvas.Current.ResetTransform(true);

            commandTable[CommandType.MovePageWithCursor].CanExecute =
                () => BookOperation.Current.IsValid;
            commandTable[CommandType.MovePageWithCursor].Execute =
                (s, e) => _vm.MovePageWithCursor(this.MainView);
            commandTable[CommandType.MovePageWithCursor].ExecuteMessage =
                (e) => _vm.MovePageWithCursorMessage(this.MainView);

            // loupe
            commandTable[CommandType.ToggleIsLoupe].Execute =
                (s, e) => mouse.IsLoupeMode = !mouse.IsLoupeMode;
            commandTable[CommandType.ToggleIsLoupe].ExecuteMessage =
                e => mouse.IsLoupeMode ? "ルーペOFF" : "ルーペON";
            commandTable[CommandType.ToggleIsLoupe].CreateIsCheckedBinding =
                () => new Binding(nameof(mouse.IsLoupeMode)) { Mode = BindingMode.OneWay, Source = mouse };
            commandTable[CommandType.LoupeOn].Execute =
                (s, e) => mouse.IsLoupeMode = true;
            commandTable[CommandType.LoupeOff].Execute =
                (s, e) => mouse.IsLoupeMode = false;

            // print
            commandTable[CommandType.Print].Execute =
                (s, e) => ContentCanvas.Current.Print(this, this.PageContents, this.MainContent.RenderTransform, this.MainView.ActualWidth, this.MainView.ActualHeight);

            // context menu
            commandTable[CommandType.OpenContextMenu].Execute =
                (s, e) =>
                {
                    if (this.MainViewPanel.ContextMenu != null)
                    {
                        this.MainViewPanel.ContextMenu.DataContext = _vm;
                        this.MainViewPanel.ContextMenu.PlacementTarget = this.MainViewPanel;
                        this.MainViewPanel.ContextMenu.Placement = PlacementMode.MousePoint;
                        this.MainViewPanel.ContextMenu.IsOpen = true;
                    }
                };

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

        // InputGesture設定
        // TODO: MouseInputManagerで処理する？
        public void InitializeInputGestures()
        {
            var mouse = MouseInput.Current;

            mouse.ClearMouseEventHandler();

            mouse.Commands.Clear();
            mouse.MouseGestureChanged += (s, x) => mouse.Commands.Execute(x.Sequence);

            var mouseNormalHandlers = new List<EventHandler<MouseButtonEventArgs>>();
            var mouseExtraHndlers = new List<EventHandler<MouseButtonEventArgs>>();

            foreach (var e in RoutedCommandTable.Current.Commands)
            {
                e.Value.InputGestures.Clear();
                var inputGestures = CommandTable.Current[e.Key].GetInputGestureCollection();
                foreach (var gesture in inputGestures)
                {
                    if (gesture is MouseGesture mouseClick)
                    {
                        mouseNormalHandlers.Add((s, x) => { if (!x.Handled && gesture.Matches(this, x)) { e.Value.Execute(null, this); x.Handled = true; } });
                    }
                    else if (gesture is MouseExGesture)
                    {
                        mouseExtraHndlers.Add((s, x) => { if (!x.Handled && gesture.Matches(this, x)) { e.Value.Execute(null, this); x.Handled = true; } });
                    }
                    else if (gesture is MouseWheelGesture)
                    {
                        mouse.MouseWheelChanged += (s, x) => { if (!x.Handled && gesture.Matches(this, x)) { WheelCommandExecute(e.Value, x); } };
                    }
                    else
                    {
                        e.Value.InputGestures.Add(gesture);
                    }
                }

                // mouse gesture
                var mouseGesture = CommandTable.Current[e.Key].MouseGesture;
                if (mouseGesture != null)
                {
                    mouse.Commands.Add(mouseGesture, e.Value);
                }
            }

            // 拡張マウス入力から先に処理を行う
            foreach (var lambda in mouseExtraHndlers.Concat(mouseNormalHandlers))
            {
                mouse.MouseButtonChanged += lambda;
            }

            // Update Menu GestureText
            // TODO: このあたりイベントで処理できないか？
            NeeView.MenuBar.Current.Reflesh();
            _vm.ContextMenu?.UpdateInputGestureText();
        }


        /// <summary>
        /// wheel command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <param name="target"></param>
        /// <param name="arg"></param>
        private void WheelCommandExecute(RoutedUICommand command, MouseWheelEventArgs arg)
        {
            int turn = MouseInputHelper.DeltaCount(arg);

            // Debug.WriteLine($"WheelCommand: {turn}({arg.Delta})");

            for (int i = 0; i < turn; i++)
            {
                command.Execute(null, this);
            }
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


        #region WindowStateCommand

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


        // ウィンドウ表示開始
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // レイアウト更新
            UpdateWindowLayout();

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
            var isChanged = App.Config.SetDip(e.NewDpi);
            if (!isChanged) return;

            //
            this.MenuBar.WindowCaptionButtons.UpdateStrokeThickness(e.NewDpi);

            // 背景更新
            ContentCanvasBrush.Current.UpdateBackgroundBrush();

            // Window Border
            WindowShape.Current?.UpdateWindowBorderThickness();

            // ウィンドウサイズのDPI非追従
            if (Preference.Current.dpi_window_ignore && this.WindowState == WindowState.Normal)
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



        #region レイアウト管理

        /// <summary>
        /// レイアウト更新
        /// </summary>
        private void UpdateWindowLayout()
        {
            UpdateMenuAreaLayout();
            UpdateStatusAreaLayout();
        }

        //
        private void UpdateMenuAreaLayout()
        {
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
        /// ステータスエリアレイアウト更新
        /// </summary>
        private void UpdateStatusAreaLayout()
        {
            UpdatePageSliderLayout();
            UpdateThumbnailListLayout();
        }

        /// <summary>
        /// ページスライダーレイアウト更新。
        /// ここの状態が変化する場合、サムネイルリストも更新の必要あり。
        /// </summary>
        private void UpdatePageSliderLayout()
        {
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

        //
        private void UpdateThumbnailListLayout()
        {
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
            this.MenuLayerVisibility = new DelayVisibility();
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

            if (MainWindowModel.Current.CanHideMenu)
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
            this.StatusLayerVisibility = new DelayVisibility();
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
            const double visibleMargin = 16;

            if (this.LayerStatusArea.Visibility == Visibility.Visible)
            {
                var point = Mouse.GetPosition(this.LayerStatusArea);
                bool isVisible = this.LayerStatusArea.IsFocused || this.LayerStatusArea.IsMouseOver || point.Y > -visibleMargin && this.IsMouseOver;
                StatusLayerVisibility.Set(isVisible ? Visibility.Visible : Visibility.Collapsed);
            }
            else
            {
                var point = Mouse.GetPosition(this.RootBottom);
                bool isVisible = point.Y > -visibleMargin && this.IsMouseOver;
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
