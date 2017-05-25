﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Globalization;
using NeeView.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// TODO: 機能の細分化 (UserControl?)
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _vm;

        private MouseInputManager _mouse;

        private ContentDropManager _contentDrop = new ContentDropManager();

        private bool _nowLoading = false;


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

        }


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

            this.PreviewMouseMove += MainWindow_PreviewMouseMove;


            // System Models
            Models.Instantiate();

            var models = Models.Current;

            // ViewModel
            _vm = new MainWindowViewModel(models.MainWindowModel);
            this.DataContext = _vm;

            this.SliderArea.Source = models.PageSlider;
            this.SliderArea.FocusTo = this.MainView;

            this.ThumbnailListArea.Source = models.ThumbnailList;

            this.AddressBar.Source = models.AddressBar;

            this.MenuBar.Source = models.MenuBar;

            this.NowLoading.Source = models.NowLoading;


            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.IsFullScreen),
                (s, e) => UpdateWindowLayout());



            // コマンド初期化
            InitializeCommandBindings();

            InitializeMenuLayerVisibility();
            InitializeStatusLayerVisibility();


#if DEBUG
            this.RootDockPanel.Children.Insert(0, new DevPageList());
            this.RootDockPanel.Children.Insert(1, new DevInfo());

            this.PreviewKeyDown += Debug_PreviewKeyDown;
