// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    /// NotifyPropertyChanged 配達
    /// </summary>
    public class NotifyPropertyChangedDelivery
    {
        public delegate void Reciever(object sender, PropertyChangedEventArgs e);

        private Dictionary<string, Reciever> _recieverCollection = new Dictionary<string, Reciever>();

        public void AddReciever(string propertyName, Reciever reciever)
        {
            _recieverCollection.Add(propertyName, reciever);
        }

        public bool Send(object sender, PropertyChangedEventArgs e)
        {
            Reciever reciever;
            if (_recieverCollection.TryGetValue(e.PropertyName, out reciever))
            {
                reciever(sender, e);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM _VM;

        private MouseInputManager _mouse;

        private ContentDropManager _contentDrop = new ContentDropManager();

        private bool _nowLoading = false;

        private NotifyPropertyChangedDelivery _notifyPropertyChangedDelivery = new NotifyPropertyChangedDelivery();

        // コンストラクタ
        public MainWindow()
        {
            InitializeComponent();

            InitializeMenuLayerVisibility();
            InitializeStatusLayerVisibility();

            // ウィンドウ座標復元
            if (!App.Options["--reset-placement"].IsValid && App.Setting.ViewMemento.IsSaveWindowPlacement)
            {
                WindowPlacement.Restore(this, App.Setting.WindowPlacement);
            }

#if DEBUG
            this.RootDockPanel.Children.Insert(0, new DevPageList());
            this.RootDockPanel.Children.Insert(1, new DevInfo());

            this.PreviewKeyDown += Debug_PreviewKeyDown;
#else
            this.MenuItemDev.Visibility = Visibility.Collapsed;
#endif

            // ViewModel
            _VM = new MainWindowVM(this);
            this.DataContext = _VM;

            App.Config.LocalApplicationDataRemoved +=
                (s, e) =>
                {
                    _VM.IsEnableSave = false; // 保存禁止
                    this.Close();
                };

            InitializeVisualTree();

            ////_VM.InitializeSidePanels(this.SidePanelFrame);

            // mouse input
            _mouse = MouseInputManager.Current = new MouseInputManager(this, this.MainView, this.MainContent, this.MainContentShadow);

            _mouse.TransformChanged +=
                (s, e) => _VM.SetViewTransform(e);

            this.LoupeInfo.DataContext = _mouse.Loupe;

            // mouse gesture
            _mouse.Gesture.MouseGestureProgressed += OnMouseGestureUpdate;

            // mouse drag
            ModelContext.DragActionTable.SetTarget(_mouse.Drag);


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
            InitializeCommandBindings();
            InitializeInputGestures();

            // publish routed commands
            _VM.BookCommands = BookCommands;

            // MainMenu Initialize
            _VM.MainMenuInitialize();

            // VM NotifyPropertyChanged Hook

            _notifyPropertyChangedDelivery.AddReciever(nameof(_VM.TinyInfoText),
                (s, e) =>
                {
                    AutoFade(TinyInfoTextBlock, 1.0, 0.5);
                });

            _notifyPropertyChangedDelivery.AddReciever(nameof(_VM.IsSliderDirectionReversed),
                (s, e) =>
                {
                    // Retrieve the Track from the Slider control
                    var track = this.PageSlider.Template.FindName("PART_Track", this.PageSlider) as System.Windows.Controls.Primitives.Track;
                    // Force it to rerender
                    track.InvalidateVisual();

                    this.PageMarkers.IsSliderDirectionReversed = _VM.IsSliderDirectionReversed;
                });

            _notifyPropertyChangedDelivery.AddReciever(nameof(_VM.IsLoupeCenter),
                (s, e) =>
                {
                    _mouse.Loupe.IsCenterMode = _VM.IsLoupeCenter; // ##
                });

            _notifyPropertyChangedDelivery.AddReciever(nameof(_VM.LongLeftButtonDownMode),
                (s, e) =>
                {
                    _mouse.Normal.LongLeftButtonDownMode = _VM.LongLeftButtonDownMode; // ##
                });


            // messenger
            Messenger.AddReciever("MessageBox", CallMessageBox);
            Messenger.AddReciever("MessageShow", CallMessageShow);
            Messenger.AddReciever("Export", CallExport);
            ////Messenger.AddReciever("ResetHideDelay", CallResetPanelHideDelay);

            // mouse event capture for active check
            this.MainView.PreviewMouseMove += MainView_PreviewMouseMove;
            this.MainView.PreviewMouseDown += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseUp += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseWheel += MainView_PreviewMouseAction;

            // timer for slideshow
            _timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(_maxTimerTick);
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);
            _timer.Start();

            // shlideshow mode check
            AppContext.Current.IsPlayingSlideShowChanged += AppContext_IsPlayingSlideShowChanged;

            // cancel rename triggers
            this.MouseLeftButtonDown += (s, e) => this.RenameManager.Stop();
            this.MouseRightButtonDown += (s, e) => this.RenameManager.Stop();
            this.Deactivated += (s, e) => this.RenameManager.Stop();
        }


        //
        private double _maxTimerTick = 0.2;

        /// <summary>
        /// スライドショー状態変更時にインターバル時間を修正する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppContext_IsPlayingSlideShowChanged(object sender, EventArgs e)
        {
            if (AppContext.Current.IsPlayingSlideShow)
            {
                if (_VM.SlideShowInterval < _timer.Interval.TotalSeconds * 0.5)
                {
                    var interval = _VM.SlideShowInterval * 0.5;
                    if (interval < 0.01) interval = 0.01;
                    if (interval > _maxTimerTick) interval = _maxTimerTick;
                    _timer.Interval = TimeSpan.FromSeconds(interval);
                }
                _lastShowTime = DateTime.Now;
            }
            else
            {
                _timer.Interval = TimeSpan.FromSeconds(_maxTimerTick);
            }

            Debug.WriteLine($"TimerInterval = {_timer.Interval.TotalMilliseconds}ms");
        }

        // ビジュアル初期化
        private void InitializeVisualTree()
        {
            ////this.MenuArea.Visibility = Visibility.Hidden;
            ////this.SliderArea.Visibility = Visibility.Hidden;
            ////this.ThumbnailListArea.Visibility = Visibility.Hidden;
            ////this.LeftPanel.Visibility = Visibility.Hidden;
            ////this.RightPanel.Visibility = Visibility.Hidden;

            // IsMouseOverの変更イベントをハンドルする。
            var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(Grid));
            dpd.AddValueChanged(this.MenuArea, MenuArea_IsMouseOverChanged);

            // IsFocusedの変更イベントをハンドルする。
            var dpd2 = DependencyPropertyDescriptor.FromProperty(UIElement.IsFocusedProperty, typeof(Grid));
            dpd2.AddValueChanged(this.AddressTextBox, MenuArea_IsMouseOverChanged);
        }

        // Preference適用
        public void ApplyPreference(Preference preference)
        {
            // パネルが自動的に隠れる時間
            ////this.AutoHideDelayTime = preference.panel_autohide_delaytime;

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
        private double DefaultViewAngle(bool isResetAngle)
        {
            return _VM.IsAutoRotateCondition() ? _VM.GetAutoRotateAngle() : isResetAngle ? 0.0 : _mouse.Drag.Angle;
        }

        //
        private void InitializeViewModelEvents()
        {
            _VM.ViewChanged +=
                (s, e) =>
                {
                    // ページ変更でルーペ解除
                    //_mouseLoupe.IsEnabled = false;
                    _mouse.IsLoupeMode = false;

                    UpdateMouseDragSetting(e.PageDirection, e.ViewOrigin);

                    bool isResetScale = e.ResetViewTransform || !_VM.IsKeepScale;
                    bool isResetAngle = e.ResetViewTransform || !_VM.IsKeepAngle || _VM.IsAutoRotate;
                    bool isResetFlip = e.ResetViewTransform || !_VM.IsKeepFlip;

                    _mouse.Drag.Reset(isResetScale, isResetAngle, isResetFlip, DefaultViewAngle(isResetAngle));
                };

            _VM.AutoRotateChanged +=
                (s, e) =>
                {
                    _mouse.Drag.Reset(true, true, true, _VM.ContentAngle);
                };

            _VM.InputGestureChanged +=
                (s, e) => InitializeInputGestures();

            _VM.PropertyChanged +=
                (s, e) => _notifyPropertyChangedDelivery.Send(s, e);

            _VM.Loading +=
                (s, e) =>
                {
                    _nowLoading = e != null;
                    DispNowLoading(_nowLoading);
                };

            _VM.NotifyMenuVisibilityChanged +=
                (s, e) => OnMenuVisibilityChanged();

            _VM.PageListChanged +=
                OnPageListChanged;

            _VM.IndexChanged +=
                OnIndexChanged;

#if false
            _VM.LeftPanelVisibled +=
                (s, e) =>
                {
                    if (e == PanelType.PageList) _VM.SidePanels.FolderListPanel.PageListControl.FocusAtOnce = true;
                    ////SetLeftPanelVisibisityForced(_isVisibleLeftPanel && _VM.LeftPanel != PanelType.None, false);
                };

            _VM.RightPanelVisibled +=
                (s, e) =>
                {
                    ////SetRightPanelVisibisityForced(_isVisibleRightPanel && _VM.RightPanel != PanelType.None, false);
                };
#endif

            _VM.ResetFocus +=
                (s, e) =>
                {
                    this.MainView.Focus();
                };
        }

        //
        private void OnIndexChanged(object sender, EventArgs e)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                _lastShowTime = DateTime.Now;
                DartyThumbnailList();
            });
        }

        //
        private void OnPageListChanged(object sender, EventArgs e)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                ////var sw = new Stopwatch();
                ////sw.Start();
                this.ThumbnailListBox.Items.Refresh();
                this.ThumbnailListBox.UpdateLayout();
                ////sw.Stop();
                ////Debug.WriteLine($"ThumbnailListBox: {sw.ElapsedMilliseconds}ms");
                DartyThumbnailList();
                LoadThumbnailList(+1);
            });
        }


