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

        private MouseDragController _mouseDrag;
        private MouseGestureManager _mouseGesture;
        private MouseLongDown _mouseLongDown;

        private ContentDropManager _contentDrop = new ContentDropManager();

        private bool _nowLoading = false;

        public MouseDragController MouseDragController => _mouseDrag;

        private NotifyPropertyChangedDelivery _notifyPropertyChangedDelivery = new NotifyPropertyChangedDelivery();

        // コンストラクタ
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
#else
            this.MenuItemDev.Visibility = Visibility.Collapsed;
#endif

            // ViewModel
            _VM = new MainWindowVM();
            this.DataContext = _VM;

            App.Config.LocalApplicationDataRemoved +=
                (s, e) =>
                {
                    _VM.IsEnableSave = false; // 保存禁止
                    this.Close();
                };

            InitializeVisualTree();

            // mouse long down
            _mouseLongDown = new MouseLongDown(this.MainView);
            _mouseLongDown.StatusChanged +=
                (s, e) =>
                {
                    if (_VM.LongLeftButtonDownMode == LongButtonDownMode.Loupe)
                    {
                        _mouseDrag.IsLoupe = (e == MouseLongDownStatus.On);
                    }
                };
            _mouseLongDown.MouseWheel +=
                (s, e) =>
                {
                    if (_VM.LongLeftButtonDownMode == LongButtonDownMode.Loupe)
                    {
                        _mouseDrag.LoupeZoom(e);
                    }
                };

            _notifyPropertyChangedDelivery.AddReciever(nameof(_VM.LongButtonDownTick),
                (s, e) =>
                {
                    _mouseLongDown.Tick = TimeSpan.FromSeconds(_VM.LongButtonDownTick);
                });


            // mouse drag
            _mouseDrag = new MouseDragController(this, this.MainView, this.MainContent, this.MainContentShadow);
            _mouseDrag.TransformChanged +=
                (s, e) =>
                {
                    _VM.SetViewTransform(_mouseDrag.Scale, _mouseDrag.FixedLoupeScale, _mouseDrag.Angle, _mouseDrag.IsFlipHorizontal, _mouseDrag.IsFlipVertical, e.ActionType);
                    if (e.ChangeType == TransformChangeType.Scale)
                    {
                        _VM.UpdateWindowTitle(UpdateWindowTitleMask.View);
                    }
                };
            ModelContext.DragActionTable.SetTarget(_mouseDrag);

            // mouse gesture
            _mouseGesture = new MouseGestureManager(this.MainView);
            _mouseGesture.Controller.MouseGestureUpdateEventHandler += OnMouseGestureUpdate;



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


            // messenger
            Messenger.AddReciever("MessageBox", CallMessageBox);
            Messenger.AddReciever("MessageShow", CallMessageShow);
            Messenger.AddReciever("Export", CallExport);
            Messenger.AddReciever("ResetHideDelay", CallResetPanelHideDelay);

            // mouse event capture for active check
            this.MainView.PreviewMouseMove += MainView_PreviewMouseMove;
            this.MainView.PreviewMouseDown += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseUp += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseWheel += MainView_PreviewMouseAction;

            // timer for slideshow
            _timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(0.2);
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);
            _timer.Start();
        }

        // ビジュアル初期化
        private void InitializeVisualTree()
        {
            this.MenuArea.Opacity = 0.0;
            this.StatusArea.Visibility = Visibility.Hidden;
            this.ThumbnailListArea.Visibility = Visibility.Hidden;
            this.LeftPanel.Visibility = Visibility.Hidden;
            this.RightPanel.Visibility = Visibility.Hidden;

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
            this.AutoHideDelayTime = preference.panel_autohide_delaytime;

            // マウスジェスチャーの最小移動距離
            _mouseGesture.Controller.InitializeGestureMinimumDistance(
                preference.input_gesture_minimumdistance_x,
                preference.input_gesture_minimumdistance_y);
        }

        //
        private void MenuArea_IsMouseOverChanged(object sender, EventArgs e)
        {
            UpdateMenuAreaVisibility();
        }

        //
        private void InitializeViewModelEvents()
        {
            _VM.ViewChanged +=
                (s, e) =>
                {
                    UpdateMouseDragSetting(e.PageDirection, e.ViewOrigin);
                    bool isResetScale = e.ResetViewTransform || !_VM.IsKeepScale;
                    bool isResetAngle = e.ResetViewTransform || !_VM.IsKeepAngle;
                    bool isResetFlip = e.ResetViewTransform || !_VM.IsKeepFlip;
                    _mouseDrag.Reset(isResetScale, isResetAngle, isResetFlip);
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

            _VM.ContextMenuEnableChanged +=
                (s, e) =>
                {
                    _mouseGesture.Controller.ContextMenuSetting = _VM.ContextMenuSetting;
                };

            _VM.PageListChanged +=
                OnPageListChanged;

            _VM.IndexChanged +=
                OnIndexChanged;

            _VM.LeftPanelVisibled +=
                (s, e) =>
                {
                    if (e == PanelType.PageList) this.PageList.FocusAtOnce = true;
                    SetLeftPanelVisibisityForced(_isVisibleLeftPanel && _VM.LeftPanel != PanelType.None, false);
                };

            _VM.RightPanelVisibled +=
                (s, e) =>
                {
                    SetRightPanelVisibisityForced(_isVisibleRightPanel && _VM.RightPanel != PanelType.None, false);
                };
        }

        //
        private void OnIndexChanged(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                DartyThumbnailList();
            });
        }

        //
        private void OnPageListChanged(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.ThumbnailListBox.Items.Refresh();
                this.ThumbnailListBox.UpdateLayout();
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

            // スライドショーのインターバルを非アクティブ時間で求める
            if ((DateTime.Now - _lastShowTime).TotalSeconds > _VM.SlideShowInterval)
            {
                if (!_nowLoading) _VM.NextSlide();
                _lastShowTime = DateTime.Now;
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
                if (this.MainView.Cursor == Cursors.None && !_mouseDrag.IsLoupe)
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
        private void OnMouseGestureUpdate(object sender, MouseGestureSequence e)
        {
            _VM.ShowGesture(_mouseGesture.GetGestureString(), _mouseGesture.GetGestureCommandName());
        }


        // ドラッグでビュー操作設定の更新
        private void UpdateMouseDragSetting(int direction, DragViewOrigin origin)
        {
            _mouseDrag.IsLimitMove = _VM.IsLimitMove;
            _mouseDrag.DragControlCenter = _VM.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
            _mouseDrag.SnapAngle = _VM.IsAngleSnap ? 45 : 0;


            if (origin == DragViewOrigin.None)
            {
                origin = _VM.IsViewStartPositionCenter
                    ? DragViewOrigin.Center
                    : _VM.BookSetting.BookReadOrder == PageReadOrder.LeftToRight
                        ? DragViewOrigin.LeftTop
                        : DragViewOrigin.RightTop;

                _mouseDrag.ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                _mouseDrag.ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop) ? 1.0 : -1.0;
            }
            else
            {
                _mouseDrag.ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                _mouseDrag.ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop || origin == DragViewOrigin.LeftBottom) ? 1.0 : -1.0;
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
                    _mouseDrag.ScrollUp(parameter.Scroll / 100.0);
                };
            ModelContext.CommandTable[CommandType.ViewScrollDown].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScrollCommandParameter)ModelContext.CommandTable[CommandType.ViewScrollDown].Parameter;
                    _mouseDrag.ScrollDown(parameter.Scroll / 100.0);
                };
            ModelContext.CommandTable[CommandType.ViewScaleUp].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScaleCommandParameter)ModelContext.CommandTable[CommandType.ViewScaleUp].Parameter;
                    _mouseDrag.ScaleUp(parameter.Scale / 100.0);
                };
            ModelContext.CommandTable[CommandType.ViewScaleDown].Execute =
                (s, e) =>
                {
                    var parameter = (ViewScaleCommandParameter)ModelContext.CommandTable[CommandType.ViewScaleDown].Parameter;
                    _mouseDrag.ScaleDown(parameter.Scale / 100.0);
                };
            ModelContext.CommandTable[CommandType.ViewRotateLeft].Execute =
                (s, e) =>
                {
                    var parameter = (ViewRotateCommandParameter)ModelContext.CommandTable[CommandType.ViewRotateLeft].Parameter;
                    _mouseDrag.Rotate(-parameter.Angle);
                };
            ModelContext.CommandTable[CommandType.ViewRotateRight].Execute =
                (s, e) =>
                {
                    var parameter = (ViewRotateCommandParameter)ModelContext.CommandTable[CommandType.ViewRotateRight].Parameter;
                    _mouseDrag.Rotate(+parameter.Angle);
                };
            ModelContext.CommandTable[CommandType.ToggleViewFlipHorizontal].Execute =
                (s, e) => _mouseDrag.ToggleFlipHorizontal();
            ModelContext.CommandTable[CommandType.ViewFlipHorizontalOn].Execute =
                (s, e) => _mouseDrag.FlipHorizontal(true);
            ModelContext.CommandTable[CommandType.ViewFlipHorizontalOff].Execute =
                (s, e) => _mouseDrag.FlipHorizontal(false);

            ModelContext.CommandTable[CommandType.ToggleViewFlipVertical].Execute =
                (s, e) => _mouseDrag.ToggleFlipVertical();
            ModelContext.CommandTable[CommandType.ViewFlipVerticalOn].Execute =
                (s, e) => _mouseDrag.FlipVertical(true);
            ModelContext.CommandTable[CommandType.ViewFlipVerticalOff].Execute =
                (s, e) => _mouseDrag.FlipVertical(false);

            ModelContext.CommandTable[CommandType.ViewReset].Execute =
                (s, e) => _mouseDrag.Reset(true, true, true);
            ModelContext.CommandTable[CommandType.PrevScrollPage].Execute =
                (s, e) => PrevScrollPage();
            ModelContext.CommandTable[CommandType.NextScrollPage].Execute =
                (s, e) => NextScrollPage();
            ModelContext.CommandTable[CommandType.MovePageWithCursor].Execute =
                (s, e) => MovePageWithCursor();

            ModelContext.CommandTable[CommandType.MovePageWithCursor].ExecuteMessage =
                (e) => MovePageWithCursorMessage();


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


        // InputGesture設定
        public void InitializeInputGestures()
        {
            _mouseDrag.ClearClickEventHandler();

            _mouseGesture.ClearClickEventHandler();
            _mouseGesture.CommandCollection.Clear();

            foreach (var e in BookCommands)
            {
                e.Value.InputGestures.Clear();
                var inputGestures = ModelContext.CommandTable[e.Key].GetInputGestureCollection();
                foreach (var gesture in inputGestures)
                {
                    // マウスクリックはドラッグ系処理のイベントとして登録
                    if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.LeftClick)
                    {
                        _mouseDrag.MouseClickEventHandler += (s, x) => { if (gesture.Matches(this, x)) { e.Value.Execute(null, this); x.Handled = true; } };
                    }
                    else if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.MiddleClick)
                    {
                        _mouseDrag.MouseClickEventHandler += (s, x) => { if (gesture.Matches(this, x)) { e.Value.Execute(null, this); x.Handled = true; } };
                    }
                    else if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.RightClick)
                    {
                        _mouseGesture.MouseClickEventHandler += (s, x) => { if (gesture.Matches(this, x)) { e.Value.Execute(null, this); x.Handled = true; } };
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
                    _mouseGesture.CommandCollection.Add(mouseGesture, e.Value);
                }
            }

            // drag key
            _mouseDrag.SetKeyBindings(ModelContext.DragActionTable.GetKeyBinding());

            // context menu gesture
            if (_VM.ContextMenuSetting.IsEnabled && _VM.ContextMenuSetting.IsOpenByGesture)
            {
                _mouseGesture.AddOpenContextMenuGesture(_VM.ContextMenuSetting.MouseGesture);
            }

            // Update Menu GestureText
            _VM.MainMenu?.UpdateInputGestureText();
            _VM.ContextMenu?.UpdateInputGestureText();
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
            bool isScrolled = _mouseDrag.ScrollN(-1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation);

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
            bool isScrolled = _mouseDrag.ScrollN(+1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation);

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

            var dialog = new SettingWindow(setting, history);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                SetUpdateMenuLayoutMode(false);
                _VM.RestoreSetting(setting, false);
                SetUpdateMenuLayoutMode(true);
                _VM.SaveSetting(this);
                ModelContext.BookHistory.Restore(history, false);
            }
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

        // フルスクリーン前の状態保持
        private WindowState _windowStateMemento = WindowState.Normal;
        private bool _fullScreened;

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

        // メニューレイアウト更新
        private void UpdateMenuLayout()
        {
            _IsDartyMenuLayout = false;

            // window style
            if (_VM.IsFullScreen || !_VM.IsVisibleTitleBar)
            {
                this.WindowStyle = WindowStyle.None;
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }

            // fullscreen 
            if (_VM.IsFullScreen != _fullScreened)
            {
                _fullScreened = _VM.IsFullScreen;
                if (_VM.IsFullScreen)
                {
                    this.ResizeMode = System.Windows.ResizeMode.NoResize;
                    _windowStateMemento = this.WindowState;
                    if (this.WindowState == WindowState.Maximized) this.WindowState = WindowState.Normal;
                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    this.ResizeMode = System.Windows.ResizeMode.CanResize;
                    this.WindowState = _windowStateMemento;
                }
            }

            // menu hide
            bool isMenuDock = !_VM.IsHideMenu && !_VM.IsFullScreen;
            bool isPageSliderDock = !_VM.IsHidePageSlider && !_VM.IsFullScreen;

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

            // アドレスバー
            this.AddressBar.Visibility = _VM.IsVisibleAddressBar ? Visibility.Visible : Visibility.Collapsed;

            // サムネイルリスト
            SetThumbnailListAreaVisibisity(_VM.IsEnableThumbnailList, true);

            DartyThumbnailList();
            UpdateStateAreaVisibility();

            double statusAreaHeight = this.PageSlider.Height + _VM.ThumbnailItemHeight; // アバウト
            double bottomMargin = (isPageSliderDock && _VM.IsEnableThumbnailList && !_VM.IsHideThumbnailList ? statusAreaHeight : this.PageSlider.Height);
            this.LeftPanelMargin.Height = bottomMargin;
            this.RightPanelMargin.Height = bottomMargin;

            // パネル表示設定
            UpdateLeftPanelVisibility();
            UpdateRightPanelVisibility();

            //
            UpdateMenuAreaVisibility();

            // コントロール表示状態更新
            {
                SetControlVisibility(this.LeftPanel, _isVisibleLeftPanel, true, VisibleStoryboardType.Collapsed);
                SetControlVisibility(this.RightPanel, _isVisibleRightPanel, true, VisibleStoryboardType.Collapsed);

                SetControlVisibility(this.MenuArea, _isMenuAreaVisibility, true, VisibleStoryboardType.Opacity);

                SetControlVisibility(this.ThumbnailListArea, _isVisibleThumbnailList, true, VisibleStoryboardType.Collapsed);
                SetControlVisibility(this.StatusArea, _isVisibleStatausArea, true, VisibleStoryboardType.Collapsed);
            }

            // 再計算
            this.UpdateLayout();

            // ビュー領域設定
            this.ViewArea.Margin = new Thickness(0, isMenuDock ? this.MenuArea.ActualHeight : 0, 0, 0);

            // コンテンツ表示領域設定
            this.MainView.Margin = new Thickness(0, 0, 0, isPageSliderDock ? bottomMargin : 0);

            // 通知表示位置設定
            this.TinyInfoTextBlock.Margin = new Thickness(0, 0, 0, bottomMargin);
            this.InfoTextAreaBase.Margin = new Thickness(0, 0, 0, bottomMargin);
            this.NowLoadingTiny.Margin = new Thickness(0, 0, 0, bottomMargin);
        }


        //
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (!App.Options["--reset-placement"].IsValid && App.Setting.ViewMemento.IsSaveWindowPlacement)
            {
                // ウィンドウ座標復元 (スレッドスリープする)
                WindowPlacement.Restore(this, App.Setting.WindowPlacement, App.Setting.ViewMemento.IsFullScreen);
            }
        }

        //
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // VMイベント設定
            InitializeViewModelEvents();

            SetUpdateMenuLayoutMode(false);

            // 設定反映
            _VM.RestoreSetting(App.Setting, true);

            // 履歴読み込み
            _VM.LoadHistory(App.Setting);

            // ブックマーク読み込み
            _VM.LoadBookmark(App.Setting);

            // ページマーク読込
            _VM.LoadPagemark(App.Setting);

            App.Setting = null; // ロード設定破棄

            // パネル幅復元
            this.LeftPanel.Width = _VM.LeftPanelWidth;
            this.RightPanel.Width = _VM.RightPanelWidth;


            // PanelColor
            _VM.FlushPanelColor();

            // DPI倍率設定
            App.Config.UpdateDpiScaleFactor(this);

            // オプションによるフルスクリーン指定
            if (App.Options["--fullscreen"].IsValid)
            {
                _VM.IsFullScreen = App.Options["--fullscreen"].Bool;
            }

            // ウィンドウモードで初期化
            OnMenuVisibilityChanged();
            SetUpdateMenuLayoutMode(true);


            // フォルダリスト初期化
            this.FolderList.SetPlace(ModelContext.BookHistory.LastFolder, null, false);
            this.PageList.Initialize(_VM);
            // 履歴リスト初期化
            this.HistoryArea.Initialize(_VM.BookHub);
            // ブックマークリスト初期化
            this.BookmarkArea.Initialize(_VM.BookHub);
            // ブックマークリスト初期化
            this.PagemarkArea.Initialize(_VM.BookHub);

            // マーカー初期化
            this.PageMarkers.Initialize(_VM.BookHub);

            // フォルダを開く
            if (!App.Options["--blank"].IsValid)
            {
                if (App.StartupPlace != null)
                {
                    // 起動引数の場所で開く
                    LoadAs(App.StartupPlace);
                }
                else
                {
                    // 最後に開いたフォルダを復元する
                    _VM.LoadLastFolder();
                }
            }

            // スライドショーの自動再生
            if (App.Options["--slideshow"].IsValid ? App.Options["--slideshow"].Bool : _VM.IsAutoPlaySlideShow)
            {
                _VM.BookHub.IsEnableSlideShow = true;
            }
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
            _mouseDrag.SnapView();
        }

        //
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _VM.SaveSetting(this); // ウィンドウサイズ保存のためこのタイミングで行う
        }

        //
        private void Window_Closed(object sender, EventArgs e)
        {
            Temporary.RemoveTempFolder();
            _VM.Dispose();

            Debug.WriteLine("Window.Closed done.");
            //Environment.Exit(0);
        }


        // 開発用コマンド：テンポラリフォルダを開く
        private void MenuItemDevTempFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + Temporary.TempDirectory + "\"");
        }

        // 開発用コマンド：アプリケーションフォルダを開く
        private void MenuItemDevApplicationFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + System.Reflection.Assembly.GetEntryAssembly().Location + "\"");
        }

        // 開発用コマンド：アプリケーションデータフォルダを開く
        private void MenuItemDevApplicationDataFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + App.Config.LocalApplicationDataPath + "\"");
        }

        // 開発用コマンド：コンテンツ座標更新
        private void UpdateContentPoint_Click(object sender, RoutedEventArgs e)
        {
            _VM.UpdateContentPosition();
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
                var ani = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
                this.NowLoading.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 90;
                aniRotate.Duration = TimeSpan.FromSeconds(0.5);
                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
        }

        // オンラインヘルプ
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }

        // 情報エリアクリックでメインビューにフォーカスを移す
        private void InfoArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.MainView.Focus();
        }

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


        // [開発用] テストボタン
        private void MenuItemDevButton_Click(object sender, RoutedEventArgs e)
        {
            //ModelContext.CommandTable.OpenCommandListHelp();
            //App.Config.RemoveApplicationData();
        }



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



        // ステータスエリアの表示判定
        private void UpdateStateAreaVisibility()
        {
            if (_VM.IsHidePageSlider || _VM.IsFullScreen)
            {
                SetStatusAreaVisibisity(IsStateAreaMouseOver(), false);
                SetThumbnailListAreaVisibisity(_VM.IsEnableThumbnailList, true);
            }
            else
            {
                SetStatusAreaVisibisity(true, true);
                SetThumbnailListAreaVisibisity(_VM.IsEnableThumbnailList && (!_VM.CanHideThumbnailList || IsStateAreaMouseOver()), false);
            }
        }

        //
        private bool IsStateAreaMouseOver()
        {
            const double visibleMargin = 32;
            const double hideMargin = 8;

            Point point = Mouse.GetPosition(this.Root);
            if (this.StatusArea.IsVisible)
            {
                double margin = this.StatusArea.ActualHeight + hideMargin > visibleMargin ? this.StatusArea.ActualHeight + hideMargin : visibleMargin;
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
                SetControlVisibility(this.StatusArea, _isVisibleStatausArea, isQuickly, VisibleStoryboardType.Collapsed);

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

        #endregion


        private void UpdateMenuAreaVisibility()
        {
            const double visibleMargin = 32;
            const double hideMargin = 8;

            if (_VM.IsHideMenu || _VM.IsFullScreen)
            {
                Point point = Mouse.GetPosition(this.Root);
                bool isVisible = this.MenuArea.IsMouseOver || this.AddressTextBox.IsFocused;
                if (this.MenuArea.Opacity >= 0.99) //IsVisible)
                {
                    double margin = this.MenuArea.ActualHeight + hideMargin > visibleMargin ? this.MenuArea.ActualHeight + hideMargin : visibleMargin;
                    isVisible = isVisible || (point.Y < 0.0 + margin && this.IsMouseOver);
                }
                else
                {
                    isVisible = isVisible || point.Y < 0.0 + visibleMargin && this.IsMouseOver;
                }
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
                SetControlVisibility(this.MenuArea, _isMenuAreaVisibility, isQuickly, VisibleStoryboardType.Opacity);
            }
        }


        #region Panel Visibility

        //
        private bool _isVisibleLeftPanel;
        private bool _isVisibleRightPanel;

        // ViewAreaでのマウス移動
        private void ViewArea_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateControlsVisibility();
        }

        //
        private void UpdateControlsVisibility()
        {
            UpdateLeftPanelVisibility();
            UpdateRightPanelVisibility();
            UpdateMenuAreaVisibility();
            UpdateStateAreaVisibility();
        }


        //
        private void UpdateLeftPanelVisibility()
        {
            const double visibleMargin = 32;
            const double hideMargin = 16;

            if (_VM.LeftPanel == PanelType.None)
            {
                SetLeftPanelVisibisity(false, false);
            }
            else if (_VM.CanHidePanel)
            {
                Point point = Mouse.GetPosition(this.ViewArea);
                if (this.LeftPanel.IsVisible)
                {
                    double margin = this.LeftPanel.Width + hideMargin > visibleMargin ? this.LeftPanel.Width + hideMargin : visibleMargin;
                    SetLeftPanelVisibisity(this.LeftPanel.IsMouseOver || _IsContextMenuOpened || point.X < margin && this.IsMouseOver, false);
                }
                else if (point.X < visibleMargin && this.IsMouseOver && !this.MenuArea.IsMouseOver && !this.StatusArea.IsMouseOver)
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
                else if (point.X > this.ViewArea.ActualWidth - visibleMargin && this.IsMouseOver && !this.MenuArea.IsMouseOver && !this.StatusArea.IsMouseOver)
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
            if (e.Key == Key.Escape)
            {
                _VM.LeftPanel = PanelType.None;
                e.Handled = true;
            }
        }

        private void RightPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _VM.RightPanel = PanelType.None;
                e.Handled = true;
            }
        }

        private void ThumbnailListBox_Loaded(object sender, RoutedEventArgs e)
        {
            // nop.
        }

        private void ThumbnailListBoxPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // パネルコントロール取得
            if (_thumbnailListPanel == null)
            {
                _thumbnailListPanel = sender as VirtualizingStackPanel;
                DartyThumbnailList();
            }
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


        private void LeftPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _VM.IsVisibleLeftPanel = this.LeftPanel.IsVisible;
        }

        private void RightPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _VM.IsVisibleRightPanel = this.RightPanel.IsVisible;
        }

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
            catch(Exception e)
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
            string path = (string)value;
            return NVUtility.PlaceToTitle(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // コンバータ：フォルダの並びフラグ
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

            return length / App.Config.DpiScaleFactor.X;
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

            return length / App.Config.DpiScaleFactor.Y;
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

    #endregion
}
