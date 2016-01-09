using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        MainWindowVM _VM;

        public FullScreen _WindowMode;

        public MouseDragController _MouseDragController;
        public MouseGestureManager _MouseGesture;

        bool _NowLoading = false;

        public MainWindow()
        {
            App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            InitializeComponent();

            _VM = new MainWindowVM();
            _VM.ViewChanged += OnViewChanged;
            _VM.ViewModeChanged += OnViewModeChanged;
            _VM.InputGestureChanged += (s, e) => InitializeInputGestures();
            _VM.PropertyChanged += OnPropertyChanged;
            _VM.Loading += (s, e) =>
            {
                //this.Cursor = e != null ? Cursors.Wait : null;
                _NowLoading = e != null;
                //this.Root.IsEnabled = e == null;
                DispNowLoading(e);
            };


            this.DataContext = _VM;

            _WindowMode = new FullScreen(this);
            _WindowMode.NotifyWindowModeChanged += (s, e) => OnWindowModeChanged(e);

            _MouseDragController = new MouseDragController(this.MainView, this.MainContent, this.MainContentShadow);

            _MouseGesture = new MouseGestureManager(this.MainView);
            this.GestureTextBlock.SetBinding(TextBlock.TextProperty, new Binding("GestureText") { Source = _MouseGesture });
            _MouseGesture.Controller.MouseGestureUpdateEventHandler += OnMouseGestureUpdate;

            InitializeCommandBindings();
            InitializeInputGestures();

            _VM.BookCommands = BookCommands;

            // messenger
            Messenger.AddReciever("MessageBox", CallMessageBox);
            Messenger.AddReciever("MessageShow", CallMessageShow);

            this.MainView.PreviewMouseMove += MainView_PreviewMouseMove;

            // タイマーを作成する
            _Timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            _Timer.Interval = TimeSpan.FromSeconds(0.2);
            _Timer.Tick += new EventHandler(DispatcherTimer_Tick);
            // タイマーの実行開始
            _Timer.Start();
        }

        DispatcherTimer _Timer;

        DateTime _LastActionTime;
        Point _LastActionPoint;

        DateTime _LastShowTime;


        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine($"Interval: {DateTime.Now.Second}.{DateTime.Now.Millisecond}");

            if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
            {
                _LastActionTime = DateTime.Now;
                _LastShowTime = DateTime.Now;
                return;
            }

            if ((DateTime.Now - _LastActionTime).TotalSeconds > 2.0)
            {
                SetMouseVisible(false);
                _LastActionTime = DateTime.Now;
            }


            if ((DateTime.Now - _LastShowTime).TotalSeconds > _VM.SlideShowInterval)
            {
                //Debug.WriteLine($"SlideShow: {DateTime.Now.Second}.{DateTime.Now.Millisecond}");
                if (!_NowLoading) _VM.NextSlide();
                _LastShowTime = DateTime.Now;
            }
        }

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

        private void OnMouseGestureUpdate(object sender, MouseGestureSequence e)
        {
            _VM.ShowGesture(_MouseGesture.GetGestureString(), _MouseGesture.GetGestureCommandName());
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //case "InfoText":
                //    AutoFade(InfoTextArea, 1.0, 0.5);
                //    break;
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

            //throw new NotImplementedException();

            //this.MenuArea.Items.Refresh();
        }

        // ドラッグでのビュー操作の設定変更
        private void OnViewModeChanged(object sender, EventArgs e)
        {
            _MouseDragController.IsLimitMove = _VM.IsLimitMove;
            _MouseDragController.DragControlCenter = _VM.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
            _MouseDragController.SnapAngle = _VM.IsAngleSnap ? 45 : 0;
            //_MouseDragController.IsStartPositionCenter = _VM.IsViewStartPositionCenter;
            _MouseDragController.ViewOrigin = _VM.IsViewStartPositionCenter ? ViewOrigin.Center :
                _VM.BookSetting.BookReadOrder == PageReadOrder.LeftToRight ? ViewOrigin.LeftTop : ViewOrigin.RightTop;


            // ここはあまりよくない
            // _MouseDragController.Reset();
        }


        // new!
        public Dictionary<CommandType, RoutedUICommand> BookCommands { get; set; } = new Dictionary<CommandType, RoutedUICommand>();






        public void InitializeCommandBindings()
        {
            // コマンドインスタンス作成
            foreach (CommandType type in Enum.GetValues(typeof(CommandType)))
            {
                BookCommands.Add(type, new RoutedUICommand(_VM.CommandCollection[type].Text, type.ToString(), typeof(MainWindow)));
            }

            // スタティックコマンド
            //this.CommandBindings.Add(new CommandBinding(LoadCommand, Load));

            // View系コマンド登録
            _VM.CommandCollection[CommandType.OpenSettingWindow].Execute =
                (e) => OpenSettingWindow();
            _VM.CommandCollection[CommandType.LoadAs].Execute =
                (e) => LoadAs(e);
            _VM.CommandCollection[CommandType.ClearHistory].Execute =
                (e) => _VM.ClearHistor();
            _VM.CommandCollection[CommandType.ToggleFullScreen].Execute =
                (e) => _WindowMode.Toggle();
            _VM.CommandCollection[CommandType.SetFullScreen].Execute =
                (e) => _WindowMode.IsFullScreened = true;
            _VM.CommandCollection[CommandType.CancelFullScreen].Execute =
                (e) => _WindowMode.IsFullScreened = false;
            _VM.CommandCollection[CommandType.ViewScrollUp].Execute =
                (e) => _MouseDragController.ScrollUp();
            _VM.CommandCollection[CommandType.ViewScrollDown].Execute =
                (e) => _MouseDragController.ScrollDown();
            _VM.CommandCollection[CommandType.ViewScaleUp].Execute =
                (e) => _MouseDragController.ScaleUp();
            _VM.CommandCollection[CommandType.ViewScaleDown].Execute =
                (e) => _MouseDragController.ScaleDown();
            _VM.CommandCollection[CommandType.ViewRotateLeft].Execute =
                (e) => _MouseDragController.Rotate(-45);
            _VM.CommandCollection[CommandType.ViewRotateRight].Execute =
                (e) => _MouseDragController.Rotate(+45);

            // コマンドバインド作成
            foreach (CommandType type in Enum.GetValues(typeof(CommandType)))
            {
                // フルスクリーン系コマンドは常に有効
                if (type == CommandType.ToggleFullScreen || type == CommandType.CancelFullScreen)
                {
                    this.CommandBindings.Add(new CommandBinding(BookCommands[type], (t, e) => _VM.Execute(type, e.Parameter)));
                }
                else
                {
                    this.CommandBindings.Add(new CommandBinding(BookCommands[type], (t, e) => _VM.Execute(type, e.Parameter), CanExecute));
                }
            }
        }

        //
        private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_NowLoading;
        }



        // ダイアログでファイル選択して画像を読み込む
        private void LoadAs(object param)
        {
            string path = param as string;

            if (path == null)
            {
                var dialog = new OpenFileDialog();

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


        // 設定ウィンドウを開く
        private void OpenSettingWindow()
        {
            var setting = _VM.CreateSettingContext();

            var dialog = new SettingWindow(setting);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                _VM.SetSettingContext(setting);
            }
        }


        // InputGesture設定
        public void InitializeInputGestures()
        {
            _MouseDragController.ClearClickEventHandler();

            _MouseGesture.ClearClickEventHandler();
            _MouseGesture.CommandCollection.Clear();

            foreach (var e in BookCommands)
            {
                //var e = pair.Value;

                // shortcut key
                e.Value.InputGestures.Clear();
                var inputGestures = _VM.GetShortCutCollection(e.Key);
                foreach (var gesture in inputGestures)
                {
                    // マウスクリックはドラッグ系処理のイベントとして登録
                    if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.LeftClick)
                    {
                        _MouseDragController.MouseClickEventHandler += (s, x) => e.Value.Execute(null, this);
                    }
                    else if (gesture is MouseGesture && ((MouseGesture)gesture).MouseAction == MouseAction.RightClick)
                    {
                        _MouseGesture.MouseClickEventHandler += (s, x) => e.Value.Execute(null, this);
                    }
                    else
                    {
                        e.Value.InputGestures.Add(gesture);
                    }
                }

                // mouse gesture
                var mouseGesture = _VM.GetMouseGesture(e.Key);
                if (mouseGesture != null)
                {
                    _MouseGesture.CommandCollection.Add(mouseGesture, e.Value);
                }
            }

            // Update Menu ...
            this.MainMenu.UpdateInputGestureText();

        }

        // 表示変更でマウスドラッグによる変形を初期化する
        private void OnViewChanged(object sender, EventArgs e)
        {
            _MouseDragController.Reset();
        }

        // スクリーンモード切り替えによるコントロール設定の変更
        private void OnWindowModeChanged(bool isFullScreened)
        {
            if (isFullScreened)
            {
                var autoHideStyle = (Style)this.Resources["AutoHideContent"];
                this.MenuArea.Style = autoHideStyle;
                this.StatusArea.Style = autoHideStyle;
                this.MainView.Margin = new Thickness(0);
            }
            else
            {
                this.MenuArea.Style = null;
                this.StatusArea.Style = null;
                this.MainView.Margin = new Thickness(0, this.MenuArea.ActualHeight, 0, this.StatusArea.ActualHeight);
            }

            this.TinyInfoTextBlock.Margin = new Thickness(0, 0, 0, this.StatusArea.ActualHeight);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // 設定読み込み
            _VM.LoadSetting(this);
        }

        //
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //_VM.LoadSetting(this);

            // 標準ウィンドウモードで初期化
            OnWindowModeChanged(false);
        }

        //
        private void MainWindow_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true) && !_NowLoading)
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        // ファイルのドラッグ＆ドロップで処理を開始する
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                //_VM.Load(files[0]);
                //_VM.CommandCollection[BookCommandType.LoadAs].Execute(files[0]);
                BookCommands[CommandType.LoadAs].Execute(files[0], this);
            }
        }

        // ウィンドウサイズが変化したらコンテンツサイズも追従する
        private void MainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _VM.SetViewSize(this.MainView.ActualWidth, this.MainView.ActualHeight);

            // スナップ
            _MouseDragController.SnapView();
        }

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            double margin = 5;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var slider = (Slider)sender;
                Point position = e.GetPosition(slider);
                double d = 1.0d / (slider.ActualWidth - margin * 2) * (position.X - margin);
                var p = slider.Maximum * (1.0 - d);
                slider.Value = p;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Temporary.RemoveTempFolder();
            _VM.Dispose();

            Debug.WriteLine("Window.Closed done.");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _VM.SaveSetting(this);
        }

        private void MenuItemDevTempFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + Temporary.TempDirectory + "\"");
        }


        private void CallMessageBox(object sender, MessageEventArgs e)
        {
            var param = (MessageBoxParams)e.Parameter;

            var dialog = new MessageBoxEx(param);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.AllowDrop = false;

            e.Result = dialog.ShowDialog();

            this.AllowDrop = true;

#if false
            var result = MessageBox.Show(this, param.MessageBoxText, param.Caption, param.Button, param.Icon);
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK)
            {
                e.Result = true;
            }
