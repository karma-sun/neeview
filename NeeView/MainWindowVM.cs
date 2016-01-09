using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Runtime.Serialization;
//using System.Drawing;

namespace NeeView
{
    // 背景の種類
    public enum BackgroundStyle
    {
        Black,
        White,
        Auto,
        Check
    };

    public enum ShowMessageType
    {
        None,
        Normal,
        Tiny,
    }



    public class DispPage
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }



    public class MainWindowVM : INotifyPropertyChanged, IDisposable
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public event EventHandler ViewModeChanged;

        public void OnViewModeChanged()
        {
            ViewModeChanged?.Invoke(this, null);
        }

        public bool IsLimitMove { get; set; }

        public bool IsControlCenterImage { get; set; }

        public bool IsAngleSnap { get; set; }

        public ShowMessageType CommandShowMessageType { get; set; }
        public ShowMessageType GestureShowMessageType { get; set; }

        #region Property: IsSliderDirectionReversed
        private bool _IsSliderDirectionReversed;
        public bool IsSliderDirectionReversed
        {
            get { return _IsSliderDirectionReversed; }
            set
            {
                if (_IsSliderDirectionReversed != value)
                {
                    _IsSliderDirectionReversed = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion


        #region Property: IsVisibleEmptyPageMessage
        private bool _IsVisibleEmptyPageMessage = false;
        public bool IsVisibleEmptyPageMessage
        {
            get { return _IsVisibleEmptyPageMessage; }
            set { if (_IsVisibleEmptyPageMessage != value) { _IsVisibleEmptyPageMessage = value; OnPropertyChanged(); } }
        }
        #endregion

        public Dictionary<CommandType, RoutedUICommand> BookCommands { get; set; }

        public JobEngine JobEngine { get { return ModelContext.JobEngine; } }
        public int JobCount { get { return ModelContext.JobEngine.Context.JobList.Count; } }

        public int Index
        {
            get { return BookHub.GetPageIndex(); }
            set { BookHub.SetPageIndex(value); }
        }

        public int IndexMax
        {
            get { return BookHub.GetPageCount(); }
        }

        public ObservableCollection<DispPage> PageList { get; private set; } = new ObservableCollection<DispPage>();


        private ImageSource _WindowIconDefault;
        private ImageSource _WindowIconPlay;

        private void InitializeWindowIcons()
        {
            _WindowIconDefault = null; // BitmapFrame.Create(new Uri("pack://application:,,,/App.ico", UriKind.RelativeOrAbsolute));
            _WindowIconPlay = BitmapFrame.Create(new Uri("pack://application:,,,/Play.ico", UriKind.RelativeOrAbsolute));
        }

        public ImageSource WindowIcon
        {
            get
            {
                return BookHub.IsEnableSlideShow ? _WindowIconPlay : _WindowIconDefault;
            }
        }


        public string WindowTitle
        {
            get
            {
                if (LoadingPath != null)
                    return LoadingPath + " - Loading";

                if (BookHub.Current?.Place == null)
                    return _DefaultWindowTitle;

                string text = LoosePath.GetFileName(BookHub.Current.Place);

                if (BookHub.Current.CurrentPage != null)
                {
                    string name = BookHub.Current.CurrentPage.FullPath?.TrimEnd('\\').Replace('/', '\\').Replace("\\", " > ");
                    text += $" ({Index + 1}/{IndexMax + 1}) - {name}";
                }

                if (BookHub.Current.CurrentViewPageCount >= 2 && BookHub.Current.CurrentNextPage != null)
                {
                    string name = LoosePath.GetFileName(BookHub.Current.CurrentNextPage.Path);
                    text += $" | {name}";
                }

                return text;
            }
        }

        private string _LoadingPath;
        public string LoadingPath
        {
            get { return _LoadingPath; }
            set { _LoadingPath = value; OnPropertyChanged("WindowTitle"); }
        }

        #region Property: InfoText
        private string _InfoText;
        public string InfoText
        {
            get { return _InfoText; }
            set { _InfoText = value; OnPropertyChanged(); }
        }
        #endregion


        #region Property: TinyInfoText
        private string _TinyInfoText;
        public string TinyInfoText
        {
            get { return _TinyInfoText; }
            set { _TinyInfoText = value; OnPropertyChanged(); }
        }
        #endregion

        public double InfoTextFontSize { get; set; } = 24.0;


        public Book.Memento BookSetting => BookHub.BookMemento;


        public bool IsViewStartPositionCenter { get; set; }


        public event EventHandler<string> Loading
        {
            add { BookHub.Loading += value; }
            remove { BookHub.Loading -= value; }
        }


        #region Property: LastFiles
        private List<Book.Memento> _LastFiles;
        public List<Book.Memento> LastFiles
        {
            get { return _LastFiles; }
            set { _LastFiles = value; OnPropertyChanged(); }
        }
        #endregion

        #region Property: StretchMode
        private PageStretchMode _StretchMode = PageStretchMode.Uniform;
        public PageStretchMode StretchMode
        {
            get { return _StretchMode; }
            set
            {
                if (_StretchMode != value)
                {
                    _StretchMode = value;
                    OnPropertyChanged();
                    UpdateContentsWidth();
                    ViewChanged?.Invoke(this, null);
                    ViewModeChanged?.Invoke(this, null);
                }
            }
        }
        #endregion


        public ObservableCollection<FrameworkElement> Contents { get; private set; }
        public ObservableCollection<double> ContentsWidth { get; private set; }
        public ObservableCollection<double> ContentsHeight { get; private set; }

        public Brush _PageColor = Brushes.Black;


        public event EventHandler ViewChanged;
        public event EventHandler InputGestureChanged;

        public BookHub BookHub { get; private set; }

        public CommandTable CommandCollection => ModelContext.CommandTable; // TODO:定義位置とか

        //public FolderOrder FolderOrder => BookHub.FolderOrder;

        private Setting _Setting;




        public MainWindowVM()
        {
            InitializeWindowIcons();

            ModelContext.Initialize();

            BookHub = new BookHub();

            //ModelContext.BookHistory = new BookHistory();

            //CommandCollection = new CommandTable(this, BookHub);
            //CommandCollection.Initialize(this, BookHub, null);
            CommandCollection.SetTarget(this, BookHub);


            ModelContext.JobEngine.Context.AddEvent += JobEngineEvent;
            ModelContext.JobEngine.Context.RemoveEvent += JobEngineEvent;

            BookHub.Loading +=
                (s, e) => LoadingPath = e;

            BookHub.BookChanged +=
                OnBookChanged;

            BookHub.PageChanged +=
                OnPageChanged;

            BookHub.ViewContentsChanged +=
                OnViewContentsChanged;

            BookHub.SettingChanged +=
                (s, e) =>
                {
                    OnPropertyChanged(nameof(BookSetting));
                    OnPropertyChanged(nameof(BookHub));
                };
            BookHub.InfoMessage +=
                (s, e) => Messenger.Send(this, new MessageEventArgs("MessageShow") { Parameter = new MessageShowParams(e) });

            BookHub.SlideShowModeChanged +=
                (s, e) => OnPropertyChanged(nameof(WindowIcon));

            Contents = new ObservableCollection<FrameworkElement>();
            Contents.Add(null);
            Contents.Add(null);
            ContentsWidth = new ObservableCollection<double>();
            ContentsWidth.Add(0);
            ContentsWidth.Add(0);
            ContentsHeight = new ObservableCollection<double>();
            ContentsHeight.Add(0);
            ContentsHeight.Add(0);

            // title
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = FileVersionInfo.GetVersionInfo(assembly.Location);
            _DefaultWindowTitle = $"{assembly.GetName().Name} {ver.FileMajorPart}.{ver.ProductMinorPart}";
#if DEBUG
            _DefaultWindowTitle += " [Debug]";
#endif

            // setting filename
            _SettingFileName = System.IO.Path.GetDirectoryName(assembly.Location) + "\\UserSetting.xml";

            // messenger
            Messenger.AddReciever("UpdateLastFiles", (s, e) => UpdateLastFiles());
        }

        // 本が変更された
        private void OnBookChanged(object sender, bool isBookmark)
        {
            Messenger.Send(this, new MessageEventArgs("MessageShow")
            {
                Parameter = new MessageShowParams(LoosePath.GetFileName(BookHub.Current.Place))
                {
                    IsBookmark = isBookmark,
                    DispTime = 2.0
                }
            });

            OnPropertyChanged(nameof(IndexMax));

            UpdateLastFiles();
        }

        private void UpdateLastFiles()
        {
            // 最近使ったファイル
            LastFiles = ModelContext.BookHistory.ListUp(10);
        }

        public void ClearHistor()
        {
            ModelContext.BookHistory.Clear();
            UpdateLastFiles();
        }

        private Brush _ForegroundBrush = Brushes.White;
        public Brush ForegroundBrush
        {
            get { return _ForegroundBrush; }
            set { if (_ForegroundBrush != value) { _ForegroundBrush = value; OnPropertyChanged(); } }
        }

        private void UpdateForegroundBrush()
        {
            var solidColorBrush = BackgroundBrush as SolidColorBrush;
            if (solidColorBrush != null)
            {
                double y =
                    (double)solidColorBrush.Color.R * 0.299 +
                    (double)solidColorBrush.Color.G * 0.587 +
                    (double)solidColorBrush.Color.B * 0.114;

                ForegroundBrush = (y < 0.25) ? Brushes.White : Brushes.Black;
            }
            else
            {
                ForegroundBrush = Brushes.Black;
            }
        }

        #region Property: BackgroundBrush
        private Brush _BackgroundBrush = Brushes.Black;
        public Brush BackgroundBrush
        {
            get { return _BackgroundBrush; }
            set { if (_BackgroundBrush != value) { _BackgroundBrush = value; OnPropertyChanged(); UpdateForegroundBrush(); } }
        }
        #endregion

        #region Property: Background
        private BackgroundStyle _Background;
        public BackgroundStyle Background
        {
            get { return _Background; }
            set { _Background = value; OnBackgroundChanged(this, null); OnPropertyChanged(); }
        }
        #endregion

        private void OnBackgroundChanged(object sender, EventArgs e)
        {
            switch (this.Background)
            {
                default:
                case BackgroundStyle.Black:
                    BackgroundBrush = Brushes.Black;
                    break;
                case BackgroundStyle.White:
                    BackgroundBrush = Brushes.White;
                    break;
                case BackgroundStyle.Auto:
                    BackgroundBrush = _PageColor;
                    break;
                case BackgroundStyle.Check:
                    BackgroundBrush = (DrawingBrush)App.Current.Resources["CheckerBrush"];
                    break;
            }
        }


        private string _DefaultWindowTitle;
        private string _SettingFileName;

        public Setting CreateSettingContext()
        {
            var setting = new Setting();
            setting.ViewMemento = this.CreateMemento();
            setting.SusieMemento = ModelContext.SusieContext.CreateMemento();
            setting.BookHubMemento = BookHub.CreateMemento();
            setting.CommandMememto = CommandCollection.CreateMemento();

            setting.BookHistoryMemento = ModelContext.BookHistory.CreateMemento();

            return setting;
        }

        public void SetSettingContext(Setting setting)
        {
            this.Restore(setting.ViewMemento);
            ModelContext.SusieContext.Restore(setting.SusieMemento);
            BookHub.Restore(setting.BookHubMemento);
            CommandCollection.Restore(setting.CommandMememto);
            //setting.GestureSetting.Restore(CommandCollection);

            ModelContext.BookHistory.Restore(setting.BookHistoryMemento);
            UpdateLastFiles();

            InputGestureChanged?.Invoke(this, null);
        }

        public void LoadSetting(Window window)
        {
            // 設定の読み込み
            if (System.IO.File.Exists(_SettingFileName))
            {
                try
                {
                    _Setting = Setting.Load(_SettingFileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Messenger.MessageBox(this, "設定の読み込みに失敗しました。初期設定で起動します。", _DefaultWindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    _Setting = new Setting();
                }
            }
            else
            {
                _Setting = new Setting();
            }

            this.Restore(_Setting.ViewMemento);
            ModelContext.SusieContext.Restore(_Setting.SusieMemento);
            BookHub.Restore(_Setting.BookHubMemento);
            CommandCollection.Restore(_Setting.CommandMememto);

            ModelContext.BookHistory.Restore(_Setting.BookHistoryMemento);
            UpdateLastFiles();

            InputGestureChanged?.Invoke(this, null);

            _Setting.WindowPlacement?.Restore(window);
        }

        public void SaveSetting(Window window)
        {
            _Setting = new Setting();
            _Setting.WindowPlacement.Store(window);

            _Setting.ViewMemento = this.CreateMemento();
            _Setting.SusieMemento = ModelContext.SusieContext.CreateMemento();
            _Setting.BookHubMemento = BookHub.CreateMemento();
            _Setting.CommandMememto = CommandCollection.CreateMemento();

            ModelContext.BookHistory.Add(BookHub.Current);
            _Setting.BookHistoryMemento = ModelContext.BookHistory.CreateMemento();

            _Setting.Save(_SettingFileName);
        }


        // 表示コンテンツ更新
        private void OnViewContentsChanged(object sender, EventArgs e)
        {
            var book = BookHub.Current;

            Brush pageColor = Brushes.Black;

            if (book != null)
            {


                //
                IsVisibleEmptyPageMessage = book.ViewContents.All(content => content == null);

                //
                for (int index = 0; index < 2; ++index)
                {
                    int cid = (book.BookReadOrder == PageReadOrder.RightToLeft) ? index : 1 - index;

                    ViewContent content = book.ViewContents[cid];

                    if (string.IsNullOrEmpty(book.Place))
                    {
                        Contents[index] = null;
                    }
                    else if (content?.Content == null)
                    {
                        Contents[index] = null;
                    }
                    else if (content.Content is BitmapSource)
                    {
                        var image = new Image();
                        image.Source = (BitmapSource)content.Content;
                        image.Stretch = Stretch.Fill;
                        //image.UseLayoutRounding = true;
                        //image.SnapsToDevicePixels = true;
                        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                        Contents[index] = image;
                    }
                    else if (content.Content is Uri)
                    {
#if true
                        var media = new MediaElement();
                        media.Source = (Uri)content.Content;
                        media.MediaEnded += (s, e_) => media.Position = TimeSpan.FromMilliseconds(1);
                        Contents[index] = media;
#else
                // 高速切替でゾンビプロセスが残るのでNG
                var image = new Image();
                XamlAnimatedGif.AnimationBehavior.SetSourceUri(image, (Uri)page.Content);
                //XamlAnimatedGif.AnimationBehavior.AddLoadedHandler(image, AnimationBehavior_OnLoaded);
                //XamlAnimatedGif.AnimationBehavior.AddErrorHandler(image, AnimationBehavior_OnError);
                Contents[index] = image;
#endif
                    }
                    else if (content.Content is FilePageContext)
                    {
                        var control = new FilePageControl(content.Content as FilePageContext);
                        control.DefaultBrush = Brushes.Red;
                        control.SetBinding(FilePageControl.DefaultBrushProperty, new System.Windows.Data.Binding("ForegroundBrush") { Source = this });
                        Contents[index] = control;
                    }
                    else if (content.Content is string)
                    {
                        var context = new FilePageContext() { Icon = FilePageIcon.File, Message = (string)content.Content };
                        var control = new FilePageControl(context);
                        Contents[index] = control;
                    }
                    else
                    {
                        Contents[index] = null;
                    }

                    //
                    if (content?.Color != null)
                    {
                        pageColor = new SolidColorBrush(content.Color);
                    }
                }
            }
            else
            {
                Contents[0] = null;
                Contents[1] = null;
            }

            _PageColor = pageColor;
            OnBackgroundChanged(sender, null);

            UpdateContentsWidth();

            ViewChanged?.Invoke(this, null);
        }

        // ページ番号の更新
        private void OnPageChanged(object sender, int e)
        {
            OnPropertyChanged(nameof(Index));
            OnPropertyChanged(nameof(WindowTitle));
        }



        private double _ViewWidth;
        private double _ViewHeight;

        public void SetViewSize(double width, double height)
        {
            _ViewWidth = width;
            _ViewHeight = height;

            UpdateContentsWidth();
        }

        private void UpdateContentsWidth()
        {
            if (BookHub.Current == null) return;

            var scales = CalcContentScale(_ViewWidth, _ViewHeight);

            for (int i = 0; i < 2; ++i)
            {
                int cid = (BookHub.Current.BookReadOrder == PageReadOrder.RightToLeft) ? i : 1 - i;

                //ContentsWidth[i] = CalcContentWidth(i, _ViewWidth, _ViewHeight);
                //var scale = CalcContentScale(i, _ViewWidth, _ViewHeight);
                var size = GetContentSize(BookHub.Current.ViewContents[cid]);
                ContentsWidth[i] = size.Width * scales[cid];
                ContentsHeight[i] = size.Height * scales[cid];
            }
        }

        //
        private double[] CalcContentScale(double width, double height)
        {
            var c0 = GetContentSize(BookHub.Current.ViewContents[0]);
            var c1 = GetContentSize(BookHub.Current.ViewContents[1]);

            if (this.StretchMode == PageStretchMode.None)
            {
                return new double[] { 1.0, 1.0 }; // ; // (contentId == 0) ? c0.X : c1.X;
            }


            double rate0 = 1.0;
            double rate1 = 1.0;

            Size content;

            //if (_Book.PageMode == 1)
            if (BookHub.Current.ViewContents[1] == null)
            {
                content = c0;
            }
            else
            {
                // どちらもImageでない
                if (c0.Width == 0 && c1.Width == 0)
                {
                    return new double[] { 1.0, 1.0 }; // 1.0; //  width * 0.5;
                }

                if (c0.Width == 0) c0 = c1;
                if (c1.Width == 0) c1 = c0;

                // c1 の高さを c0 に合わせる
                rate1 = c0.Height / c1.Height;

                // 高さをあわせたときの幅の合計
                content = new Size(c0.Width * rate0 + c1.Width * rate1, c0.Height);
            }

            //
            double rateW = width / content.Width;
            double rateH = height / content.Height;


            // 拡大はしない
            if (this.StretchMode == PageStretchMode.Inside)
            {
                if (rateW > 1.0) rateW = 1.0;
                if (rateH > 1.0) rateH = 1.0;
            }
            // 縮小はしない
            else if (this.StretchMode == PageStretchMode.Outside)
            {
                if (rateW < 1.0) rateW = 1.0;
                if (rateH < 1.0) rateH = 1.0;
            }

            // 枠いっぱいに広げる
            if (this.StretchMode == PageStretchMode.UniformToFill)
            {
                if (rateW > rateH)
                {
                    rate0 *= rateW;
                    rate1 *= rateW;
                }
                else
                {
                    rate0 *= rateH;
                    rate1 *= rateH;
                }
            }
            // 枠に収めるように広げる
            else
            {
                if (rateW < rateH)
                {
                    rate0 *= rateW;
                    rate1 *= rateW;
                }
                else
                {
                    rate0 *= rateH;
                    rate1 *= rateH;
                }
            }

            //
            return new double[] { rate0, rate1 };

            /*
            // 計算された幅を返す

            if (contentId == 0)
            {
                return rate0; // c0.X * rate0;
            }
            else
            {
                return rate1; // c1.X * rate1;
            }
            */
        }

        //
        private Size GetContentSize(ViewContent content)
        {
            var size = new Size();

            if (content?.Content == null)
            {
                size.Width = 0;
                size.Height = 0;
            }
            else
            {
                size.Width = content.Width;
                size.Height = content.Height;
            }
            return size;
        }



        // 
        /*
        public void Execute(BookCommandType type)
        {
            _Commands[type].Execute(null);
        }
        */

        // 
        public void Execute(CommandType type, object param)
        {
            // 通知
            //if (CommandCollection.ShortcutSource[type].IsShowMessage)
            if (CommandCollection[type].IsShowMessage)
            {
                string message = CommandCollection[type].ExecuteMessage(param);

                switch (CommandShowMessageType)
                {
                    case ShowMessageType.Normal:
                        //InfoText = BookCommandExtension.Headers[type].Text;
                        Messenger.Send(this, new MessageEventArgs("MessageShow")
                        {
                            Parameter = new MessageShowParams(message)
                        });
                        break;
                    case ShowMessageType.Tiny:
                        TinyInfoText = message;
                        break;
                }
            }

            // 実行
            CommandCollection[type].Execute(param);
        }

        //
        public void ShowGesture(string gesture, string commandName)
        {
            if (string.IsNullOrEmpty(gesture) && string.IsNullOrEmpty(commandName)) return;

            switch (GestureShowMessageType)
            {
                case ShowMessageType.Normal:
                    //InfoText = ((commandName != null) ? commandName + "\n" : "") + gesture;
                    Messenger.Send(this, new MessageEventArgs("MessageShow")
                    {
                        Parameter = new MessageShowParams(((commandName != null) ? commandName + "\n" : "") + gesture)
                    });
                    break;
                case ShowMessageType.Tiny:
                    TinyInfoText = gesture + ((commandName != null) ? " " + commandName : "");
                    break;
            }
        }


        public List<InputGesture> GetShortCutCollection(CommandType type)
        {
            var list = new List<InputGesture>();
            if (CommandCollection[type].ShortCutKey != null)
            {
                foreach (var key in CommandCollection[type].ShortCutKey.Split(','))
                {
                    InputGesture inputGesture = InputGestureConverter.ConvertFromString(key);
                    if (inputGesture != null)
                    {
                        list.Add(inputGesture);
                    }
                    else
                    {
                        Debug.WriteLine("no support gesture: " + key);
                    }

#if false
                    try
                    {
                        KeyGestureConverter converter = new KeyGestureConverter();
                        KeyGesture keyGesture = (KeyGesture)converter.ConvertFromString(key);
                        list.Add(keyGesture);
                        continue;
                    }
                    catch { }

                    try
                    {
                        MouseGestureConverter converter = new MouseGestureConverter();
                        MouseGesture mouseGesture = (MouseGesture)converter.ConvertFromString(key);
                        list.Add(mouseGesture);
                        continue;
                    }
                    catch { }

                    Debug.WriteLine("no support gesture: " + key);
#endif
                }
            }

            return list;
        }

        public string GetMouseGesture(CommandType type)
        {
            return CommandCollection[type].MouseGesture;
        }


        private void JobEngineEvent(object sender, Job e)
        {
            OnPropertyChanged(nameof(JobCount));
        }

        //
        public double SlideShowInterval => BookHub.SlideShowInterval;

        public void NextSlide()
        {
            BookHub.NextSlide();
        }


        //
        public void Load(string path)
        {
            //BookHub.IsEnableSlideShow = false; // スライドショウ停止
            BookHub.Load(path, BookLoadOption.None);
        }


        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsLimitMove { get; set; }

            [DataMember]
            public bool IsControlCenterImage { get; set; }

            [DataMember]
            public bool IsAngleSnap { get; set; }

            [DataMember]
            public bool IsViewStartPositionCenter { get; set; }

            [DataMember]
            public PageStretchMode StretchMode { get; set; }

            [DataMember]
            public BackgroundStyle Background { get; set; }

            [DataMember]
            public bool IsSliderDirectionReversed { get; set; }

            [DataMember]
            public ShowMessageType CommandShowMessageType { get; set; }

            [DataMember]
            public ShowMessageType GestureShowMessageType { get; set; }

            void Constructor()
            {
                IsLimitMove = true;
                IsSliderDirectionReversed = true;
                CommandShowMessageType = ShowMessageType.Normal;
                GestureShowMessageType = ShowMessageType.Normal;
                StretchMode = PageStretchMode.Uniform;
                Background = BackgroundStyle.Black;
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }
        }


        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsLimitMove = this.IsLimitMove;
            memento.IsControlCenterImage = this.IsControlCenterImage;
            memento.IsAngleSnap = this.IsAngleSnap;
            memento.IsViewStartPositionCenter = this.IsViewStartPositionCenter;
            memento.StretchMode = this.StretchMode;
            memento.Background = this.Background;
            memento.IsSliderDirectionReversed = this.IsSliderDirectionReversed;
            memento.CommandShowMessageType = this.CommandShowMessageType;
            memento.GestureShowMessageType = this.GestureShowMessageType;

            return memento;
        }

        public void Restore(Memento memento)
        {
            this.IsLimitMove = memento.IsLimitMove;
            this.IsControlCenterImage = memento.IsControlCenterImage;
            this.IsAngleSnap = memento.IsAngleSnap;
            this.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
            this.StretchMode = memento.StretchMode;
            this.Background = memento.Background;
            this.IsSliderDirectionReversed = memento.IsSliderDirectionReversed;
            this.CommandShowMessageType = memento.CommandShowMessageType;
            this.GestureShowMessageType = memento.GestureShowMessageType;

            this.OnViewModeChanged();
        }

        public void Dispose()
        {
            ModelContext.Terminate();
        }
    }
}
