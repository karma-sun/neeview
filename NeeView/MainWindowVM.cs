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

        public void Store(MainWindowVM vm)
        {
            IsLimitMove = vm.IsLimitMove;
            IsControlCenterImage = vm.IsControlCenterImage;
            IsAngleSnap = vm.IsAngleSnap;
            IsViewStartPositionCenter = vm.IsViewStartPositionCenter;
        }

        public void Restore(MainWindowVM vm)
        {
            vm.IsLimitMove = IsLimitMove;
            vm.IsControlCenterImage = IsControlCenterImage;
            vm.IsAngleSnap = IsAngleSnap;
            vm.IsViewStartPositionCenter = IsViewStartPositionCenter;

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

        public JobEngine JobEngine { get { return Book.JobEngine; } }
        public int JobCount { get { return Book.JobEngine.Context.JobList.Count; } }

        public int MaxPage { get { return _Book.Pages.Count - 1; } }

        public ObservableCollection<DispPage> PageList { get; private set; } = new ObservableCollection<DispPage>();

        public string CurrentPage
        {
            get
            {
                if (_Book?.Place == null || _Book?.CurrentPage == null) return _DefaultWindowTitle;
                string name = _Book.CurrentPage.FullPath?.TrimEnd('\\').Replace('/', '\\').Replace("\\", " > ");
                string place = LoosePath.GetFileName(_Book.Place);
                return $"{place} ({Index}/{MaxPage}) - {name}";
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


        public int PageMode { get { return _Book.PageMode; } }
        public PageStretchMode StretchMode { get { return _Book.StretchMode; } }
        public BookSortMode SortMode { get { return _Book.SortMode; } }
        public bool IsReverseSort { get { return _Book.IsReverseSort; } }
        public bool IsViewStartPositionCenter { get; set; }
        public bool IsSupportedTitlePage => _Book.IsSupportedTitlePage;
        public bool IsSupportedWidePage => _Book.IsSupportedWidePage;
        public BookReadOrder BookReadOrder => _Book.BookReadOrder;
        public bool IsRecursiveFolder => _Book.IsRecursiveFolder;

        public event EventHandler<bool> Loaded
        {
            add { _Book.Loaded += value; }
            remove { _Book.Loaded -= value; }
        }

        public int Index
        {
            get { return _Book.Index; }
            set { _Book.Index = value; }
        }

        #region Property: LastFiles
        private List<BookParamSetting> _LastFiles;
        public List<BookParamSetting> LastFiles
        {
            get { return _LastFiles; }
            set { _LastFiles = value; OnPropertyChanged(); }
        }
        #endregion




        public ObservableCollection<FrameworkElement> Contents { get; private set; }
        public ObservableCollection<double> ContentsWidth { get; private set; }
        public ObservableCollection<double> ContentsHeight { get; private set; }

        public Brush _PageColor = Brushes.Black;


        public event EventHandler PageChanged;
        public event EventHandler InputGestureChanged;

        private Book _Book;
        //private Dictionary<string, BookParamSetting> _BookParamSettings;
        private BookCommandCollection _Commands;

        private Setting _Setting;

        public MainWindowVM()
        {
            ModelContext.Initialize();

            _Book = new Book();

            //ModelContext.BookHistory = new BookHistory();

            _Commands = new BookCommandCollection();
            _Commands.Initialize(_Book, null);

            Book.JobEngine.Context.AddEvent += JobEngineEvent;
            Book.JobEngine.Context.RemoveEvent += JobEngineEvent;

            _Book.BookChanged +=
                OnBookChanged;

            _Book.PageChanged +=
                OnPageChanged;
            _Book.ModeChanged +=
                (s, e) =>
                {
                    UpdateContentsWidth();
                    OnPropertyChanged(nameof(PageMode));
                    OnPropertyChanged(nameof(StretchMode));
                    OnPropertyChanged(nameof(SortMode));
                    OnPropertyChanged(nameof(IsReverseSort));
                    PageChanged?.Invoke(this, null);
                    ViewModeChanged?.Invoke(this, null);
                };
            _Book.NowPagesChanged += OnContentsCollectionChanged;
            _Book.BackgroundChanged += OnBackgroundChanged;
            _Book.PropertyChanged +=
                (s, e) =>
                {
                    OnPropertyChanged(nameof(IsSupportedTitlePage));
                    OnPropertyChanged(nameof(IsSupportedWidePage));
                    OnPropertyChanged(nameof(BookReadOrder));
                    OnPropertyChanged(nameof(IsRecursiveFolder));
                    if (e == "BookReadOrder")
                    {
                        ViewModeChanged?.Invoke(this, null);
                    }
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

        private void OnBookChanged(object sender, EventArgs e)
        {
#if false // 結局ページリストは却下
            PageList.Clear();
            foreach (var page in _Book.Pages)
            {
                PageList.Add(new DispPage()
                {
                    ID = PageList.Count + 1,
                    Name = LoosePath.GetFileName(page.Path)
                });
            }
#endif
            InfoText = LoosePath.GetFileName(_Book.Place);
            OnPropertyChanged(nameof(MaxPage));

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


        private void OnBackgroundChanged(object sender, EventArgs e)
        {
            switch (_Book.Background)
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
            setting.BookSetting.Store(_Book);
            setting.GestureSetting.Store(_Commands);

            return setting;
        }

        public void SetSettingContext(Setting setting)
        {
            setting.ViewSetting.Restore(this);
            setting.BookSetting.Restore(_Book);
            setting.GestureSetting.Restore(_Commands);

            InputGestureChanged?.Invoke(this, null);
        }

        public void LoadSetting(Window window)
        {
            // 設定の読み込み
            if (System.IO.File.Exists(_SettingFileName))
            {
                _Setting = Setting.Load(_SettingFileName);
                _Setting.WindowPlacement?.Restore(window);

                _Setting.ViewSetting.Restore(this);
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
            _Setting.BookSetting.Store(_Book);
            _Setting.GestureSetting.Store(_Commands);

            ModelContext.BookHistory.Add(_Book);
            _Setting.BookHistory = ModelContext.BookHistory;

            _Setting.Save(_SettingFileName);
        }

        private void OnContentsCollectionChanged(object sender, EventArgs e)
        {
            for (int index = 0; index < 2; ++index)
            {
                //int id = (BookReadOrder == BookReadOrder.RightToLeft) ? index : 1 - index;
                ViewContent content = _Book.NowPages[index];

                if (string.IsNullOrEmpty(_Book.Place))
                {
                    Contents[index] = null;
                }
                else if (index == 0 && content == null)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = $"{_Book.Place}\n読み込めるファイルがありません";
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
#if true
                    var context = new FilePageContext() { Icon = FilePageIcon.File, Message = (string)content.Content };
                    var control = new FilePageControl(context);
                    //control.Width = content.Width;
                    //control.Height = content.Height;
                    Contents[index] = control;
#else
                        var textBlock = new TextBlock();
                    textBlock.Text = (string)content.Content;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.Background = Brushes.Orange;
                    textBlock.Padding = new Thickness(16);
                    Contents[index] = textBlock;
#endif
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

            PageChanged?.Invoke(this, null);
        }

        private void OnPageChanged(object sender, int e)
        {
            OnPropertyChanged(nameof(Index));
            OnPropertyChanged(nameof(CurrentPage));
            OnPropertyChanged(nameof(PageMode));
            //PageChanged?.Invoke(this, null);
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
            for (int i = 0; i < 2; ++i)
            {
                //ContentsWidth[i] = CalcContentWidth(i, _ViewWidth, _ViewHeight);
                var scale = CalcContentScale(i, _ViewWidth, _ViewHeight);
                Point size = GetContentSize(_Book.NowPages[i]);
                ContentsWidth[i] = size.X * scale;
                ContentsHeight[i] = size.Y * scale;
            }
        }

        //
        private double CalcContentScale(int contentId, double width, double height)
        {
            Point c0 = GetContentSize(_Book.NowPages[0]);
            Point c1 = GetContentSize(_Book.NowPages[1]);

            if (_Book.StretchMode == PageStretchMode.None)
            {
                return 1.0; // (contentId == 0) ? c0.X : c1.X;
            }


            double rate0 = 1.0;
            double rate1 = 1.0;

            Point content;


            //if (_Book.PageMode == 1)
            if (_Book.NowPages[1] == null)
            {
                content = c0;
            }
            else
            {
                // どちらもImageでない
                if (c0.X == 0 && c1.X == 0) return 1.0; //  width * 0.5;

                if (c0.X == 0) c0 = c1;
                if (c1.X == 0) c1 = c0;

                // c1 の高さを c0 に合わせる
                rate1 = c0.Y / c1.Y;

                // 高さをあわせたときの幅の合計
                content = new Point(c0.X * rate0 + c1.X * rate1, c0.Y);
            }

            //
            double rateW = width / content.X;
            double rateH = height / content.Y;


            // 拡大はしない
            if (_Book.StretchMode == PageStretchMode.Inside)
            {
                if (rateW > 1.0) rateW = 1.0;
                if (rateH > 1.0) rateH = 1.0;
            }
            // 縮小はしない
            else if (_Book.StretchMode == PageStretchMode.Outside)
            {
                if (rateW < 1.0) rateW = 1.0;
                if (rateH < 1.0) rateH = 1.0;
            }

            // 枠いっぱいに広げる
            if (_Book.StretchMode == PageStretchMode.UniformToFill)
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

            // 計算された幅を返す
            if (contentId == 0)
            {
                return rate0; // c0.X * rate0;
            }
            else
            {
                return rate1; // c1.X * rate1;
            }
        }


        //
        private double CalcContentWidth(int contentId, double width, double height)
        {
            Point c0 = GetContentSize(_Book.NowPages[0]);
            Point c1 = GetContentSize(_Book.NowPages[1]);

            if (_Book.StretchMode == PageStretchMode.None)
            {
                return (contentId == 0) ? c0.X : c1.X;
            }


            double rate0 = 1.0;
            double rate1 = 1.0;

            Point content;


            //if (_Book.PageMode == 1)
            if (_Book.NowPages[1] == null)
            {
                content = c0;
            }
            else
            {
                // どちらもImageでない
                if (c0.X == 0 && c1.X == 0) return width * 0.5;

                if (c0.X == 0) c0 = c1;
                if (c1.X == 0) c1 = c0;

                // c1 の高さを c0 に合わせる
                rate1 = c0.Y / c1.Y;

                // 高さをあわせたときの幅の合計
                content = new Point(c0.X * rate0 + c1.X * rate1, c0.Y);
            }

            //
            double rateW = width / content.X;
            double rateH = height / content.Y;


            // 拡大はしない
            if (_Book.StretchMode == PageStretchMode.Inside)
            {
                if (rateW > 1.0) rateW = 1.0;
                if (rateH > 1.0) rateH = 1.0;
            }
            // 縮小はしない
            else if (_Book.StretchMode == PageStretchMode.Outside)
            {
                if (rateW < 1.0) rateW = 1.0;
                if (rateH < 1.0) rateH = 1.0;
            }

            // 枠いっぱいに広げる
            if (_Book.StretchMode == PageStretchMode.UniformToFill)
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

            // 計算された幅を返す
            if (contentId == 0)
            {
                return c0.X * rate0;
            }
            else
            {
                return c1.X * rate1;
            }
        }

        //
        private Point GetContentSize(ViewContent content)
        {
            var size = new Point();

            if (content?.Content == null)
            {
                size.X = 0;
                size.Y = 0;
            }
            else
            {
                size.X = content.Width;
                size.Y = content.Height;
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
