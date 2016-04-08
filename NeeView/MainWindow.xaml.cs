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

            this.DataContext = _VM;


            // mouse drag
            _MouseDrag = new MouseDragController(this, this.MainView, this.MainContent, this.MainContentShadow);
            _MouseDrag.TransformChanged +=
                (s, e) => _VM.SetViewTransform(_MouseDrag.Scale, _MouseDrag.Angle);
            ModelContext.DragActionTable.SetTarget(_MouseDrag);

            // mouse gesture
            _MouseGesture = new MouseGestureManager(this.MainView);
            _MouseGesture.Controller.MouseGestureUpdateEventHandler += OnMouseGestureUpdate;

            // initialize routed commands
            InitializeCommandBindings();
            InitializeInputGestures();

            // publish routed commands
            _VM.BookCommands = BookCommands;

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
                _LastShowTime = DateTime.Now;
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
        public Dictionary<CommandType, RoutedUICommand> BookCommands { get; set; } = new Dictionary<CommandType, RoutedUICommand>();


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
                        _MouseDrag.MouseClickEventHandler += (s, x) => { if (gesture.Matches(this, x)) e.Value.Execute(null, this); };
                    }
                    else if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.MiddleClick)
                    {
                        _MouseDrag.MouseClickEventHandler += (s, x) => { if (gesture.Matches(this, x)) e.Value.Execute(null, this); };
                    }
                    else if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.RightClick)
                    {
                        _MouseGesture.MouseClickEventHandler += (s, x) => { if (gesture.Matches(this, x)) e.Value.Execute(null, this); };
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

            // Update Menu ...
            this.MainMenu.UpdateInputGestureText();
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

        // フルスクリーン前の状態保持
        WindowState _WindowStateMemento = WindowState.Normal;
        bool _FullScreened;

        // スクリーンモード切り替えによるコントロール設定の変更
        private void OnMenuVisibilityChanged()
        {
            // window style
            if (_VM.IsFullScreen || _VM.IsHideTitleBar)
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
            if (_VM.IsFullScreen || _VM.IsHideMenu)
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

            // ウィンドウ表示設定
            this.FolderListArea.Visibility = _VM.IsVisibleFolderList ? Visibility.Visible : Visibility.Collapsed;
            this.InfoArea.Visibility = _VM.IsVisibleFileInfo ? Visibility.Visible : Visibility.Collapsed;

            // ビュー領域設定
            this.ViewArea.Margin = new Thickness(0, isMenuDock ? this.MenuArea.ActualHeight : 0, 0, 0);

            // コンテンツ表示領域設定
            this.MainView.Margin = new Thickness(0, 0, 0, isMenuDock ? this.StatusArea.ActualHeight : 0);

            // 通知表示位置設定
            this.TinyInfoTextBlock.Margin = new Thickness(0, 0, 0, this.StatusArea.ActualHeight);
            this.NowLoadingTiny.Margin = new Thickness(0, 0, 0, this.StatusArea.ActualHeight);
        }


        //
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // 設定読み込み
            _VM.LoadSetting(this);
        }

        //
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // PanelColor
            _VM.FlushPanelColor();

            // DPI倍率設定
            _VM.UpdateDpiScaleFactor(this);

            // 標準ウィンドウモードで初期化
            OnMenuVisibilityChanged();

            // フォルダリスト初期化
            this.FolderListArea.SetPlace(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), null);

            //
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


        // ドラッグ＆ドロップ前処理
        private void MainWindow_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (!_NowLoading && _ContentDrop.CheckDragContent(sender, e))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
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
            this.InfoBookmark.Visibility = param.IsBookmark ? Visibility.Visible : Visibility.Collapsed;
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
