// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM _VM;

        private MouseDragController _MouseDrag;
        private MouseGestureManager _MouseGesture;

        private ContentDropManager _ContentDrop = new ContentDropManager();

        private bool _NowLoading = false;

        public MouseDragController MouseDragController => _MouseDrag;


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

            _VM.ViewChanged +=
                (s, e) =>
                {
                    UpdateMouseDragSetting(e.PageDirection);
                    bool isResetScale = e.ResetViewTransform || !_VM.IsKeepScale;
                    bool isResetAngle = e.ResetViewTransform || !_VM.IsKeepAngle;
                    bool isResetFlip = e.ResetViewTransform || !_VM.IsKeepFlip;
                    _MouseDrag.Reset(isResetScale, isResetAngle, isResetFlip);
                };

            _VM.InputGestureChanged +=
                (s, e) => InitializeInputGestures();

            _VM.PropertyChanged +=
                OnPropertyChanged;

            _VM.Loading +=
                (s, e) =>
                {
                    _NowLoading = e != null;
                    DispNowLoading(_NowLoading);
                };

            _VM.NotifyMenuVisibilityChanged +=
                (s, e) => OnMenuVisibilityChanged();

            _VM.ContextMenuEnableChanged +=
                (s, e) =>
                {
                    _MouseGesture.Controller.ContextMenuSetting = _VM.ContextMenuSetting;
                };

            _VM.PageListChanged +=
                OnPageListChanged;

            _VM.IndexChanged +=
                OnIndexChanged;

            this.DataContext = _VM;


            // mouse drag
            _MouseDrag = new MouseDragController(this, this.MainView, this.MainContent, this.MainContentShadow);
            _MouseDrag.TransformChanged +=
                (s, e) =>
                {
                    _VM.SetViewTransform(_MouseDrag.Scale, _MouseDrag.Angle);
                    if (e == TransformChangeType.Scale)
                    {
                        _VM.UpdateWindowTitle(UpdateWindowTitleMask.View);
                    }
                };
            ModelContext.DragActionTable.SetTarget(_MouseDrag);

            // mouse gesture
            _MouseGesture = new MouseGestureManager(this.MainView);
            _MouseGesture.Controller.MouseGestureUpdateEventHandler += OnMouseGestureUpdate;

            // initialize routed commands
            InitializeCommandBindings();
            InitializeInputGestures();

            // publish routed commands
            _VM.BookCommands = BookCommands;

            // MainMenu Initialize
            _VM.MainMenuInitialize();

            // messenger
            Messenger.AddReciever("MessageBox", CallMessageBox);
            Messenger.AddReciever("MessageShow", CallMessageShow);
            Messenger.AddReciever("Export", CallExport);

            // mouse event capture for active check
            this.MainView.PreviewMouseMove += MainView_PreviewMouseMove;
            this.MainView.PreviewMouseDown += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseUp += MainView_PreviewMouseAction;
            this.MainView.PreviewMouseWheel += MainView_PreviewMouseAction;

            // timer for slideshow
            _Timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            _Timer.Interval = TimeSpan.FromSeconds(0.2);
            _Timer.Tick += new EventHandler(DispatcherTimer_Tick);
            _Timer.Start();
        }

        //
        private void OnIndexChanged(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                UpdateThumbnailList(_VM.Index, _VM.IndexMax);
            });
        }

        //
        private void OnPageListChanged(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                this.ThumbnailListBox.Items.Refresh();
                this.ThumbnailListBox.UpdateLayout();
                UpdateThumbnailList(_VM.Index, _VM.IndexMax);
                LoadThumbnailList(+1);
            });
        }


        #region Timer

        // タイマーディスパッチ
        DispatcherTimer _Timer;

        // 非アクティブ時間チェック用
        DateTime _LastActionTime;
        Point _LastActionPoint;

        // スライドショー表示間隔用
        DateTime _LastShowTime;

        // タイマー処理
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                _LastActionTime = DateTime.Now;
                _LastShowTime = DateTime.Now;
                return;
            }

            // 非アクティブ時間が続いたらマウスカーソルを非表示にする
            if ((DateTime.Now - _LastActionTime).TotalSeconds > 2.0)
            {
                SetMouseVisible(false);
                _LastActionTime = DateTime.Now;
            }

            // スライドショーのインターバルを非アクティブ時間で求める
            if ((DateTime.Now - _LastShowTime).TotalSeconds > _VM.SlideShowInterval)
            {
                if (!_NowLoading) _VM.NextSlide();
                _LastShowTime = DateTime.Now;
            }
        }

        // マウス移動で非アクティブ時間リセット
        private void MainView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var nowPoint = e.GetPosition(this.MainView);

            if (Math.Abs(nowPoint.X - _LastActionPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(nowPoint.Y - _LastActionPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _LastActionTime = DateTime.Now;
                if (_VM.IsCancelSlideByMouseMove)
                {
                    _LastShowTime = DateTime.Now;
                }
                _LastActionPoint = nowPoint;
                SetMouseVisible(true);
            }
        }

        // マウスアクションで非アクティブ時間リセット
        private void MainView_PreviewMouseAction(object sender, MouseEventArgs e)
        {
            _LastActionTime = DateTime.Now;
            _LastShowTime = DateTime.Now;
            SetMouseVisible(true);
        }

        // マウスカーソル表示ON/OFF
        public void SetMouseVisible(bool isVisible)
        {
            if (isVisible)
            {
                if (this.MainView.Cursor == Cursors.None)
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
            _VM.ShowGesture(_MouseGesture.GetGestureString(), _MouseGesture.GetGestureCommandName());
        }

        // VMプロパティ監視
        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TinyInfoText":
                    AutoFade(TinyInfoTextBlock, 1.0, 0.5);
                    break;
                case "IsSliderDirectionReversed":
                    // Retrieve the Track from the Slider control
                    var track = this.PageSlider.Template.FindName("PART_Track", this.PageSlider) as System.Windows.Controls.Primitives.Track;
                    // Force it to rerender
                    track.InvalidateVisual();
                    break;
            }
        }


        // ドラッグでビュー操作設定の更新
        private void UpdateMouseDragSetting(int direction)
        {
            _MouseDrag.IsLimitMove = _VM.IsLimitMove;
            _MouseDrag.DragControlCenter = _VM.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
            _MouseDrag.SnapAngle = _VM.IsAngleSnap ? 45 : 0;

            var origin = _VM.IsViewStartPositionCenter ? DragViewOrigin.Center : _VM.BookSetting.BookReadOrder == PageReadOrder.LeftToRight ? DragViewOrigin.LeftTop : DragViewOrigin.RightTop;
            _MouseDrag.ViewOrigin = direction < 0 ? origin.Reverse() : origin;
            _MouseDrag.ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop) ? 1.0 : -1.0;
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
                (e) => OpenSettingWindow();
            ModelContext.CommandTable[CommandType.OpenVersionWindow].Execute =
                (e) => OpenVersionWindow();
            ModelContext.CommandTable[CommandType.CloseApplication].Execute =
                (e) => Close();
            ModelContext.CommandTable[CommandType.LoadAs].Execute =
                (e) => LoadAs(e);
            ModelContext.CommandTable[CommandType.ViewScrollUp].Execute =
                (e) => _MouseDrag.ScrollUp();
            ModelContext.CommandTable[CommandType.ViewScrollDown].Execute =
                (e) => _MouseDrag.ScrollDown();
            ModelContext.CommandTable[CommandType.ViewScaleUp].Execute =
                (e) => _MouseDrag.ScaleUp();
            ModelContext.CommandTable[CommandType.ViewScaleDown].Execute =
                (e) => _MouseDrag.ScaleDown();
            ModelContext.CommandTable[CommandType.ViewRotateLeft].Execute =
                (e) => _MouseDrag.Rotate(-45);
            ModelContext.CommandTable[CommandType.ViewRotateRight].Execute =
                (e) => _MouseDrag.Rotate(+45);
            ModelContext.CommandTable[CommandType.ToggleViewFlipHorizontal].Execute =
                (e) => _MouseDrag.ToggleFlipHorizontal();
            ModelContext.CommandTable[CommandType.ViewFlipHorizontalOn].Execute =
                (e) => _MouseDrag.FlipHorizontal(true);
            ModelContext.CommandTable[CommandType.ViewFlipHorizontalOff].Execute =
                (e) => _MouseDrag.FlipHorizontal(false);

            ModelContext.CommandTable[CommandType.ToggleViewFlipVertical].Execute =
                (e) => _MouseDrag.ToggleFlipVertical();
            ModelContext.CommandTable[CommandType.ViewFlipVerticalOn].Execute =
                (e) => _MouseDrag.FlipVertical(true);
            ModelContext.CommandTable[CommandType.ViewFlipVerticalOff].Execute =
                (e) => _MouseDrag.FlipVertical(false);

            ModelContext.CommandTable[CommandType.ViewReset].Execute =
                (e) => _MouseDrag.Reset(true, true, true);
            ModelContext.CommandTable[CommandType.PrevScrollPage].Execute =
                (e) => PrevScrollPage();
            ModelContext.CommandTable[CommandType.NextScrollPage].Execute =
                (e) => NextScrollPage();
            ModelContext.CommandTable[CommandType.MovePageWithCursor].Execute =
                (e) => MovePageWithCursor();

            ModelContext.CommandTable[CommandType.MovePageWithCursor].ExecuteMessage =
                (e) => MovePageWithCursorMessage();


            // コマンドバインド作成
            foreach (CommandType type in Enum.GetValues(typeof(CommandType)))
            {
                if (ModelContext.CommandTable[type].CanExecute != null)
                {
                    this.CommandBindings.Add(new CommandBinding(BookCommands[type], (t, e) => _VM.Execute(type, e.Parameter),
                        (t, e) => e.CanExecute = ModelContext.CommandTable[type].CanExecute()));
                }
                else
                {
                    this.CommandBindings.Add(new CommandBinding(BookCommands[type], (t, e) => _VM.Execute(type, e.Parameter),
                        CanExecute));
                }
            }
        }

        // ロード中のコマンドを無効にする CanExecute
        private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_NowLoading;
        }


        // InputGesture設定
        public void InitializeInputGestures()
        {
            _MouseDrag.ClearClickEventHandler();

            _MouseGesture.ClearClickEventHandler();
            _MouseGesture.CommandCollection.Clear();

            foreach (var e in BookCommands)
            {
                e.Value.InputGestures.Clear();
                var inputGestures = ModelContext.CommandTable[e.Key].GetInputGestureCollection();
                foreach (var gesture in inputGestures)
                {
                    // マウスクリックはドラッグ系処理のイベントとして登録
                    if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.LeftClick)
                    {
                        _MouseDrag.MouseClickEventHandler += (s, x) => { if (gesture.Matches(this, x)) { e.Value.Execute(null, this); x.Handled = true; } };
                    }
                    else if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.MiddleClick)
                    {
                        _MouseDrag.MouseClickEventHandler += (s, x) => { if (gesture.Matches(this, x)) { e.Value.Execute(null, this); x.Handled = true; } };
                    }
                    else if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.RightClick)
                    {
                        _MouseGesture.MouseClickEventHandler += (s, x) => { if (gesture.Matches(this, x)) { e.Value.Execute(null, this); x.Handled = true; } };
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
                    _MouseGesture.CommandCollection.Add(mouseGesture, e.Value);
                }
            }

            // drag key
            _MouseDrag.SetKeyBindings(ModelContext.DragActionTable.GetKeyBinding());

            // context menu gesture
            if (_VM.ContextMenuSetting.IsEnabled && _VM.ContextMenuSetting.IsOpenByGesture)
            {
                _MouseGesture.AddOpenContextMenuGesture(_VM.ContextMenuSetting.MouseGesture);
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
            bool isScrolled = (_VM.BookHub.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? _MouseDrag.ScrollRight() : _MouseDrag.ScrollLeft();

            if (!isScrolled)
            {
                _VM.BookHub.PrevPage();
            }
        }

        // スクロール＋次のページに進む
        private void NextScrollPage()
        {
            bool isScrolled = (_VM.BookHub.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? _MouseDrag.ScrollLeft() : _MouseDrag.ScrollRight();

            if (!isScrolled)
            {
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
                _VM.RestoreSetting(setting);
                _VM.SaveSetting(this);
                ModelContext.BookHistory.Restore(history);
            }
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
        WindowState _WindowStateMemento = WindowState.Normal;
        bool _FullScreened;

        // TODO: 呼びだされすぎ
        // スクリーンモード切り替えによるコントロール設定の変更
        private void OnMenuVisibilityChanged()
        {
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
            if (_VM.IsFullScreen != _FullScreened)
            {
                _FullScreened = _VM.IsFullScreen;
                if (_VM.IsFullScreen)
                {
                    _WindowStateMemento = this.WindowState;
                    if (this.WindowState == WindowState.Maximized) this.WindowState = WindowState.Normal;
                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    this.WindowState = _WindowStateMemento;
                }
            }

            bool isMenuDock;

            // menu hide
            if (_VM.IsHideMenu || _VM.IsFullScreen)
            {
                var autoHideStyle = (Style)this.Resources["AutoHideContent"];
                this.MenuArea.Style = autoHideStyle;
                this.StatusArea.Style = autoHideStyle;
                isMenuDock = false;
            }
            else
            {
                this.MenuArea.Style = null;
                this.StatusArea.Style = null;
                isMenuDock = true;
            }

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
            this.ThumbnailListArea.Visibility = _VM.IsEnableThumbnailList ? Visibility.Visible : Visibility.Collapsed;
            this._ThumbnailListPanel.FlowDirection = _VM.IsSliderDirectionReversed ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            UpdateThumbnailList();
            UpdateThumbnailListVisibility();

            double statusAreaHeight = this.PageSlider.ActualHeight + _VM.ThumbnailItemHeight; // アバウト
            double bottomMargin = (isMenuDock && _VM.IsEnableThumbnailList && !_VM.IsHideThumbnailList ? statusAreaHeight : this.PageSlider.ActualHeight);
            this.LeftPanelMargin.Height = bottomMargin;
            this.RightPanelMargin.Height = bottomMargin;

            // パネル表示設定
            UpdatePanelVisibility(true);

            // 再計算
            this.UpdateLayout();

            // ビュー領域設定
            this.ViewArea.Margin = new Thickness(0, isMenuDock ? this.MenuArea.ActualHeight : 0, 0, 0);

            // コンテンツ表示領域設定
            this.MainView.Margin = new Thickness(0, 0, 0, isMenuDock ? bottomMargin : 0);

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
            // パネルコントロール取得
            _ThumbnailListPanel = FindVisualChild<VirtualizingStackPanel>(this.ThumbnailListBox);

            // 設定反映
            _VM.RestoreSetting(App.Setting);

            // 履歴読み込み
            _VM.LoadHistory(App.Setting);

            // ブックマーク読み込み
            _VM.LoadBookmark(App.Setting);

            App.Setting = null; // ロード設定破棄

            // パネル幅復元
            this.LeftPanel.Width = _VM.LeftPanelWidth;
            this.RightPanel.Width = _VM.RightPanelWidth;

            // PanelColor
            _VM.FlushPanelColor();

            // DPI倍率設定
            _VM.UpdateDpiScaleFactor(this);

            // オプションによるフルスクリーン指定
            if (App.Options["--fullscreen"].IsValid)
            {
                _VM.IsFullScreen = App.Options["--fullscreen"].Bool;
            }

            // ウィンドウモードで初期化
            OnMenuVisibilityChanged();

            // フォルダリスト初期化
            this.FolderListArea.SetPlace(ModelContext.BookHistory.LastFolder, null, false);
            // 履歴リスト初期化
            this.HistoryArea.Initialize(_VM.BookHub);
            // ブックマークリスト初期化
            this.BookmarkArea.Initialize(_VM.BookHub);
            // ページリスト初期化
            this.PageArea.Initialize(_VM.BookHub);

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
            if (!_NowLoading && _ContentDrop.CheckDragContent(sender, e))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        // ドラッグ＆ドロップで処理を開始する
        private async void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (_NowLoading) return;

            try
            {
                string path = await _ContentDrop.DropAsync(sender, e, _VM.DownloadPath, (string message) => _VM.OnLoading(this, message));
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
            _MouseDrag.SnapView();
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
        private bool isDispNowLoading = false;

        /// <summary>
        /// NowLoadinの表示/非表示
        /// </summary>
        /// <param name="isDisp"></param>
        private void DispNowLoading(bool isDisp)
        {
            if (isDispNowLoading == isDisp) return;
            isDispNowLoading = isDisp;

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
            //throw new NotImplementedException();

            double rate = (this.PageSlider.Value - this.PageSlider.Minimum) / (this.PageSlider.Maximum - this.PageSlider.Minimum);

            // ListBoxからAutomationPeerを取得
            var peer = ItemsControlAutomationPeer.CreatePeerForElement(this.ThumbnailListBox);
            // GetPatternでIScrollProviderを取得
            var scrollProvider = peer.GetPattern(PatternInterface.Scroll) as IScrollProvider;
            scrollProvider.SetScrollPercent(rate * 100.0, scrollProvider.VerticalScrollPercent);
        }


        // TODO: クラス化
        #region thumbnail list

        // サムネイルリストのパネルコントロール
        private VirtualizingStackPanel _ThumbnailListPanel;

        private void ThumbnailListArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateThumbnailList();
        }

        //
        private void UpdateThumbnailList()
        {
            //UpdateThumbnailList(this.ThumbnailListBox.SelectedIndex, this.ThumbnailListBox.Items.Count);
            UpdateThumbnailList(_VM.Index, _VM.IndexMax);
        }

        //
        private void UpdateThumbnailList(int index, int indexMax)
        {
            if (_ThumbnailListPanel == null) return;

            if (!_VM.IsEnableThumbnailList) return;

            // 非表示時は処理なし
            if (!this.ThumbnailListArea.IsVisible) return;

            // リストボックス項目と同期がまだ取れていなければ処理しない
            //if (indexMax + 1 != this.ThumbnailListBox.Items.Count) return;

            // 項目の幅 取得
            var listBoxItem = this.ThumbnailListBox.ItemContainerGenerator.ContainerFromIndex((int)_ThumbnailListPanel.HorizontalOffset) as ListBoxItem;
            double itemWidth = (listBoxItem != null) ? listBoxItem.ActualWidth : 0.0;
            if (itemWidth <= 0.0) return;

            // 表示領域の幅
            double panelWidth = this.ThumbnailListArea.ActualWidth;

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
            _ThumbnailListPanel.SetHorizontalOffset(topIndex);

            // 選択
            this.ThumbnailListBox.SelectedIndex = index;

            // ##
            //Debug.WriteLine(topIndex + " / " + this.ThumbnailListBox.Items.Count);

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
            if (_ThumbnailListPanel == null || this.ThumbnailListBox.SelectedIndex < 0) return;

            if (_ThumbnailListPanel.FlowDirection == FlowDirection.RightToLeft)
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
            if (_ThumbnailListPanel != null && this.ThumbnailListBox.Items.Count > 0)
            {
                LoadThumbnailList(e.HorizontalChange < 0 ? -1 : +1);
            }
        }

        // サムネ更新。表示されているページのサムネの読み込み要求
        private void LoadThumbnailList(int direction)
        {
            if (_ThumbnailListPanel != null)
            {
                _VM.RequestThumbnail((int)_ThumbnailListPanel.HorizontalOffset, (int)_ThumbnailListPanel.ViewportWidth, 2, direction);
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

        // サムネイルリストの非表示判定
        private void UpdateThumbnailListVisibility()
        {
            if (_VM.IsEnableThumbnailList && _VM.IsHideThumbnailList)
            {
                Point point = Mouse.GetPosition(this.StatusArea);
                if (point.Y < 0.0 - 40.0)
                {
                    this.ThumbnailListArea.Visibility = Visibility.Collapsed;
                }
            }
        }

        // スライダーに乗ったら表示開始
        private void PageSlider_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_VM.IsEnableThumbnailList && !this.ThumbnailListArea.IsVisible)
            {
                this.ThumbnailListArea.Visibility = Visibility.Visible;
                UpdateThumbnailList();
            }
        }

        #endregion



        #region Panel Visibility

        //
        bool _IsVisibleLeftPanel;
        bool _IsVisibleRightPanel;

        // ViewAreaでのマウス移動
        private void ViewArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (_VM.CanHidePanel) UpdatePanelVisibility(false);

            // サムネイルリスト表示状態更新
            UpdateThumbnailListVisibility();
        }

        // パネルの表示ON/OFF更新
        private void UpdatePanelVisibility(bool isForce)
        {
            Point point = Mouse.GetPosition(this.ViewArea);

            bool inViewArea = this.ViewArea.IsMouseOver;

            const double visibleMargin = 20;
            const double hideMargin = 40;

            //
            bool isVisibleLeftpanel = _IsVisibleLeftPanel;
            if (point.X < visibleMargin && inViewArea)
            {
                isVisibleLeftpanel = true;
            }
            else if (point.X > this.LeftPanel.Width + hideMargin || !inViewArea)
            {
                isVisibleLeftpanel = false;
            }

            //
            bool isVisibleRightPanel = _IsVisibleRightPanel;
            if (point.X > this.ViewArea.ActualWidth - visibleMargin && inViewArea)
            {
                isVisibleRightPanel = true;
            }
            else if (point.X < this.ViewArea.ActualWidth - this.RightPanel.Width - hideMargin || !inViewArea)
            {
                isVisibleRightPanel = false;
            }

            //
            if (isForce || _IsVisibleLeftPanel != isVisibleLeftpanel)
            {
                _IsVisibleLeftPanel = isVisibleLeftpanel;
                UpdateLeftPanelVisibility();
            }

            //
            if (isForce || _IsVisibleRightPanel != isVisibleRightPanel)
            {
                _IsVisibleRightPanel = isVisibleRightPanel;
                UpdateRightPanelVisibility();
            }
        }

        // 左パネルの表示更新
        private void UpdateLeftPanelVisibility()
        {
            bool isVisible = _VM.LeftPanel != PanelType.None;

            if (_VM.CanHidePanel)
            {
                isVisible = isVisible && _IsVisibleLeftPanel;
            }

            this.LeftPanel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        // 右パネルの表示更新
        private void UpdateRightPanelVisibility()
        {
            bool isVisible = _VM.RightPanel != PanelType.None;

            if (_VM.CanHidePanel)
            {
                isVisible = isVisible && _IsVisibleRightPanel;
            }

            this.RightPanel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        //
        private void PageSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Debug.WriteLine("EVENT: " + e.NewValue);
        }

        private void PageSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //_VM.IsPermitSliderCall = true;
            //_VM.Index = (int)this.PageSlider.Value;
            //Debug.WriteLine("DECIDE");

            if (_VM.CanSliderLinkedThumbnailList)
            {
                _VM.SetIndex(_VM.Index);
            }
        }

        private void PageSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //_VM.IsPermitSliderCall = false;
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

    #endregion
}
