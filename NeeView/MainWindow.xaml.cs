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

namespace NeeView
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowVM _VM;

        public WindowMode _WindowMode;

        public MouseDragController _MouseDragController;
        public MouseGestureEx _MouseGesture;

        public MainWindow()
        {
            InitializeComponent();

            _VM = new MainWindowVM();
            _VM.ViewChanged += OnViewChanged;
            _VM.ViewModeChanged += OnViewModeChanged;
            _VM.InputGestureChanged += (s, e) => InitializeInputGestures();
            _VM.PropertyChanged += OnPropertyChanged;
            _VM.Loaded += (s, e) =>
            {
                this.Cursor = e ? Cursors.Wait : null;
                this.Root.IsEnabled = !e;
                DispNowLoading(e);
            };

            this.DataContext = _VM;

            _WindowMode = new WindowMode(this);
            _WindowMode.NotifyWindowModeChanged += (s, e) => OnWindowModeChanged(e);

            _MouseDragController = new MouseDragController(this.MainView, this.MainContent, this.MainContentShadow);

            _MouseGesture = new MouseGestureEx(this.MainView);
            this.GestureTextBlock.SetBinding(TextBlock.TextProperty, new Binding("GestureText") { Source = _MouseGesture });
            _MouseGesture.Controller.MouseGestureUpdateEventHandler += OnMouseGestureUpdate;

            InitializeCommandBindings();
            InitializeInputGestures();

            _VM.BookCommands = BookCommands;

            // messenger
            Messenger.Initialize();
            Messenger.AddReciever("MessageBox", CallMessageBox);
            //Messenger.AddReciever("NowLoading", CallNowLoading);
        }

        private void OnMouseGestureUpdate(object sender, MouseGestureCollection e)
        {
            string text = _MouseGesture.GetGestureText();
            if (!string.IsNullOrEmpty(text))
            {
                _VM.InfoText = text;
            }
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "InfoText":
                    AutoFade(InfoTextBlock, 1.0, 0.5);
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
                _VM.BookSetting.BookReadOrder == BookReadOrder.LeftToRight ? ViewOrigin.LeftTop : ViewOrigin.RightTop;


            // ここはあまりよくない
            // _MouseDragController.Reset();
        }


        // new!
        public Dictionary<BookCommandType, RoutedCommand> BookCommands { get; set; } = new Dictionary<BookCommandType, RoutedCommand>();

        public static readonly RoutedCommand LoadCommand = new RoutedCommand("LoadCommand", typeof(MainWindow));





        class CommandSetting
        {
            public RoutedCommand Command { set; get; }
            public BookCommandType Type { set; get; }
        }

        class CommandSettingCollection : Dictionary<RoutedCommand, CommandSetting>
        {
            public void Add(RoutedCommand command, BookCommandType type)
            {
                Add(command, new CommandSetting() { Command = command, Type = type });
            }
        }

        //CommandSettingCollection _CommandSetting;

        public void InitializeCommandBindings()
        {
            // コマンドインスタンス作成
            foreach (BookCommandType type in Enum.GetValues(typeof(BookCommandType)))
            {
                BookCommands.Add(type, new RoutedCommand(type.ToString(), typeof(MainWindow)));
            }

            // スタティックコマンド
            this.CommandBindings.Add(new CommandBinding(LoadCommand, Load));

            // カスタムコマンドバインドを先に作成する
            this.CommandBindings.Add(new CommandBinding(BookCommands[BookCommandType.OpenSettingWindow],
                (t, e) => OpenSettingWindow()));
            this.CommandBindings.Add(new CommandBinding(BookCommands[BookCommandType.LoadAs],
                (t, e) => LoadAs()));
            this.CommandBindings.Add(new CommandBinding(BookCommands[BookCommandType.ClearHistory],
                (t, e) => _VM.ClearHistor()));
            this.CommandBindings.Add(new CommandBinding(BookCommands[BookCommandType.ToggleFullScreen],
                (t, e) => _WindowMode.Toggle()));
            this.CommandBindings.Add(new CommandBinding(BookCommands[BookCommandType.ViewScrollUp],
                (t, e) => _MouseDragController.ScrollUp()));
            this.CommandBindings.Add(new CommandBinding(BookCommands[BookCommandType.ViewScrollDown],
                (t, e) => _MouseDragController.ScrollDown()));
            this.CommandBindings.Add(new CommandBinding(BookCommands[BookCommandType.ViewScaleUp],
                (t, e) => _MouseDragController.ScaleUp()));
            this.CommandBindings.Add(new CommandBinding(BookCommands[BookCommandType.ViewScaleDown],
                (t, e) => _MouseDragController.ScaleDown()));

            // 標準コマンドバインド作成
            foreach (BookCommandType type in Enum.GetValues(typeof(BookCommandType)))
            {
                this.CommandBindings.Add(new CommandBinding(BookCommands[type], (t, e) => _VM.Execute(type)));
            }
        }


        // ダイアログでファイル選択して画像を読み込む
        private void LoadAs()
        {
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog(this) == true)
            {
                _VM.Execute(BookCommandType.LoadAs, dialog.FileName);
            }
        }

        private void Load(object sender, ExecutedRoutedEventArgs e)
        {
            var path = e.Parameter as string;
            _VM.Execute(BookCommandType.LoadAs, path);
        }

        // 設定ウィンドウを開く
        private void OpenSettingWindow()
        {
            var setting = _VM.CreateSettingContext();

            var dialog = new SettingWindow(_VM, setting);
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
            _MouseGesture.CommandBinding.Clear();

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
                    _MouseGesture.CommandBinding.Add(mouseGesture, e.Value);
                }
            }

            // Update Menu ...
            this.MenuArea.UpdateInputGestureText();

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
                this.MainView.Margin = new Thickness(0, this.StatusArea.ActualHeight, 0, this.StatusArea.ActualHeight);
            }
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
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
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
                _VM.Execute(BookCommandType.LoadAs, files[0]);
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


        private void DispNowLoading(bool isDisp)
        {
            if (isDisp)
            {
                this.NowLoading.Opacity = 0.0;

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



    [ValueConversion(typeof(BookSortMode), typeof(bool))]
    public class SortModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BookSortMode mode0 = (BookSortMode)value;
            BookSortMode mode1 = (BookSortMode)Enum.Parse(typeof(BookSortMode), parameter as string);
            return (mode0 == mode1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(BookReadOrder), typeof(bool))]
    public class BookReadOrderToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BookReadOrder mode0 = (BookReadOrder)value;
            BookReadOrder mode1 = (BookReadOrder)Enum.Parse(typeof(BookReadOrder), parameter as string);
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

}