#region Timer

        // タイマーディスパッチ
        private DispatcherTimer _timer;

        // 非アクティブ時間チェック用
        private DateTime _lastActionTime;
        private Point _lastActionPoint;

        // スライドショー表示間隔用
        private DateTime _lastShowTime;

        // タイマー処理
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                _lastActionTime = DateTime.Now;
                _lastShowTime = DateTime.Now;
                return;
            }

            // 非アクティブ時間が続いたらマウスカーソルを非表示にする
            if ((DateTime.Now - _lastActionTime).TotalSeconds > 2.0)
            {
                SetMouseVisible(false);
                _lastActionTime = DateTime.Now;
            }

            if (AppContext.Current.IsPlayingSlideShow)
            {
                // スライドショーのインターバルを非アクティブ時間で求める
                if ((DateTime.Now - _lastShowTime).TotalSeconds > _VM.SlideShowInterval)
                {
                    if (!_nowLoading) _VM.NextSlide();
                    _lastShowTime = DateTime.Now;
                }
            }
        }

        // マウス移動で非アクティブ時間リセット
        private void MainView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var nowPoint = e.GetPosition(this.MainView);

            if (Math.Abs(nowPoint.X - _lastActionPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(nowPoint.Y - _lastActionPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _lastActionTime = DateTime.Now;
                if (_VM.IsCancelSlideByMouseMove)
                {
                    _lastShowTime = DateTime.Now;
                }
                _lastActionPoint = nowPoint;
                SetMouseVisible(true);
            }
        }

        // マウスアクションで非アクティブ時間リセット
        private void MainView_PreviewMouseAction(object sender, MouseEventArgs e)
        {
            _lastActionTime = DateTime.Now;
            _lastShowTime = DateTime.Now;
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
            _VM.ShowGesture(e.Sequence.ToDispString(), _mouseGestureCommandCollection?.GetCommand(e.Sequence)?.Text);
        }


        // ドラッグでビュー操作設定の更新
        private void UpdateMouseDragSetting(int direction, DragViewOrigin origin)
        {
            _mouse.Drag.IsLimitMove = _VM.IsLimitMove;
            _mouse.Drag.DragControlCenter = _VM.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
            _mouse.Drag.AngleFrequency = _VM.AngleFrequency;

            if (origin == DragViewOrigin.None)
            {
                origin = _VM.IsViewStartPositionCenter
                    ? DragViewOrigin.Center
                    : _VM.BookSetting.BookReadOrder == PageReadOrder.LeftToRight
                        ? DragViewOrigin.LeftTop
                        : DragViewOrigin.RightTop;

                _mouse.Drag.ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                _mouse.Drag.ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop) ? 1.0 : -1.0;
            }
            else
            {
                _mouse.Drag.ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                _mouse.Drag.ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop || origin == DragViewOrigin.LeftBottom) ? 1.0 : -1.0;
            }
        }



        // RoutedCommand辞書
        public Dictionary<CommandType, RoutedUICommand> BookCommands => ModelContext.BookCommands;

        // RoutedCommand バインディング
        public void InitializeCommandBindings()
        {
            // RoutedCommand作成
            foreach (CommandType type in Enum.GetValues(typeof(CommandType)))
            {
                BookCommands.Add(type, new RoutedUICommand(ModelContext.CommandTable[type].Text, type.ToString(), typeof(MainWindow)));
            }

            // View系コマンド登録
            ModelContext.CommandTable[CommandType.OpenSettingWindow].Execute =
                (s, e) => OpenSettingWindow();
            ModelContext.CommandTable[CommandType.OpenSettingFilesFolder].Execute =
                (s, e) => OpenSettingFilesFolder();
            ModelContext.CommandTable[CommandType.OpenVersionWindow].Execute =
                (s, e) => OpenVersionWindow();
            ModelContext.CommandTable[CommandType.CloseApplication].Execute =
                (s, e) => Close();
            ModelContext.CommandTable[CommandType.ToggleWindowMinimize].Execute =
                (s, e) => MainWindow_Minimize();
            ModelContext.CommandTable[CommandType.ToggleWindowMaximize].Execute =
                (s, e) => MainWindow_Maximize();
            ModelContext.CommandTable[CommandType.LoadAs].Execute =
                (s, e) => LoadAs(e);
            ModelContext.CommandTable[CommandType.Paste].Execute =
                (s, e) => LoadFromClipboard();
            ModelContext.CommandTable[CommandType.Paste].CanExecute =
                () => CanLoadFromClipboard();
            ModelContext.CommandTable[CommandType.ViewScrollUp].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScrollCommandParameter)ModelContext.CommandTable[CommandType.ViewScrollUp].Parameter;
                    _mouse.Drag.ScrollUp(parameter.Scroll / 100.0);
                };
            ModelContext.CommandTable[CommandType.ViewScrollDown].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScrollCommandParameter)ModelContext.CommandTable[CommandType.ViewScrollDown].Parameter;
                    _mouse.Drag.ScrollDown(parameter.Scroll / 100.0);
                };
            ModelContext.CommandTable[CommandType.ViewScaleUp].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScaleCommandParameter)ModelContext.CommandTable[CommandType.ViewScaleUp].Parameter;
                    _mouse.Drag.ScaleUp(parameter.Scale / 100.0);
                };
            ModelContext.CommandTable[CommandType.ViewScaleDown].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScaleCommandParameter)ModelContext.CommandTable[CommandType.ViewScaleDown].Parameter;
                    _mouse.Drag.ScaleDown(parameter.Scale / 100.0);
                };
            ModelContext.CommandTable[CommandType.ViewRotateLeft].Execute =
                (s, e) =>
                {
                    var parameter = (ViewRotateCommandParameter)ModelContext.CommandTable[CommandType.ViewRotateLeft].Parameter;
                    if (parameter.IsStretch) _mouse.Drag.ResetDefault();
                    _mouse.Drag.Rotate(-parameter.Angle);
                    if (parameter.IsStretch) _VM.UpdateContentSize(_mouse.Drag.Angle);
                };
            ModelContext.CommandTable[CommandType.ViewRotateRight].Execute =
                (s, e) =>
                {
                    var parameter = (ViewRotateCommandParameter)ModelContext.CommandTable[CommandType.ViewRotateRight].Parameter;
                    if (parameter.IsStretch) _mouse.Drag.ResetDefault();
                    _mouse.Drag.Rotate(+parameter.Angle);
                    if (parameter.IsStretch) _VM.UpdateContentSize(_mouse.Drag.Angle);
                };
            ModelContext.CommandTable[CommandType.ToggleViewFlipHorizontal].Execute =
                (s, e) => _mouse.Drag.ToggleFlipHorizontal();
            ModelContext.CommandTable[CommandType.ViewFlipHorizontalOn].Execute =
                (s, e) => _mouse.Drag.FlipHorizontal(true);
            ModelContext.CommandTable[CommandType.ViewFlipHorizontalOff].Execute =
                (s, e) => _mouse.Drag.FlipHorizontal(false);

            ModelContext.CommandTable[CommandType.ToggleViewFlipVertical].Execute =
                (s, e) => _mouse.Drag.ToggleFlipVertical();
            ModelContext.CommandTable[CommandType.ViewFlipVerticalOn].Execute =
                (s, e) => _mouse.Drag.FlipVertical(true);
            ModelContext.CommandTable[CommandType.ViewFlipVerticalOff].Execute =
                (s, e) => _mouse.Drag.FlipVertical(false);

            ModelContext.CommandTable[CommandType.ViewReset].Execute =
                (s, e) => _mouse.Drag.Reset(true, true, true, DefaultViewAngle(true));
            ModelContext.CommandTable[CommandType.PrevScrollPage].Execute =
                (s, e) => PrevScrollPage();
            ModelContext.CommandTable[CommandType.NextScrollPage].Execute =
                (s, e) => NextScrollPage();
            ModelContext.CommandTable[CommandType.MovePageWithCursor].Execute =
                (s, e) => MovePageWithCursor();
            ModelContext.CommandTable[CommandType.MovePageWithCursor].ExecuteMessage =
                (e) => MovePageWithCursorMessage();

            ModelContext.CommandTable[CommandType.ToggleIsLoupe].Execute =
                (s, e) => _mouse.IsLoupeMode = !_mouse.IsLoupeMode;
            ModelContext.CommandTable[CommandType.ToggleIsLoupe].ExecuteMessage =
                e => _mouse.IsLoupeMode ? "ルーペOFF" : "ルーペON";
            ModelContext.CommandTable[CommandType.ToggleIsLoupe].CreateIsCheckedBinding =
                () => new Binding(nameof(_mouse.IsLoupeMode)) { Mode = BindingMode.OneWay, Source = _mouse };
            ModelContext.CommandTable[CommandType.LoupeOn].Execute =
                (s, e) => _mouse.IsLoupeMode = true;
            ModelContext.CommandTable[CommandType.LoupeOff].Execute =
                (s, e) => _mouse.IsLoupeMode = false;

            ModelContext.CommandTable[CommandType.Print].Execute =
                (s, e) =>
                {
                    _VM.Print(this, this.PageContents, this.MainContent.RenderTransform, this.MainView.ActualWidth, this.MainView.ActualHeight);
                };

            // context menu
            ModelContext.CommandTable[CommandType.OpenContextMenu].Execute =
                (s, e) =>
                {
                    if (this.MainViewPanel.ContextMenu != null)
                    {
                        this.MainViewPanel.ContextMenu.DataContext = _VM;
                        this.MainViewPanel.ContextMenu.PlacementTarget = this.MainViewPanel;
                        this.MainViewPanel.ContextMenu.Placement = PlacementMode.MousePoint;
                        this.MainViewPanel.ContextMenu.IsOpen = true;
                    }
                };

            // コマンドバインド作成
            foreach (CommandType type in Enum.GetValues(typeof(CommandType)))
            {
                if (ModelContext.CommandTable[type].CanExecute != null)
                {
                    this.CommandBindings.Add(new CommandBinding(BookCommands[type], (t, e) => _VM.Execute(type, e.Source, e.Parameter),
                        (t, e) => e.CanExecute = ModelContext.CommandTable[type].CanExecute()));
                }
                else
                {
                    this.CommandBindings.Add(new CommandBinding(BookCommands[type], (t, e) => _VM.Execute(type, e.Source, e.Parameter),
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
                var inputGestures = ModelContext.CommandTable[e.Key].GetInputGestureCollection();
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
                var mouseGesture = ModelContext.CommandTable[e.Key].MouseGesture;
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
            _VM.MainMenu?.UpdateInputGestureText();
            _VM.ContextMenu?.UpdateInputGestureText();
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
                dialog.InitialDirectory = _VM.BookHub.GetDefaultFolder();

                if (dialog.ShowDialog(this) == true)
                {
                    path = dialog.FileName;
                }
                else
                {
                    return;
                }
            }

            _VM.Load(path);
        }


        // スクロール＋前のページに戻る
        private void PrevScrollPage()
        {
            var parameter = (ScrollPageCommandParameter)ModelContext.CommandTable[CommandType.PrevScrollPage].Parameter;

            int bookReadDirection = (_VM.BookHub.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = _mouse.Drag.ScrollN(-1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation);

            if (!isScrolled)
            {
                _VM.NextViewOrigin = (_VM.BookHub.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightBottom : DragViewOrigin.LeftBottom;
                _VM.BookHub.PrevPage();
            }
        }

        // スクロール＋次のページに進む
        private void NextScrollPage()
        {
            var parameter = (ScrollPageCommandParameter)ModelContext.CommandTable[CommandType.NextScrollPage].Parameter;

            int bookReadDirection = (_VM.BookHub.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = _mouse.Drag.ScrollN(+1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation);

            if (!isScrolled)
            {
                _VM.NextViewOrigin = (_VM.BookHub.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightTop : DragViewOrigin.LeftTop;
                _VM.BookHub.NextPage();
            }
        }


        // マウスの位置でページを送る
        private void MovePageWithCursor()
        {
            var point = Mouse.GetPosition(this.MainView);

            if (point.X < this.MainView.ActualWidth * 0.5)
            {
                _VM.BookHub.NextPage();
            }
            else
            {
                _VM.BookHub.PrevPage();
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
            var setting = _VM.CreateSetting();
            var history = ModelContext.BookHistory.CreateMemento(false);

            // スライドショー停止
            AppContext.Current.PauseSlideShow();

            var dialog = new SettingWindow(setting, history);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                SetUpdateMenuLayoutMode(false);
                _VM.RestoreSetting(setting, false);
                SetUpdateMenuLayoutMode(true);
                _VM.StoreWindowPlacement(this);
                _VM.SaveSetting();
                ModelContext.BookHistory.Restore(history, false);
                AppContext.Current.RaizeAllPropertyChanged();

                // 現在ページ再読込
                _VM.BookHub.ReLoad();
            }

            // スライドショー再開
            AppContext.Current.ResumeSlideShow();
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

        // メニューレイアウト更新フラグ
        public bool _AllowUpdateMenuLayout;
        public bool _IsDartyMenuLayout;

        // メニューレイアウト更新要求
        private void OnMenuVisibilityChanged()
        {
            _IsDartyMenuLayout = true;
            if (_AllowUpdateMenuLayout)
            {
                UpdateMenuLayout();
            }
        }

        // メニューレイアウト更新許可設定
        private void SetUpdateMenuLayoutMode(bool allow)
        {
            _AllowUpdateMenuLayout = allow;
            if (_AllowUpdateMenuLayout && _IsDartyMenuLayout)
            {
                UpdateMenuLayout();
            }
        }

        /// <summary>
        /// メニューレイアウト更新
        /// </summary>
        private void UpdateMenuLayout()
        {
            _IsDartyMenuLayout = false;

            // menu hide
            bool isMenuDock = !_VM.IsHideMenu && !_VM.IsFullScreen;
            bool isPageSliderDock = !_VM.IsHidePageSlider && !_VM.IsFullScreen;
            bool isThimbnailListDock = !_VM.IsHideThumbnailList && isPageSliderDock;

#if false
            // panel hide
            if (_VM.CanHidePanel)
            {
                if (this.MainViewPanelGrid.Children.Contains(this.MainViewPanel))
                {
                    this.MainViewPanelGrid.Children.Remove(this.MainViewPanel);
                    this.ViewAreaBase.Children.Add(this.MainViewPanel);
                }
            }
            else
            {
                if (!this.MainViewPanelGrid.Children.Contains(this.MainViewPanel))
                {
                    this.ViewAreaBase.Children.Remove(this.MainViewPanel);
                    this.MainViewPanelGrid.Children.Add(this.MainViewPanel);
                }

                this.LeftPanel.Style = null;
                this.RightPanel.Style = null;
            }
#endif

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


            // アドレスバー
            this.AddressBar.Visibility = _VM.IsVisibleAddressBar ? Visibility.Visible : Visibility.Collapsed;

            // サムネイルリスト
            ////SetThumbnailListAreaVisibisity(_VM.IsEnableThumbnailList, true);
            this.ThumbnailListArea.Visibility = _VM.IsEnableThumbnailList ? Visibility.Visible : Visibility.Collapsed;
            DartyThumbnailList();


            ////double statusAreaHeight = this.PageSlider.Height + _VM.ThumbnailItemHeight; // アバウト
            ////double bottomMargin = (isPageSliderDock && _VM.IsEnableThumbnailList && !_VM.IsHideThumbnailList ? statusAreaHeight : this.PageSlider.Height);
            ////this.LeftPanelMargin.Height = bottomMargin;
            ////this.RightPanelMargin.Height = bottomMargin;
            //TODO: パネルボトムマージン実装

            // パネル表示設定
            ////UpdateLeftPanelVisibility();
            ////UpdateRightPanelVisibility();



            // コントロール表示状態更新
            {
                ////SetControlVisibility(this.LeftPanel, _isVisibleLeftPanel, true, VisibleStoryboardType.Collapsed);
                ////SetControlVisibility(this.RightPanel, _isVisibleRightPanel, true, VisibleStoryboardType.Collapsed);

                ////SetControlVisibility(this.MenuArea, _isMenuAreaVisibility, true, VisibleStoryboardType.Collapsed);

                ////SetControlVisibility(this.ThumbnailListArea, _isVisibleThumbnailList, true, VisibleStoryboardType.Collapsed);
                ////SetControlVisibility(this.SliderArea, _isVisibleStatausArea, true, VisibleStoryboardType.Collapsed);
            }


            //
            //Debug.WriteLine("MenuReset");
            MenuLayerVisibility.SetDelayVisibility(_VM.CanHideMenu ? Visibility.Collapsed : Visibility.Visible, 0);
            //UpdateMenuAreaVisibility();

            //
            StatusLayerVisibility.SetDelayVisibility(Visibility.Collapsed, 0);
            //UpdateStateAreaVisibility();

            // ビュー領域設定
            ////double menuAreaHeight = this.MenuBar.ActualHeight + (_VM.IsVisibleAddressBar ? this.AddressBar.Height : 0);
            ////this.ViewArea.Margin = new Thickness(0, isMenuDock ? menuAreaHeight : 0, 0, bottomMargin);

            // コンテンツ表示領域設定
            ////this.MainView.Margin = new Thickness(0, 0, 0, isPageSliderDock ? bottomMargin : 0);

            // 通知表示位置設定
            /*
            this.TinyInfoTextBlock.Margin = new Thickness(0, 0, 0, bottomMargin);
            this.InfoTextAreaBase.Margin = new Thickness(0, 0, 0, bottomMargin);
            this.NowLoadingTiny.Margin = new Thickness(0, 0, 0, bottomMargin);
            */
        }


        //
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
        }

        //
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // VMイベント設定
            InitializeViewModelEvents();

            SetUpdateMenuLayoutMode(false);

            // 設定反映
            _VM.RestoreSetting(App.Setting, true);

            // 
            _VM.FullScreenManager.WindowStateMemento = WindowState.Normal;

            // 履歴読み込み
            _VM.LoadHistory(App.Setting);

            // ブックマーク読み込み
            _VM.LoadBookmark(App.Setting);

            // ページマーク読込
            _VM.LoadPagemark(App.Setting);

            App.Setting = null; // ロード設定破棄

            // パネル幅復元
            ////this.LeftPanel.Width = _VM.LeftPanelWidth;
            ////this.RightPanel.Width = _VM.RightPanelWidth;

            // PanelColor
            _VM.FlushPanelColor();

            // サイドパネル初期化
            _VM.InitializeSidePanels(this.SidePanelFrame);

            // フォルダーリスト初期化
            ////this.FolderList.SetPlace(ModelContext.BookHistory.LastFolder ?? _VM.BookHub.GetFixedHome(), null, false);
            ////this.PageList.Initialize(_VM);
            // 履歴リスト初期化
            ////this.HistoryArea.Initialize(_VM.BookHub);
            // ブックマークリスト初期化
            ////this.BookmarkArea.Initialize(_VM.BookHub);
            // ブックマークリスト初期化
            ////this.PagemarkArea.Initialize(_VM.BookHub);

            // マーカー初期化
            this.PageMarkers.Initialize(_VM.BookHub);


            // オプションによるフルスクリーン指定
            if (App.Options["--fullscreen"].IsValid)
            {
                _VM.IsFullScreen = App.Options["--fullscreen"].Bool;
            }

            // ウィンドウモードで初期化
            OnMenuVisibilityChanged();
            SetUpdateMenuLayoutMode(true);




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
                    _VM.LoadLastFolder();
                }
            }

            // スライドショーの自動再生
            if (App.Options["--slideshow"].IsValid ? App.Options["--slideshow"].Bool : _VM.IsAutoPlaySlideShow)
            {
                AppContext.Current.IsPlayingSlideShow = true;
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
            if (this.WindowState == WindowState.Normal)
            {
                // フルスクリーン解除
                _VM.IsFullScreen = false;
            }

            // ルーペ解除
            _mouse.IsLoupeMode = false;
        }



        // ドラッグ＆ドロップ前処理
        private void MainWindow_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (!_nowLoading && _contentDrop.CheckDragContent(sender, e.Data))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
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
                string path = await _contentDrop.DropAsync(this, data, _VM.DownloadPath, (string message) => _VM.OnLoading(this, message));
                _VM.Load(path);
            }
            catch (Exception ex)
            {
                _VM.OnLoading(this, null);
                _VM.LoadError(ex.Message);
            }
        }


        // ウィンドウサイズが変化したらコンテンツサイズも追従する
        private void MainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _VM.SetViewSize(this.MainView.ActualWidth, this.MainView.ActualHeight);

            // スナップ
            _mouse.Drag.SnapView();
        }

        //
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // ウィンドウサイズ保存
            _VM.StoreWindowPlacement(this);
        }

        //
        private void Window_Closed(object sender, EventArgs e)
        {
            // タイマー停止
            _timer.Stop();

            // 設定保存
            _VM.SaveSetting();

            // スレッド終了処理 
            // 主にコマンドのキャンセル処理。 特にApp.Current にアクセスするものはキャンセルさせる
            _VM.Dispose();

            // テンポラリファイル破棄
            Temporary.RemoveTempFolder();

            // キャッシュDBを閉じる
            ThumbnailCache.Current.Dispose();

            Debug.WriteLine("Window.Closed done.");
            //Environment.Exit(0);
        }


        // 開発用コマンド：テンポラリフォルダーを開く
        private void MenuItemDevTempFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Temporary.TempDirectory);
        }

        // 開発用コマンド：アプリケーションフォルダーを開く
        private void MenuItemDevApplicationFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        // 開発用コマンド：アプリケーションデータフォルダーを開く
        private void MenuItemDevApplicationDataFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(App.Config.LocalApplicationDataPath);
        }



        // メッセージ処理：メッセージボックス表示
        private void CallMessageBox(object sender, MessageEventArgs e)
        {
            var param = (MessageBoxParams)e.Parameter;

            var dialog = new MessageBoxEx(param);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.AllowDrop = false;

            e.Result = dialog.ShowDialog();

            this.AllowDrop = true;
        }

        // メッセージ処理：通知表示
        private void CallMessageShow(object sender, MessageEventArgs e)
        {
            var param = (MessageShowParams)e.Parameter;

            _VM.InfoText = param.Text;
            this.InfoUsedBookmark.Visibility = param.BookmarkType == BookMementoType.Bookmark ? Visibility.Visible : Visibility.Collapsed;
            this.InfoUsedHistory.Visibility = param.BookmarkType == BookMementoType.History ? Visibility.Visible : Visibility.Collapsed;
            AutoFade(this.InfoTextArea, param.DispTime, 0.5);
        }

        // メッセージ処理：ファイル出力
        private void CallExport(object sender, MessageEventArgs e)
        {
            var exporter = (Exporter)e.Parameter;
            exporter.BackgroundBrush = _VM.BackgroundBrush;

            var dialog = new SaveWindow(exporter);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            e.Result = (result == true);
        }


        /// <summary>
        /// UI要素を自動的にフェイドアウトさせる
        /// </summary>
        /// <param name="element">UI要素</param>
        /// <param name="beginSec">フェイド開始時間(秒)</param>
        /// <param name="fadeSec">フェイドアウト時間(秒)</param>
        public static void AutoFade(UIElement element, double beginSec, double fadeSec)
        {
            // 既存のアニメーションを削除
            element.ApplyAnimationClock(UIElement.OpacityProperty, null);

            // 不透明度を1.0にする
            element.Opacity = 1.0;

            // 不透明度を0.0にするアニメを開始
            var ani = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(fadeSec));
            ani.BeginTime = TimeSpan.FromSeconds(beginSec);
            element.BeginAnimation(UIElement.OpacityProperty, ani);
        }


        // 現在のNowLoading表示状態
        private bool _isDispNowLoading = false;

        /// <summary>
        /// NowLoadinの表示/非表示
        /// </summary>
        /// <param name="isDisp"></param>
        private void DispNowLoading(bool isDisp)
        {
            if (_isDispNowLoading == isDisp) return;
            _isDispNowLoading = isDisp;

            if (isDisp && _VM.NowLoadingShowMessageStyle != ShowMessageStyle.None)
            {
                if (_VM.NowLoadingShowMessageStyle == ShowMessageStyle.Normal)
                {
                    this.NowLoadingNormal.Visibility = Visibility.Visible;
                    this.NowLoadingTiny.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.NowLoadingNormal.Visibility = Visibility.Collapsed;
                    this.NowLoadingTiny.Visibility = Visibility.Visible;
                }

                var ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                ani.BeginTime = TimeSpan.FromSeconds(1.0);
                this.NowLoading.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 360;
                aniRotate.Duration = TimeSpan.FromSeconds(2.0);
                aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
            else
            {
                var ani = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
                this.NowLoading.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 45;
                aniRotate.Duration = TimeSpan.FromSeconds(0.25);
                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
        }

        // オンラインヘルプ
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }

        // 情報エリアクリックでメインビューにフォーカスを移す
        /*
        private void InfoArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.MainView.Focus();
        }
        */

        // アドレスバー入力
        private void AddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                _VM.Address = this.AddressTextBox.Text;
            }

            // 単キーのショートカット無効
            KeyExGesture.AllowSingleKey = false;
            //e.Handled = true;
        }


        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        // 全てのルーテッドイベントの開始
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
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


        // [開発用] テストボタン
        private async void MenuItemDevButton_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(1000);
            Debug.WriteLine("TEST");
            Debugger.Break();
            //ModelContext.CommandTable.OpenCommandListHelp();
            //App.Config.RemoveApplicationData();
        }

        // [開発用] 現在のフォーカスを取得
        private void Debug_CheckFocus()
        {
            var element = FocusManager.GetFocusedElement(this);
            var fwelement = element as FrameworkElement;
            Debug.WriteLine($"FOCUS: {element}({element?.GetType()})({fwelement?.Name})");
        }
        #endregion

        // TODO: クラス化
        #region thumbnail list

        // サムネイルリストのパネルコントロール
        private VirtualizingStackPanel _thumbnailListPanel;

        private bool _isDartyThumbnailList = true;

        private void ThumbnailListArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DartyThumbnailList();
        }

        //
        private void DartyThumbnailList(bool isUpdateNow = false)
        {
            _isDartyThumbnailList = true;

            if (isUpdateNow || this.ThumbnailListArea.IsVisible)
            {
                UpdateThumbnailList();
            }
        }

        //
        private void UpdateThumbnailList()
        {
            UpdateThumbnailList(_VM.Index, _VM.IndexMax);
        }


        //
        private void UpdateThumbnailList(int index, int indexMax)
        {
            if (_thumbnailListPanel == null) return;

            if (!_VM.IsEnableThumbnailList) return;

            // リストボックス項目と同期がまだ取れていなければ処理しない
            //if (indexMax + 1 != this.ThumbnailListBox.Items.Count) return;

            // ここから
            if (!_isDartyThumbnailList) return;
            _isDartyThumbnailList = false;

            // 項目の幅 取得
            var listBoxItem = this.ThumbnailListBox.ItemContainerGenerator.ContainerFromIndex((int)_thumbnailListPanel.HorizontalOffset) as ListBoxItem;
            double itemWidth = (listBoxItem != null) ? listBoxItem.ActualWidth : 0.0;
            if (itemWidth <= 0.0) return;

            // 表示領域の幅
            double panelWidth = this.Root.ActualWidth;

            // 表示項目数を計算 (なるべく奇数)
            int itemsCount = (int)(panelWidth / itemWidth) / 2 * 2 + 1;
            if (itemsCount < 1) itemsCount = 1;

            // 表示先頭項目
            int topIndex = index - itemsCount / 2;
            if (topIndex < 0) topIndex = 0;

            // 少項目数補正
            if (indexMax + 1 < itemsCount)
            {
                itemsCount = indexMax + 1;
                topIndex = 0;
            }

            // ListBoxの幅を表示項目数にあわせる
            this.ThumbnailListBox.Width = itemWidth * itemsCount + 18; // TODO: 余裕が必要？

            // 表示項目先頭指定
            _thumbnailListPanel.SetHorizontalOffset(topIndex);

            // 選択
            this.ThumbnailListBox.SelectedIndex = index;

            // ##
            ////Debug.WriteLine(topIndex + " / " + this.ThumbnailListBox.Items.Count);

            // アライメント更新
            ThumbnailListBox_UpdateAlignment();
        }

        // TODO: 何度も来るのでいいかんじにする
        private void ThumbnailListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
            {
                this.ThumbnailListBox.SelectedIndex = _VM.Index;
                return;
            }

            ThumbnailListBox_UpdateAlignment();
        }

        private void ThumbnailListBox_UpdateAlignment()
        {
            // 端の表示調整
            if (this.ThumbnailListBox.Width > this.ThumbnailListArea.ActualWidth)
            {
                if (this.ThumbnailListBox.SelectedIndex <= 0)
                {
                    this.ThumbnailListBox.HorizontalAlignment = _VM.IsSliderDirectionReversed ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                }
                else if (this.ThumbnailListBox.SelectedIndex >= this.ThumbnailListBox.Items.Count - 1)
                {
                    this.ThumbnailListBox.HorizontalAlignment = _VM.IsSliderDirectionReversed ? HorizontalAlignment.Left : HorizontalAlignment.Right;
                }
                else
                {
                    this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Center;
                }
            }
            else
            {
                this.ThumbnailListBox.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        // リストボックスのドラッグ機能を無効化する
        private void ThumbnailListBox_IsMouseCapturedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.ThumbnailListBox.IsMouseCaptured)
            {
                this.ThumbnailListBox.ReleaseMouseCapture();
            }
        }

        // リストボックスのカーソルキーによる不意のスクロール抑制
        private void ThumbnailListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right);
        }

        // リストボックスのカーソルキーによる不意のスクロール抑制
        private void ThumbnailListBoxPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // 決定
            if (e.Key == Key.Return)
                _VM.BookHub.JumpPage(this.ThumbnailListBox.SelectedItem as Page);
            // 左右スクロールは自前で実装
            else if (e.Key == Key.Right)
                ThumbnailListBox_MoveSelectedIndex(+1);
            else if (e.Key == Key.Left)
                ThumbnailListBox_MoveSelectedIndex(-1);

            e.Handled = (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return);
        }

        //
        private void ThumbnailListBox_MoveSelectedIndex(int delta)
        {
            if (_thumbnailListPanel == null || this.ThumbnailListBox.SelectedIndex < 0) return;

            if (_thumbnailListPanel.FlowDirection == FlowDirection.RightToLeft)
                delta = -delta;

            int index = this.ThumbnailListBox.SelectedIndex + delta;
            if (index < 0)
                index = 0;
            if (index >= this.ThumbnailListBox.Items.Count)
                index = this.ThumbnailListBox.Items.Count - 1;

            this.ThumbnailListBox.SelectedIndex = index;
            this.ThumbnailListBox.ScrollIntoView(this.ThumbnailListBox.SelectedItem);
        }


        // 履歴項目決定
        private void ThumbnailListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            if (page != null)
            {
                _VM.BookHub.JumpPage(page);
                e.Handled = true;
            }
        }


        // スクロールしたらサムネ更新
        private void ThumbnailList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_thumbnailListPanel != null && this.ThumbnailListBox.Items.Count > 0)
            {
                LoadThumbnailList(e.HorizontalChange < 0 ? -1 : +1);
            }
        }

        // サムネ更新。表示されているページのサムネの読み込み要求
        private void LoadThumbnailList(int direction)
        {
            if (!this.ThumbnailListArea.IsVisible) return;

            if (_thumbnailListPanel != null)
            {
                _VM.RequestThumbnail((int)_thumbnailListPanel.HorizontalOffset, (int)_thumbnailListPanel.ViewportWidth, 2, direction);
            }
        }

        // 子ビジュアルコントロールの検索
        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child is T)
                {
                    return (T)child;
                }
                else
                {
                    child = FindVisualChild<T>(child);
                    if (child != null)
                    {
                        return (T)child;
                    }
                }
            }
            return null;
        }


        // スライダーに乗ったら表示開始
        private void PageSlider_MouseEnter(object sender, MouseEventArgs e)
        {
            /*
            if (_VM.IsEnableThumbnailList && !this.ThumbnailListArea.IsVisible)
            {
                //this.ThumbnailListArea.Visibility = Visibility.Visible;
                SetThumbnailListAreaVisibisity(true, false);
                UpdateThumbnailList();
            }
            */
        }