#endif


            App.Config.LocalApplicationDataRemoved +=
                (s, e) =>
                {
                    SaveData.Current.IsEnableSave = false; // 保存禁止
                    this.Close();
                };

            InitializeVisualTree();

            // mouse input
            models.MouseInput.Initialize(this, this.MainView, this.MainContent, this.MainContentShadow);
            _mouse = new MouseInputManager(models.MouseInput);
            ContentCanvasTransform.Current.SetMouseInputDrag(_mouse.Drag); // TODO: 応急処置

            this.LoupeInfo.DataContext = _mouse.Loupe;

            // mouse gesture
            _mouse.Gesture.MouseGestureProgressed += OnMouseGestureUpdate;

            // mouse drag
            DragActionTable.Current.SetTarget(_mouse.Drag);


            // render transform
            var transformView = new TransformGroup();
            transformView.Children.Add(_mouse.Drag.TransformView);
            transformView.Children.Add(_mouse.Loupe.TransformView);
            this.MainContent.RenderTransform = transformView;
            this.MainContent.RenderTransformOrigin = new Point(0.5, 0.5);

            var transformCalc = new TransformGroup();
            transformCalc.Children.Add(_mouse.Drag.TransformCalc);
            transformCalc.Children.Add(_mouse.Loupe.TransformCalc);
            this.MainContentShadow.RenderTransform = transformCalc;
            this.MainContentShadow.RenderTransformOrigin = new Point(0.5, 0.5);



            // initialize routed commands
            //InitializeCommandBindings();
            InitializeInputGestures();

            // VM NotifyPropertyChanged Hook



            // messenger
            Messenger.AddReciever("Export", CallExport);

            // mouse event capture for active check
            this.MainView.PreviewMouseMove += MainView_PreviewMouseMove;
            this.MainView.PreviewMouseDown += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseUp += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseWheel += MainView_PreviewMouseAction;

            // timer 
            InitializeNonActiveTimer();

            // cancel rename triggers
            this.MouseLeftButtonDown += (s, e) => this.RenameManager.Stop();
            this.MouseRightButtonDown += (s, e) => this.RenameManager.Stop();
            this.Deactivated += (s, e) => this.RenameManager.Stop();
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


        // ビジュアル初期化
        private void InitializeVisualTree()
        {
            // IsMouseOverの変更イベントをハンドルする。
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(Grid));
            dpd.AddValueChanged(this.MenuArea, MenuArea_IsMouseOverChanged);


            // IsFocusedの変更イベントをハンドルする。
            this.AddressBar.IsAddressTextBoxFocusedChanged += (s, e) => UpdateMenuLayerVisibility();
        }

        // Preference適用
        public void ApplyPreference(Preference preference)
        {
            // マウスジェスチャーの最小移動距離
            _mouse.Gesture.SetGestureMinimumDistance(
                preference.input_gesture_minimumdistance_x,
                preference.input_gesture_minimumdistance_y);
        }

        //
        private void MenuArea_IsMouseOverChanged(object sender, EventArgs e)
        {
            UpdateMenuLayerVisibility();
        }


        //
        private void InitializeViewModelEvents()
        {
            var models = Models.Current;

            models.CommandTable.Changed +=
                (s, e) => InitializeInputGestures();

            models.MainWindowModel.AddPropertyChanged(nameof(MainWindowModel.IsHideMenu),
                (s, e) => UpdateMenuAreaLayout());

            models.MainWindowModel.AddPropertyChanged(nameof(MainWindowModel.IsHidePageSlider),
                (s, e) => UpdateMenuAreaLayout());

            models.ThumbnailList.AddPropertyChanged(nameof(ThumbnailList.IsEnableThumbnailList),
                (s, e) => UpdateThumbnailListLayout());

            models.ThumbnailList.AddPropertyChanged(nameof(ThumbnailList.IsHideThumbnailList),
                (s, e) => UpdateThumbnailListLayout());


            SidePanel.Current.ResetFocus +=
                (s, e) => this.MainView.Focus();

            //
            BookOperation.Current.AddPropertyChanged(nameof(BookOperation.PageList), OnPageListChanged);
        }


        // TODO: 直接のThumbnailListArea操作はよくない。モデル経由で。
        private void OnPageListChanged(object sender, EventArgs e)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                this.ThumbnailListArea.OnPageListChanged();
            });
        }


        #region NonActiveTimer

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
                if (this.MainView.Cursor == Cursors.None && !_mouse.IsLoupeMode)
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


        // マウスジェスチャー更新時の処理
        private void OnMouseGestureUpdate(object sender, MouseGestureEventArgs e)
        {
            _vm.ShowGesture(e.Sequence.ToDispString(), _mouseGestureCommandCollection?.GetCommand(e.Sequence)?.Text);
        }



        // RoutedCommand辞書
        public Dictionary<CommandType, RoutedUICommand> BookCommands => RoutedCommandTable.Current.Commands;

        // RoutedCommand バインディング
        public void InitializeCommandBindings()
        {
            var commandTable = CommandTable.Current;

            // View系コマンド登録
            commandTable[CommandType.OpenSettingWindow].Execute =
                (s, e) => OpenSettingWindow();
            commandTable[CommandType.OpenSettingFilesFolder].Execute =
                (s, e) => OpenSettingFilesFolder();
            commandTable[CommandType.OpenVersionWindow].Execute =
                (s, e) => OpenVersionWindow();
            commandTable[CommandType.CloseApplication].Execute =
                (s, e) => Close();
            commandTable[CommandType.ToggleWindowMinimize].Execute =
                (s, e) => MainWindow_Minimize();
            commandTable[CommandType.ToggleWindowMaximize].Execute =
                (s, e) => MainWindow_Maximize();
            commandTable[CommandType.LoadAs].Execute =
                (s, e) => LoadAs(e);
            commandTable[CommandType.Paste].Execute =
                (s, e) => LoadFromClipboard();
            commandTable[CommandType.Paste].CanExecute =
                () => CanLoadFromClipboard();
            commandTable[CommandType.ViewScrollUp].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScrollCommandParameter)commandTable[CommandType.ViewScrollUp].Parameter;
                    _mouse.Drag.ScrollUp(parameter.Scroll / 100.0);
                };
            commandTable[CommandType.ViewScrollDown].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScrollCommandParameter)commandTable[CommandType.ViewScrollDown].Parameter;
                    _mouse.Drag.ScrollDown(parameter.Scroll / 100.0);
                };
            commandTable[CommandType.ViewScaleUp].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScaleCommandParameter)commandTable[CommandType.ViewScaleUp].Parameter;
                    _mouse.Drag.ScaleUp(parameter.Scale / 100.0);
                };
            commandTable[CommandType.ViewScaleDown].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScaleCommandParameter)commandTable[CommandType.ViewScaleDown].Parameter;
                    _mouse.Drag.ScaleDown(parameter.Scale / 100.0);
                };
            commandTable[CommandType.ViewRotateLeft].Execute =
                (s, e) =>
                {
                    var parameter = (ViewRotateCommandParameter)commandTable[CommandType.ViewRotateLeft].Parameter;
                    if (parameter.IsStretch) _mouse.Drag.ResetDefault();
                    _mouse.Drag.Rotate(-parameter.Angle);
                    if (parameter.IsStretch) ContentCanvas.Current.UpdateContentSize(_mouse.Drag.Angle);
                };
            commandTable[CommandType.ViewRotateRight].Execute =
                (s, e) =>
                {
                    var parameter = (ViewRotateCommandParameter)commandTable[CommandType.ViewRotateRight].Parameter;
                    if (parameter.IsStretch) _mouse.Drag.ResetDefault();
                    _mouse.Drag.Rotate(+parameter.Angle);
                    if (parameter.IsStretch) ContentCanvas.Current.UpdateContentSize(_mouse.Drag.Angle);
                };
            commandTable[CommandType.ToggleViewFlipHorizontal].Execute =
                (s, e) => _mouse.Drag.ToggleFlipHorizontal();
            commandTable[CommandType.ViewFlipHorizontalOn].Execute =
                (s, e) => _mouse.Drag.FlipHorizontal(true);
            commandTable[CommandType.ViewFlipHorizontalOff].Execute =
                (s, e) => _mouse.Drag.FlipHorizontal(false);

            commandTable[CommandType.ToggleViewFlipVertical].Execute =
                (s, e) => _mouse.Drag.ToggleFlipVertical();
            commandTable[CommandType.ViewFlipVerticalOn].Execute =
                (s, e) => _mouse.Drag.FlipVertical(true);
            commandTable[CommandType.ViewFlipVerticalOff].Execute =
                (s, e) => _mouse.Drag.FlipVertical(false);

            commandTable[CommandType.ViewReset].Execute =
                (s, e) => ContentCanvas.Current.ResetTransform(true);

            commandTable[CommandType.PrevScrollPage].Execute =
                (s, e) => PrevScrollPage();
            commandTable[CommandType.NextScrollPage].Execute =
                (s, e) => NextScrollPage();
            commandTable[CommandType.MovePageWithCursor].Execute =
                (s, e) => MovePageWithCursor();
            commandTable[CommandType.MovePageWithCursor].ExecuteMessage =
                (e) => MovePageWithCursorMessage();

            commandTable[CommandType.ToggleIsLoupe].Execute =
                (s, e) => _mouse.IsLoupeMode = !_mouse.IsLoupeMode;
            commandTable[CommandType.ToggleIsLoupe].ExecuteMessage =
                e => _mouse.IsLoupeMode ? "ルーペOFF" : "ルーペON";
            commandTable[CommandType.ToggleIsLoupe].CreateIsCheckedBinding =
                () => new Binding(nameof(_mouse.IsLoupeMode)) { Mode = BindingMode.OneWay, Source = _mouse };
            commandTable[CommandType.LoupeOn].Execute =
                (s, e) => _mouse.IsLoupeMode = true;
            commandTable[CommandType.LoupeOff].Execute =
                (s, e) => _mouse.IsLoupeMode = false;

            commandTable[CommandType.Print].Execute =
                (s, e) =>
                {
                    ContentCanvas.Current.Print(this, this.PageContents, this.MainContent.RenderTransform, this.MainView.ActualWidth, this.MainView.ActualHeight);
                };

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

            // コマンドバインド作成
            foreach (CommandType type in Enum.GetValues(typeof(CommandType)))
            {
                if (commandTable[type].CanExecute != null)
                {
                    this.CommandBindings.Add(new CommandBinding(BookCommands[type], (t, e) => RoutedCommandTable.Current.Execute(type, e.Source, e.Parameter),
                        (t, e) => e.CanExecute = commandTable[type].CanExecute()));
                }
                else
                {
                    this.CommandBindings.Add(new CommandBinding(BookCommands[type], (t, e) => RoutedCommandTable.Current.Execute(type, e.Source, e.Parameter),
                        CanExecute));
                }
            }
        }

        // ロード中のコマンドを無効にする CanExecute
        private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_nowLoading;
        }


        //
        private MouseGestureCommandCollection _mouseGestureCommandCollection = new MouseGestureCommandCollection();

        delegate void MouseButtonEventDelegate(object sender, MouseButtonEventArgs e);

        // InputGesture設定
        public void InitializeInputGestures()
        {
            _mouse.ClearMouseEventHandler();

            _mouseGestureCommandCollection.Clear();
            _mouse.MouseGestureChanged += (s, x) => _mouseGestureCommandCollection.Execute(x.Sequence);

            var mouseNormalHandlers = new List<EventHandler<MouseButtonEventArgs>>();
            var mouseExtraHndlers = new List<EventHandler<MouseButtonEventArgs>>();

            foreach (var e in BookCommands)
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
                        _mouse.MouseWheelChanged += (s, x) => { if (!x.Handled && gesture.Matches(this, x)) { WheelCommandExecute(e.Value, x); } };
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
                    _mouseGestureCommandCollection.Add(mouseGesture, e.Value);
                }
            }

            // 拡張マウス入力から先に処理を行う
            foreach (var lambda in mouseExtraHndlers.Concat(mouseNormalHandlers))
            {
                _mouse.MouseButtonChanged += lambda;
            }

            // Update Menu GestureText
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



        // ダイアログでファイル選択して画像を読み込む
        private void LoadAs(object param)
        {
            string path = param as string;

            if (path == null)
            {
                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = BookHub.Current.GetDefaultFolder();

                if (dialog.ShowDialog(this) == true)
                {
                    path = dialog.FileName;
                }
                else
                {
                    return;
                }
            }

            BookHub.Current.Load(path);
        }


        // スクロール＋前のページに戻る
        private void PrevScrollPage()
        {
            var parameter = (ScrollPageCommandParameter)CommandTable.Current[CommandType.PrevScrollPage].Parameter;

            int bookReadDirection = (BookHub.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = _mouse.Drag.ScrollN(-1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation);

            if (!isScrolled)
            {
                ContentCanvas.Current.NextViewOrigin = (BookHub.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightBottom : DragViewOrigin.LeftBottom;
                BookOperation.Current.PrevPage();
            }
        }

        // スクロール＋次のページに進む
        private void NextScrollPage()
        {
            var parameter = (ScrollPageCommandParameter)CommandTable.Current[CommandType.NextScrollPage].Parameter;

            int bookReadDirection = (BookHub.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = _mouse.Drag.ScrollN(+1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation);

            if (!isScrolled)
            {
                ContentCanvas.Current.NextViewOrigin = (BookHub.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightTop : DragViewOrigin.LeftTop;
                BookOperation.Current.NextPage();
            }
        }


        // マウスの位置でページを送る
        private void MovePageWithCursor()
        {
            var point = Mouse.GetPosition(this.MainView);

            if (point.X < this.MainView.ActualWidth * 0.5)
            {
                BookOperation.Current.NextPage();
            }
            else
            {
                BookOperation.Current.PrevPage();
            }
        }

        // マウスの位置でページを送る(メッセージ)
        private string MovePageWithCursorMessage()
        {
            var point = Mouse.GetPosition(this.MainView);

            if (point.X < this.MainView.ActualWidth * 0.5)
            {
                return "次のページ";
            }
            else
            {
                return "前のページ";
            }
        }


        // 設定ウィンドウを開く
        private void OpenSettingWindow()
        {
            var setting = SaveData.Current.CreateSetting();
            var history = BookHistory.Current.CreateMemento(false);

            // スライドショー停止
            SlideShow.Current.PauseSlideShow();

            var dialog = new SettingWindow(setting, history);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                SaveData.Current.RestoreSetting(setting, false);
                WindowShape.Current.CreateSnapMemento();
                SaveData.Current.SaveSetting();
                BookHistory.Current.Restore(history, false);

                // 現在ページ再読込
                BookHub.Current.ReLoad();
            }

            // スライドショー再開
            SlideShow.Current.ResumeSlideShow();
        }

        // 設定ファイルの場所を開く
        private void OpenSettingFilesFolder()
        {
            Process.Start("explorer.exe", $"\"{App.Config.LocalApplicationDataPath}\"");
        }

        // バージョン情報を表示する
        private void OpenVersionWindow()
        {
            var dialog = new VersionWindow();
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }


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

        //
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




        //
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Chrome反映
            WindowShape.Current.WindowChromeFrame = Preference.Current.window_chrome_frame;

            // VMイベント設定
            InitializeViewModelEvents();

            // 設定反映
            SaveData.Current.RestoreSetting(App.Setting, true);

            // 履歴読み込み
            SaveData.Current.LoadHistory(App.Setting);

            // ブックマーク読み込み
            SaveData.Current.LoadBookmark(App.Setting);

            // ページマーク読込
            SaveData.Current.LoadPagemark(App.Setting);

            App.Setting = null; // ロード設定破棄


            // フォルダーを開く
            if (!App.Options["--blank"].IsValid)
            {
                if (App.StartupPlace != null)
                {
                    // 起動引数の場所で開く
                    LoadAs(App.StartupPlace);
                }
                else
                {
                    // 最後に開いたフォルダーを復元する
                    _vm.LoadLastFolder();
                }
            }

            // スライドショーの自動再生
            if (App.Options["--slideshow"].IsValid ? App.Options["--slideshow"].Bool : SlideShow.Current.IsAutoPlaySlideShow)
            {
                SlideShow.Current.IsPlayingSlideShow = true;
            }
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
            _mouse.IsLoupeMode = false;
        }



        // ドラッグ＆ドロップ前処理
        private void MainWindow_DragOver(object sender, DragEventArgs e)
        {
            if (!_nowLoading && _contentDrop.CheckDragContent(sender, e.Data))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        // ドラッグ＆ドロップで処理を開始する
        private async void MainWindow_Drop(object sender, DragEventArgs e)
        {
            await LoadDataObjectAsync(sender, e.Data);
        }

        // コピー＆ペーストできる？
        private bool CanLoadFromClipboard()
        {
            var data = Clipboard.GetDataObject();
            return data != null ? !_nowLoading && _contentDrop.CheckDragContent(this, data) : false;
        }

        // コピー＆ペーストで処理を開始する
        private async void LoadFromClipboard()
        {
            await LoadDataObjectAsync(this, Clipboard.GetDataObject());
        }


        // データオブジェクトからのロード処理
        private async Task LoadDataObjectAsync(object sender, IDataObject data)
        {
            if (_nowLoading || data == null) return;

            try
            {
                var downloadPath = string.IsNullOrWhiteSpace(Preference.Current.download_path) ? Temporary.TempDownloadDirectory : Preference.Current.download_path;
                string path = await _contentDrop.DropAsync(this, data, downloadPath, (string message) => NeeView.NowLoading.Current.SetLoading(message));
                BookHub.Current.Load(path);
            }
            catch (Exception ex)
            {
                BookHub.Current.RequestUnload(true, ex.Message ?? "コンテンツの読み込みに失敗しました");
                NeeView.NowLoading.Current.ResetLoading();
            }
        }


        // ウィンドウサイズが変化したらコンテンツサイズも追従する
        private void MainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContentCanvas.Current.SetViewSize(this.MainView.ActualWidth, this.MainView.ActualHeight);

            // スナップ
            _mouse.Drag.SnapView();
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

            // スレッド終了処理 
            // 主にコマンドのキャンセル処理。 特にApp.Current にアクセスするものはキャンセルさせる
            ////_VM.Dispose();

            // テンポラリファイル破棄
            Temporary.RemoveTempFolder();

            // キャッシュDBを閉じる
            ThumbnailCache.Current.Dispose();

            Debug.WriteLine("Window.Closed done.");
            //Environment.Exit(0);
        }


        // メッセージ処理：ファイル出力
        private void CallExport(object sender, MessageEventArgs e)
        {
            var exporter = (Exporter)e.Parameter;
            exporter.BackgroundBrush = ContentCanvasBrush.Current.BackgroundBrush;

            var dialog = new SaveWindow(exporter);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            e.Result = (result == true);
        }


        // オンラインヘルプ
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }


        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        // 全てのルーテッドイベントの開始
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 単キーのショートカット有効
            KeyExGesture.AllowSingleKey = true;
        }


        #region DEBUG
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



        /// <summary>
        /// メニュー表示状態
        /// </summary>
        public DelayVisibility MenuLayerVisibility { get; set; }

        //
        private void InitializeMenuLayerVisibility()
        {
            this.MenuLayerVisibility = new DelayVisibility();
            this.MenuLayerVisibility.Changed += (s, e) =>
            {
                this.LayerMenuSocket.Visibility = MenuLayerVisibility.Visibility;
            };
        }

        //
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

        //
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

        //
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



        #region Panel Visibility

        // ViewAreaでのマウス移動
        private void ViewArea_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateControlsVisibility();
        }

        //
        private void UpdateControlsVisibility()
        {
            UpdateMenuLayerVisibility();
            UpdateStatusLayerVisibility();
        }

        #endregion



        private void LeftPanel_KeyDown(object sender, KeyEventArgs e)
        {
            // nop.
        }

        private void RightPanel_KeyDown(object sender, KeyEventArgs e)
        {
            // nop.
        }


        private void MainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            // パネル表示状態更新
            UpdateControlsVisibility();
        }




        #region ContextMenu Counter
        // コンテキストメニューが開かれているかを判定するためのあまりよろしくない実装
        // ContextMenuスタイル既定で Opened,Closed イベントをハンドルし、開かれている状態を監視する

        private int _contextMenuOpenedCount;

        private bool _IsContextMenuOpened => _contextMenuOpenedCount > 0;

        private List<object> _openedContextMenuList = new List<object>();

        //
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (_openedContextMenuList.Contains(sender))
            {
                return;
            }

            _openedContextMenuList.Add(sender);
            _contextMenuOpenedCount++;

            UpdateControlsVisibility();
        }

        //
        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            _openedContextMenuList.Remove(sender);
            _contextMenuOpenedCount--;
            if (_contextMenuOpenedCount <= 0)
            {
                _contextMenuOpenedCount = 0;
                _openedContextMenuList.Clear();
            }

            UpdateControlsVisibility();
        }

        #endregion


        private void MenuArea_MouseEnter(object sender, MouseEventArgs e)
        {
            // nop.
        }

        private void MenuArea_MouseLeave(object sender, MouseEventArgs e)
        {
            // nop.
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
    }

}