#endif
        }


        private void CallMessageShow(object sender, MessageEventArgs e)
        {
            var param = (MessageShowParams)e.Parameter;

            _VM.InfoText = param.Text;
            this.InfoBookmark.Visibility = param.IsBookmark ? Visibility.Visible : Visibility.Collapsed;
            AutoFade(this.InfoTextArea, param.DispTime, 0.5);
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


        private void DispNowLoading(string loadPath)
        {
            if (loadPath != null)
            {
                this.NowLoading.Opacity = 0.0;
                //this.NowLoadingText.Text = $"Now Loading\n{System.IO.Path.GetFileName(loadPath)}";

                var ani = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
                ani.BeginTime = TimeSpan.FromSeconds(1.0);
                this.NowLoading.BeginAnimation(UIElement.OpacityProperty, ani);

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 360;
                aniRotate.Duration = TimeSpan.FromSeconds(2.0);
                aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
            else
            {
                var ani = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
                this.NowLoading.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.Compose);

                var aniRotate = new DoubleAnimation();
                aniRotate.By = 90;
                aniRotate.Duration = TimeSpan.FromSeconds(0.5);
                this.NowLoadingMarkAngle.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

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

    [ValueConversion(typeof(int), typeof(bool))]
    public class PageModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int mode0 = (int)value;
            int mode1 = int.Parse(parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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

    [ValueConversion(typeof(string), typeof(string))]
    public class FullPathToFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = (string)value;
            return LoosePath.GetFileName(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


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

}