#if false

        // ステータスエリアの表示判定
        private void UpdateStateAreaVisibility_()
        {
            if (_VM.IsHidePageSlider || _VM.IsFullScreen)
            {
                SetStatusAreaVisibisity(IsStateAreaMouseOver() && !_mouse.IsLoupeMode, false);
                SetThumbnailListAreaVisibisity(_VM.IsEnableThumbnailList, true);
            }
            else
            {
                SetStatusAreaVisibisity(true, true);
                SetThumbnailListAreaVisibisity(_VM.IsEnableThumbnailList && (!_VM.CanHideThumbnailList || (IsStateAreaMouseOver() && !_mouse.IsLoupeMode)), false);
            }
        }

        //
        private bool IsStateAreaMouseOver()
        {
            const double visibleMargin = 32;
            const double hideMargin = 8;

            Point point = Mouse.GetPosition(this.Root);
            if (this.SliderArea.IsVisible)
            {
                double margin = this.SliderArea.ActualHeight + hideMargin > visibleMargin ? this.SliderArea.ActualHeight + hideMargin : visibleMargin;
                return point.Y > this.Root.ActualHeight - margin && this.IsMouseOver;
            }
            else
            {
                return (point.Y > this.Root.ActualHeight - visibleMargin) && this.IsMouseOver;
            }
        }


        private bool _isVisibleStatausArea = false;

        //
        private void SetStatusAreaVisibisity(bool isVisible, bool isQuickly)
        {
            if (_isVisibleStatausArea != isVisible)
            {
                _isVisibleStatausArea = isVisible;
                SetControlVisibility(this.SliderArea, _isVisibleStatausArea, isQuickly, VisibleStoryboardType.Collapsed);

                if (_isVisibleThumbnailList) UpdateThumbnailList();
            }
        }

        private bool _isVisibleThumbnailList;

        //
        private void SetThumbnailListAreaVisibisity(bool isVisible, bool isQuickly)
        {
            if (_isVisibleThumbnailList != isVisible)
            {
                _isVisibleThumbnailList = isVisible;
                SetControlVisibility(this.ThumbnailListArea, _isVisibleThumbnailList, isQuickly, VisibleStoryboardType.Collapsed);

                if (_isVisibleThumbnailList) UpdateThumbnailList();
            }
        }
