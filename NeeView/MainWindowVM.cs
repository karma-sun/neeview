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
    [DataContract]
    public class ViewSetting
    {
        [DataMember]
        public bool IsLimitMove { get; set; } = true;

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

        void Constructor()
        {
            IsLimitMove = true;
        }

        public ViewSetting()
        {
            Constructor();
        }

        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        public void Store(MainWindowVM vm)
        {
            IsLimitMove = vm.IsLimitMove;
            IsControlCenterImage = vm.IsControlCenterImage;
            IsAngleSnap = vm.IsAngleSnap;
            IsViewStartPositionCenter = vm.IsViewStartPositionCenter;
            StretchMode = vm.StretchMode;
            Background = vm.Background;
        }

        public void Restore(MainWindowVM vm)
        {
            vm.IsLimitMove = IsLimitMove;
            vm.IsControlCenterImage = IsControlCenterImage;
            vm.IsAngleSnap = IsAngleSnap;
            vm.IsViewStartPositionCenter = IsViewStartPositionCenter;
            vm.StretchMode = StretchMode;
            vm.Background = Background;

            vm.OnViewModeChanged();
        }
    }



    public class DispPage
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }



        public class MainWindowVM : INotifyPropertyChanged
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


        public Dictionary<BookCommandType, RoutedCommand> BookCommands { get; set; }

        public JobEngine JobEngine { get { return ModelContext.JobEngine; } }
        public int JobCount { get { return ModelContext.JobEngine.Context.JobList.Count; } }

        public int Index
        {
            get { return _Book.GetPageIndex(); }
            set { _Book.SetPageIndex(value); }
        }

        public int IndexMax
        {
            get { return _Book.GetPageCount(); }
        }

        public ObservableCollection<DispPage> PageList { get; private set; } = new ObservableCollection<DispPage>();

        public string CurrentPage
        {
            get
            {
                if (BookProxy.Current?.Place == null || BookProxy.Current?.CurrentPage == null) return _DefaultWindowTitle;
                string name = BookProxy.Current.CurrentPage.FullPath?.TrimEnd('\\').Replace('/', '\\').Replace("\\", " > ");
                string place = LoosePath.GetFileName(BookProxy.Current.Place);
                return $"{place} ({Index+1}/{IndexMax}) - {name}";
            }
        }

        #region Property: InfoText
        private string _InfoText;
        public string InfoText
        {
            get { return _InfoText; }
            set { _InfoText = value; OnPropertyChanged(); }
        }
        #endregion

        public BookSetting BookSetting => _Book.BookSetting;

        //public int PageMode { get { return _Book.BookSetting.PageMode; } }
        //public BookSortMode SortMode { get { return _Book.BookSetting.SortMode; } }
        //public bool IsReverseSort { get { return _Book.BookSetting.IsReverseSort; } }
        public bool IsViewStartPositionCenter { get; set; }
        //public bool IsSupportedTitlePage => _Book.BookSetting.IsSupportedTitlePage;
        //public bool IsSupportedWidePage => _Book.BookSetting.IsSupportedWidePage;
        //public BookReadOrder BookReadOrder => _Book.BookSetting.BookReadOrder;
        //public bool IsRecursiveFolder => _Book.BookSetting.IsRecursiveFolder;

        public event EventHandler<bool> Loaded
        {
            add { _Book.Loaded += value; }
            remove { _Book.Loaded -= value; }
        }


        #region Property: LastFiles
        private List<BookSetting> _LastFiles;
        public List<BookSetting> LastFiles
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

        private BookProxy _Book;
        //private Dictionary<string, BookParamSetting> _BookParamSettings;
        private BookCommandCollection _Commands;

        private Setting _Setting;

        public MainWindowVM()
        {
            ModelContext.Initialize();

            _Book = new BookProxy();

            //ModelContext.BookHistory = new BookHistory();

            _Commands = new BookCommandCollection();
            _Commands.Initialize(this, _Book, null);

            ModelContext.JobEngine.Context.AddEvent += JobEngineEvent;
            ModelContext.JobEngine.Context.RemoveEvent += JobEngineEvent;

            _Book.BookChanged +=
                OnBookChanged;

            _Book.PageChanged +=
                OnPageChanged;

            /*
            _Book.ModeChanged +=
                (s, e) =>
                {
                    UpdateContentsWidth();
                    OnPropertyChanged(nameof(BookSetting));
                    //OnPropertyChanged(nameof(PageMode));
                    OnPropertyChanged(nameof(StretchMode));
                    //OnPropertyChanged(nameof(SortMode));
                    //OnPropertyChanged(nameof(IsReverseSort));
                    PageChanged?.Invoke(this, null);
                    ViewModeChanged?.Invoke(this, null);
                };
                */

            _Book.ViewContentsChanged += OnViewContentsChanged;
            //_Book.BackgroundChanged += OnBackgroundChanged;
            _Book.SettingChanged +=
                (s, e) =>
                {
                    OnPropertyChanged(nameof(BookSetting));
                    //OnPropertyChanged(nameof(IsSupportedTitlePage));
                    //OnPropertyChanged(nameof(IsSupportedWidePage));
                    //OnPropertyChanged(nameof(BookReadOrder));
                    //OnPropertyChanged(nameof(IsRecursiveFolder));
                    /*
                    if (e == "BookReadOrder")
                    {
                        ViewModeChanged?.Invoke(this, null);
                    }
                    */
                };
            _Book.InfoMessage +=
                (s, e) => InfoText = e;

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

        }

        // 本が変更された
        private void OnBookChanged(object sender, Book book)
        {
            InfoText = LoosePath.GetFileName(book.Place);
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

        #region Property: BackgroundBrush
        private Brush _BackgroundBrush;
        public Brush BackgroundBrush
        {
            get { return _BackgroundBrush; }
            set { if (_BackgroundBrush != value) { _BackgroundBrush = value; OnPropertyChanged(); } }
        }
        #endregion

        #region Property: Background
        private BackgroundStyle _Background;
        public BackgroundStyle Background
        {
            get { return _Background; }
            set { _Background = value; OnPropertyChanged(); }
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
            setting.ViewSetting.Store(this);
            setting.SusieSetting.Store(ModelContext.SusieContext);
            setting.BookCommonSetting = BookCommonSetting.Store(_Book);
            setting.BookSetting = BookSetting.Store(_Book);
            setting.GestureSetting.Store(_Commands);

            return setting;
        }

        public void SetSettingContext(Setting setting)
        {
            setting.ViewSetting.Restore(this);
            setting.SusieSetting.Restore(ModelContext.SusieContext);
            setting.BookCommonSetting.Restore(_Book);
            setting.BookSetting.Restore(_Book);
            setting.GestureSetting.Restore(_Commands);

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
                    Debug.WriteLine("設定復元に失敗しました");
                    _Setting = new Setting();
                }

                _Setting.WindowPlacement?.Restore(window);


                _Setting.ViewSetting.Restore(this);
                _Setting.SusieSetting.Restore(ModelContext.SusieContext);
                _Setting.BookCommonSetting.Restore(_Book);
                _Setting.BookSetting.Restore(_Book);
                _Setting.GestureSetting.Restore(_Commands);

                ModelContext.BookHistory = _Setting.BookHistory;
                UpdateLastFiles();

                InputGestureChanged?.Invoke(this, null);
            }
        }

        public void SaveSetting(Window window)
        {
            _Setting = new Setting();
            _Setting.WindowPlacement.Store(window);

            _Setting.ViewSetting.Store(this);
            _Setting.SusieSetting.Store(ModelContext.SusieContext);
            _Setting.BookCommonSetting = BookCommonSetting.Store(_Book);
            _Setting.BookSetting = BookSetting.Store(_Book);
            _Setting.GestureSetting.Store(_Commands);

            ModelContext.BookHistory.Add(BookProxy.Current);
            _Setting.BookHistory = ModelContext.BookHistory;

            _Setting.Save(_SettingFileName);
        }

        private void OnViewContentsChanged(object sender, EventArgs e)
        {
            var book = BookProxy.Current;

            for (int index = 0; index < 2; ++index)
            {
                int cid = (book.BookReadOrder == BookReadOrder.RightToLeft) ? index : 1 - index;

                ViewContent content = book.NowPages[cid];

                if (string.IsNullOrEmpty(book.Place))
                {
                    Contents[index] = null;
                }
                else if (index == 0 && content == null)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = $"{book.Place}\n読み込めるファイルがありません";
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.Background = Brushes.Orange;
                    textBlock.Padding = new Thickness(16);
                    Contents[index] = textBlock;
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
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                    Contents[index] = image;
                }
                else if (content.Content is Uri)
                {
#if true
                    var media = new MediaElement();
                    media.Source = (Uri)content.Content;
                    media.MediaEnded += (s, e_) => media.Position = TimeSpan.FromMilliseconds(1);
                    //RenderOptions.SetBitmapScalingMode(media, BitmapScalingMode.HighQuality);
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

                if (content != null)
                {
                    _PageColor = (content.Color != null) ? new SolidColorBrush(content.Color) : Brushes.Black;
                }
            }

            OnBackgroundChanged(sender, null);

            UpdateContentsWidth();

            ViewChanged?.Invoke(this, null);
        }

        // ページ番号の更新
        private void OnPageChanged(object sender, int e)
        {
            OnPropertyChanged(nameof(Index));
            OnPropertyChanged(nameof(CurrentPage));
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
            if (BookProxy.Current == null) return;

            var scales = CalcContentScale(_ViewWidth, _ViewHeight);

            for (int i = 0; i < 2; ++i)
            {

                int cid = (BookProxy.Current.BookReadOrder == BookReadOrder.RightToLeft) ? i : 1 - i;

                //ContentsWidth[i] = CalcContentWidth(i, _ViewWidth, _ViewHeight);
                //var scale = CalcContentScale(i, _ViewWidth, _ViewHeight);
                var size = GetContentSize(BookProxy.Current.NowPages[cid]);
                ContentsWidth[i] = size.Width * scales[cid];
                ContentsHeight[i] = size.Height * scales[cid];
            }
        }

        //
        private double[] CalcContentScale(double width, double height)
        {
            var c0 = GetContentSize(BookProxy.Current.NowPages[0]);
            var c1 = GetContentSize(BookProxy.Current.NowPages[1]);

            if (this.StretchMode == PageStretchMode.None)
            {
                return new double[] { 1.0, 1.0 }; // ; // (contentId == 0) ? c0.X : c1.X;
            }


            double rate0 = 1.0;
            double rate1 = 1.0;

            Size content;

            //if (_Book.PageMode == 1)
            if (BookProxy.Current.NowPages[1] == null)
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
        public void Execute(BookCommandType type)
        {
            _Commands[type].Execute(null);
        }

        // 
        public void Execute(BookCommandType type, object param)
        {
            _Commands[type].Execute(param);
        }

        public List<InputGesture> GetShortCutCollection(BookCommandType type)
        {
            var list = new List<InputGesture>();
            if (_Commands[type].ShortCutKey != null)
            {
                foreach (var key in _Commands[type].ShortCutKey.Split(','))
                {
                    InputGestureConverter converter = new InputGestureConverter();
                    InputGesture inputGesture = converter.ConvertFromString(key);
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

        public string GetMouseGesture(BookCommandType type)
        {
            return _Commands[type].MouseGesture;
        }


        private void JobEngineEvent(object sender, Job e)
        {
            OnPropertyChanged(nameof(JobCount));
        }

        /*
        public void SetViewSize(double width, double height)
        {
            _Book.SetViewSize(width, height);
        }
        */


    }
}