#endif

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

            if (_VM.CanHideMenu)
            {
                var point = Mouse.GetPosition(this.Root);
                bool isVisible = this.AddressTextBox.IsFocused || this.LayerMenuSocket.IsMouseOver || point.Y < (MenuLayerVisibility.Visibility == Visibility.Visible ? this.LayerMenuSocket.ActualHeight : 0) + visibleMargin && this.IsMouseOver;
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
                    UpdateThumbnailList();
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



#if false
        /// <summary>
        /// メニュー表示更新
        /// </summary>
        private void UpdateMenuAreaVisibility_()
        {
            const double visibleMargin = 16;
            const double hideMargin = 8;

            if (_VM.IsHideMenu || _VM.IsFullScreen)
            {
                Point point = Mouse.GetPosition(this.Root);
                bool isVisible = this.AddressTextBox.IsFocused;
                if (this.MenuArea.IsVisible)
                {
                    double margin = this.MenuArea.ActualHeight + hideMargin > visibleMargin ? this.MenuArea.ActualHeight + hideMargin : visibleMargin;
                    isVisible = isVisible || this.MenuArea.IsMouseOver || (point.Y < 0.0 + margin && this.IsMouseOver);
                }
                else
                {
                    isVisible = isVisible || this.MenuBar.IsMouseOver || (point.Y < 0.0 + visibleMargin && this.IsMouseOver);
                }
                isVisible = isVisible && !_mouse.IsLoupeMode;
                SetMenuAreaVisibisity(isVisible, false);
            }
            else
            {
                SetMenuAreaVisibisity(true, true);
            }
        }


        private bool _isMenuAreaVisibility;

        //
        private void SetMenuAreaVisibisity(bool isVisible, bool isQuickly)
        {
            if (_isMenuAreaVisibility != isVisible)
            {
                _isMenuAreaVisibility = isVisible;
                SetControlVisibility(this.MenuArea, _isMenuAreaVisibility, isQuickly, VisibleStoryboardType.Hidden);
            }
        }
#endif


#region Panel Visibility

        //
        ////private bool _isVisibleLeftPanel;
        ////private bool _isVisibleRightPanel;

        // ViewAreaでのマウス移動
        private void ViewArea_MouseMove(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine($"Drag: {DateTime.Now}");

            UpdateControlsVisibility();
        }

        //
        private void UpdateControlsVisibility()
        {
            ////UpdateLeftPanelVisibility();
            ////UpdateRightPanelVisibility();
            UpdateMenuLayerVisibility();
            UpdateStatusLayerVisibility();
        }


#if false
        //
        private void UpdateLeftPanelVisibility()
        {
            const double visibleMargin = 32;
            const double hideMargin = 16;

            if (_VM.LeftPanel == PanelType.None)
            {
                SetLeftPanelVisibisity(false, false);
            }
            else if (_VM.LeftPanel == PanelType.FolderList && _VM.FolderListPanel.FolderListControl.IsRenaming)
            {
                SetLeftPanelVisibisity(true, false);
            }
            else if (_VM.CanHidePanel)
            {
                Point point = Mouse.GetPosition(this.ViewArea);
                if (this.LeftPanel.IsVisible)
                {
                    double margin = this.LeftPanel.Width + hideMargin > visibleMargin ? this.LeftPanel.Width + hideMargin : visibleMargin;
                    SetLeftPanelVisibisity(this.LeftPanel.IsMouseOver || _IsContextMenuOpened || point.X < margin && this.IsMouseOver, false);
                }
                else if (point.X < visibleMargin && this.IsMouseOver && !this.MenuArea.IsMouseOver && !this.StatusArea.IsMouseOver && !_mouse.IsLoupeMode)
                {
                    SetLeftPanelVisibisity(true, false);
                }
            }
            else
            {
                SetLeftPanelVisibisity(true, false);
            }
        }


        //
        private void SetLeftPanelVisibisity(bool isVisible, bool isQuickly)
        {
            if (_isVisibleLeftPanel != isVisible)
            {
                SetLeftPanelVisibisityForced(isVisible, isQuickly);
            }
        }

        //
        private void SetLeftPanelVisibisityForced(bool isVisible, bool isQuickly)
        {
            _isVisibleLeftPanel = isVisible;
            SetControlVisibility(this.LeftPanel, _isVisibleLeftPanel, isQuickly || _VM.LeftPanel == PanelType.None, VisibleStoryboardType.Collapsed);
        }





        //
        private void UpdateRightPanelVisibility()
        {
            const double visibleMargin = 32;
            const double hideMargin = 16;

            if (_VM.RightPanel == PanelType.None)
            {
                SeRightPanelVisibisity(false, false);
            }
            else if (_VM.CanHidePanel)
            {
                Point point = Mouse.GetPosition(this.ViewArea);
                if (this.RightPanel.IsVisible)
                {
                    double margin = this.RightPanel.Width + hideMargin > visibleMargin ? this.RightPanel.Width + hideMargin : visibleMargin;
                    SeRightPanelVisibisity(this.RightPanel.IsMouseOver || _IsContextMenuOpened || point.X > this.ViewArea.ActualWidth - margin && this.IsMouseOver, false);
                }
                else if (point.X > this.ViewArea.ActualWidth - visibleMargin && this.IsMouseOver && !this.MenuArea.IsMouseOver && !this.StatusArea.IsMouseOver && !_mouse.IsLoupeMode)
                {
                    SeRightPanelVisibisity(true, false);
                }
            }
            else
            {
                SeRightPanelVisibisity(true, false);
            }
        }

        //
        private void SeRightPanelVisibisity(bool isVisible, bool isQuickly)
        {
            if (_isVisibleRightPanel != isVisible)
            {
                SetRightPanelVisibisityForced(isVisible, isQuickly);
            }
        }

        //
        private void SetRightPanelVisibisityForced(bool isVisible, bool isQuickly)
        {
            _isVisibleRightPanel = isVisible;
            SetControlVisibility(this.RightPanel, _isVisibleRightPanel, isQuickly || _VM.RightPanel == PanelType.None, VisibleStoryboardType.Collapsed);
        }

                /// <summary>
        /// パネル非表示のリセット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallResetPanelHideDelay(object sender, MessageEventArgs e)
        {
            var param = (ResetHideDelayParam)e.Parameter;

            if (param.PanelSide == PanelSide.Left)
                if (!_isVisibleLeftPanel) SetLeftPanelVisibisityForced(_isVisibleLeftPanel, false);

            if (param.PanelSide == PanelSide.Right)
                if (!_isVisibleRightPanel) SetRightPanelVisibisityForced(_isVisibleRightPanel, false);
        }
#endif



#if false
        //
        private enum VisibleStoryboardType
        {
            Collapsed,
            Hidden,
            Opacity,
        }

        private class VisibleStoryboard
        {
            public Storyboard On { get; set; }
            public Storyboard Off { get; set; }
            public Storyboard OffDelay { get; set; }
        }

        private Dictionary<VisibleStoryboardType, VisibleStoryboard> _visibleStoryboardTable;

        //
        private Storyboard _visibleStoryboard;
        private Storyboard _collapseStoryboard;
        private Storyboard _collapseDelayStoryboard;
        private Storyboard _hideStoryboard;
        private Storyboard _hideDelayStoryboard;

        private Storyboard _opacityOneStoryboard;
        private Storyboard _opacityZeroStoryboard;
        private Storyboard _opacityZeroDelayStoryboard;

        private double _autoHideDelayTime = 1.0;
        public double AutoHideDelayTime
        {
            get { return _autoHideDelayTime; }
            set
            {
                if (_autoHideDelayTime != value)
                {
                    _autoHideDelayTime = NVUtility.Clamp(value, 0.0, 100.0);
                    _visibleStoryboard = null; // storyboard作り直し
                }
            }
        }

        //
        private void InitializeStoryboard()
        {
            if (_visibleStoryboard != null) return;

            double time = _autoHideDelayTime;
            ObjectAnimationUsingKeyFrames ani;

            ani = new ObjectAnimationUsingKeyFrames();
            ani.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Visible, TimeSpan.FromSeconds(0.0)));
            Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.VisibilityProperty));
            _visibleStoryboard = new Storyboard();
            _visibleStoryboard.Children.Add(ani);


            ani = new ObjectAnimationUsingKeyFrames();
            ani.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Collapsed, TimeSpan.FromSeconds(0.0)));
            Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.VisibilityProperty));
            _collapseStoryboard = new Storyboard();
            _collapseStoryboard.Children.Add(ani);


            ani = new ObjectAnimationUsingKeyFrames();
            ani.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Visible, TimeSpan.FromSeconds(0.0)));
            ani.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Collapsed, TimeSpan.FromSeconds(time)));
            Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.VisibilityProperty));
            _collapseDelayStoryboard = new Storyboard();
            _collapseDelayStoryboard.Children.Add(ani);


            ani = new ObjectAnimationUsingKeyFrames();
            ani.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Hidden, TimeSpan.FromSeconds(0.0)));
            Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.VisibilityProperty));
            _hideStoryboard = new Storyboard();
            _hideStoryboard.Children.Add(ani);


            ani = new ObjectAnimationUsingKeyFrames();
            ani.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Visible, TimeSpan.FromSeconds(0.0)));
            ani.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Hidden, TimeSpan.FromSeconds(time)));
            Storyboard.SetTargetProperty(ani, new PropertyPath(UIElement.VisibilityProperty));
            _hideDelayStoryboard = new Storyboard();
            _hideDelayStoryboard.Children.Add(ani);


            DoubleAnimationUsingKeyFrames an;

            an = new DoubleAnimationUsingKeyFrames();
            an.KeyFrames.Add(new DiscreteDoubleKeyFrame(1.0, TimeSpan.FromSeconds(0.0)));
            Storyboard.SetTargetProperty(an, new PropertyPath(UIElement.OpacityProperty));
            _opacityOneStoryboard = new Storyboard();
            _opacityOneStoryboard.Children.Add(an);

            an = new DoubleAnimationUsingKeyFrames();
            an.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, TimeSpan.FromSeconds(0.0)));
            Storyboard.SetTargetProperty(an, new PropertyPath(UIElement.OpacityProperty));
            _opacityZeroStoryboard = new Storyboard();
            _opacityZeroStoryboard.Children.Add(an);

            an = new DoubleAnimationUsingKeyFrames();
            an.KeyFrames.Add(new DiscreteDoubleKeyFrame(1.0, TimeSpan.FromSeconds(0.0)));
            an.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, TimeSpan.FromSeconds(time)));
            Storyboard.SetTargetProperty(an, new PropertyPath(UIElement.OpacityProperty));
            _opacityZeroDelayStoryboard = new Storyboard();
            _opacityZeroDelayStoryboard.Children.Add(an);


            _visibleStoryboardTable = new Dictionary<VisibleStoryboardType, VisibleStoryboard>();

            _visibleStoryboardTable.Add(VisibleStoryboardType.Collapsed, new VisibleStoryboard()
            {
                On = _visibleStoryboard,
                Off = _collapseStoryboard,
                OffDelay = _collapseDelayStoryboard,
            });

            _visibleStoryboardTable.Add(VisibleStoryboardType.Hidden, new VisibleStoryboard()
            {
                On = _visibleStoryboard,
                Off = _hideStoryboard,
                OffDelay = _hideDelayStoryboard,
            });

            _visibleStoryboardTable.Add(VisibleStoryboardType.Opacity, new VisibleStoryboard()
            {
                On = _opacityOneStoryboard,
                Off = _opacityZeroStoryboard,
                OffDelay = _opacityZeroDelayStoryboard,
            });
        }


        //
        private void SetControlVisibility(FrameworkElement element, bool isDisp, bool isQuickly, VisibleStoryboardType visibleType)
        {
            ////Debug.WriteLine(element.Name + ":" + isDisp);

            InitializeStoryboard();

            if (isDisp)
            {
                element.BeginStoryboard(_visibleStoryboardTable[visibleType].On);
            }
            else
            {
                element.BeginStoryboard(isQuickly ? _visibleStoryboardTable[visibleType].Off : _visibleStoryboardTable[visibleType].OffDelay);
            }
        }
#endif

#endregion

        //
        private void PageSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // nop.
        }

        private void PageSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_VM.CanSliderLinkedThumbnailList)
            {
                _VM.SetIndex(_VM.Index);
            }
        }

        private void PageSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // nop.
        }

        private void LeftPanel_KeyDown(object sender, KeyEventArgs e)
        {
            // nop.
        }

        private void RightPanel_KeyDown(object sender, KeyEventArgs e)
        {
            // nop.
        }

        private void ThumbnailListBox_Loaded(object sender, RoutedEventArgs e)
        {
            // nop.
        }

        private void ThumbnailListBoxPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // パネルコントロール取得
            _thumbnailListPanel = sender as VirtualizingStackPanel;
            DartyThumbnailList();
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


#if false
        private void LeftPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _VM.IsVisibleLeftPanel = this.LeftPanel.IsVisible;
        }

        private void RightPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _VM.IsVisibleRightPanel = this.RightPanel.IsVisible;
        }
#endif

        private void MenuArea_MouseEnter(object sender, MouseEventArgs e)
        {
            // nop.
        }

        private void MenuArea_MouseLeave(object sender, MouseEventArgs e)
        {
            // nop.
        }

        private void ThumbnailListArea_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadThumbnailList(1);
        }

        private void ThumbnailListBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int count = MouseInputHelper.DeltaCount(e);
            int delta = e.Delta < 0 ? +count : -count;
            if (_VM.IsSliderDirectionReversed) delta = -delta;
            ThumbnailListBox_MoveSelectedIndex(delta);
            e.Handled = true;
        }

        private void PageSliderTextBox_ValueChanged(object sender, EventArgs e)
        {
            _VM.SetIndex(_VM.Index);
        }

        /// <summary>
        /// 履歴戻るボタンコンテキストメニュー開始前イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrevHistoryButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _VM.GetHistory(-1, 10);
        }

        /// <summary>
        /// 履歴進むボタンコンテキストメニュー開始前イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextHistoryButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _VM.GetHistory(+1, 10);
        }

        /// <summary>
        /// スライダーエリアでのマウスホイール操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int turn = MouseInputHelper.DeltaCount(e);

            for (int i = 0; i < turn; ++i)
            {
                if (e.Delta < 0)
                {
                    _VM.BookHub.NextPage();
                }
                else
                {
                    _VM.BookHub.PrevPage();
                }
            }
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

            // 背景更新
            _VM?.UpdateBackgroundBrush();

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


#region Convertes

    // コンバータ：より大きい値ならTrue
    public class IsGreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = System.Convert.ToDouble(value);
            var compareValue = double.Parse(parameter as string);
            return v > compareValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：より小さい値ならTrue
    public class IsLessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = System.Convert.ToDouble(value);
            var compareValue = double.Parse(parameter as string);
            return v < compareValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：０に近ければTrue
    public class IsNearZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var v = System.Convert.ToDouble(value);
                return v < 0.01;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return true;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    // コンバータ：ページモードフラグ
    [ValueConversion(typeof(PageMode), typeof(bool))]
    public class PageModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PageMode mode0 = (PageMode)value;
            PageMode mode1 = (PageMode)Enum.Parse(typeof(PageMode), parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：ストレッチモードフラグ
    [ValueConversion(typeof(PageStretchMode), typeof(bool))]
    public class StretchModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PageStretchMode mode0 = (PageStretchMode)value;
            PageStretchMode mode1 = (PageStretchMode)Enum.Parse(typeof(PageStretchMode), parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：ソートモードフラグ
    [ValueConversion(typeof(PageSortMode), typeof(bool))]
    public class SortModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PageSortMode mode0 = (PageSortMode)value;
            PageSortMode mode1 = (PageSortMode)Enum.Parse(typeof(PageSortMode), parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    // コンバータ：ソートモードフラグ
    [ValueConversion(typeof(PageSortMode), typeof(Visibility))]
    public class SortModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PageSortMode mode0 = (PageSortMode)value;
            PageSortMode mode1 = (PageSortMode)Enum.Parse(typeof(PageSortMode), parameter as string);
            return (mode0 == mode1) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    // コンバータ：ソートモード表示文字列
    [ValueConversion(typeof(PageSortMode), typeof(string))]
    public class SortModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PageSortMode mode = (PageSortMode)value;
            return mode.ToDispString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    // コンバータ：右開き、左開きフラグ
    [ValueConversion(typeof(PageReadOrder), typeof(bool))]
    public class BookReadOrderToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PageReadOrder mode0 = (PageReadOrder)value;
            PageReadOrder mode1 = (PageReadOrder)Enum.Parse(typeof(PageReadOrder), parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：フルパスのファイル名取得
    [ValueConversion(typeof(string), typeof(string))]
    public class FullPathToFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                string path = (string)value;
                return NVUtility.PlaceToTitle(path);
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：フォルダーの並びフラグ
    [ValueConversion(typeof(FolderOrder), typeof(bool))]
    public class FolderOrderToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FolderOrder mode0 = (FolderOrder)value;
            FolderOrder mode1 = (FolderOrder)Enum.Parse(typeof(FolderOrder), parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：背景フラグ
    [ValueConversion(typeof(BackgroundStyle), typeof(bool))]
    public class BackgroundStyleToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BackgroundStyle mode0 = (BackgroundStyle)value;
            BackgroundStyle mode1 = (BackgroundStyle)Enum.Parse(typeof(BackgroundStyle), parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：DPI調整
    [ValueConversion(typeof(double), typeof(double))]
    public class DpiScaleXInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double length;

            if (value is double)
                length = (double)value;
            else if (value is int)
                length = (int)value;
            else
                length = double.Parse((string)value);

            return length;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：DPI調整
    [ValueConversion(typeof(double), typeof(double))]
    public class DpiScaleYInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double length;

            if (value is double)
                length = (double)value;
            else if (value is int)
                length = (int)value;
            else
                length = double.Parse((string)value);

            return length;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：サムネイル方向
    [ValueConversion(typeof(bool), typeof(FlowDirection))]
    public class SliderDirectionToFlowDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isReverse = false; ;
            if (value is bool)
            {
                isReverse = (bool)value;
            }
            else if (value is string)
            {
                bool.TryParse((string)value, out isReverse);
            }

            return isReverse ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    // コンバータ：ファイルサイズのKB表示
    [ValueConversion(typeof(PageMode), typeof(bool))]
    public class FileSizeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var length = (long)value;

            if (length < 0)
            {
                return "";
            }
            else if (length == 0)
            {
                return "0 KB";
            }
            else
            {
                return $"{(length + 1023) / 1024:#,0} KB";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

#endregion
}
