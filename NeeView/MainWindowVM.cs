// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


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

    // 通知表示の種類
    public enum ShowMessageStyle
    {
        None,
        Normal,
        Tiny,
    }


    /// <summary>
    /// ViewModel
    /// </summary>
    public class MainWindowVM : INotifyPropertyChanged, IDisposable
    {
        #region Events

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

        // ビューモデルの設定変更通知
        // マウスドラッグ情報のリセット等に使用されます。
        public event EventHandler ViewModeChanged;

        // ロード中通知
        public event EventHandler<string> Loading;

        // 表示変更を通知
        public event EventHandler ViewChanged;

        // ショートカット変更を通知
        public event EventHandler InputGestureChanged;

        #endregion


        // 移動制限モード
        public bool IsLimitMove { get; set; }

        // 回転、拡縮をコンテンツの中心基準にする
        public bool IsControlCenterImage { get; set; }

        // 回転スナップ
        public bool IsAngleSnap { get; set; }

        // 表示開始時の基準
        public bool IsViewStartPositionCenter { get; set; }

        // 通知表示スタイル
        public ShowMessageStyle CommandShowMessageStyle { get; set; }

        // ゼスチャ表示スタイル
        public ShowMessageStyle GestureShowMessageStyle { get; set; }

        // スライダー方向
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

        // スケールモード
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
                    UpdateContentSize();
                    ViewChanged?.Invoke(this, null);
                    ViewModeChanged?.Invoke(this, null);
                }
            }
        }
        #endregion

        // 背景スタイル
        #region Property: Background
        private BackgroundStyle _Background;
        public BackgroundStyle Background
        {
            get { return _Background; }
            set { _Background = value; UpdateBackgroundBrush(); OnPropertyChanged(); }
        }
        #endregion




        // コマンドバインド用
        // View側で定義されます
        public Dictionary<CommandType, RoutedUICommand> BookCommands { get; set; }

        // 空フォルダ通知表示のON/OFF
        #region Property: IsVisibleEmptyPageMessage
        private bool _IsVisibleEmptyPageMessage = false;
        public bool IsVisibleEmptyPageMessage
        {
            get { return _IsVisibleEmptyPageMessage; }
            set { if (_IsVisibleEmptyPageMessage != value) { _IsVisibleEmptyPageMessage = value; OnPropertyChanged(); } }
        }
        #endregion

        // 現在ページ番号
        public int Index
        {
            get { return BookHub.GetPageIndex(); }
            set { BookHub.SetPageIndex(value); }
        }

        // 最大ページ番号
        public int IndexMax
        {
            get { return BookHub.GetPageCount(); }
        }

        #region Window Icon

        // ウィンドウアイコン：標準
        private ImageSource _WindowIconDefault;

        // ウィンドウアイコン：スライドショー再生中
        private ImageSource _WindowIconPlay;

        // ウィンドウアイコン初期化
        private void InitializeWindowIcons()
        {
            _WindowIconDefault = null;
            _WindowIconPlay = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Play.ico", UriKind.RelativeOrAbsolute));
        }

        // 現在のウィンドウアイコン取得
        public ImageSource WindowIcon
        {
            get
            {
                return BookHub.IsEnableSlideShow ? _WindowIconPlay : _WindowIconDefault;
            }
        }

        #endregion

        #region Window Title

        // ウィンドウタイトル
        public string WindowTitle
        {
            get
            {
                if (LoadingPath != null)
                    return LoosePath.GetFileName(LoadingPath) + " (読込中)";

                if (BookHub.Current?.Place == null)
                    return _DefaultWindowTitle;

                string text = LoosePath.GetFileName(BookHub.Current.Place);

                if (_MainContent != null)
                {
                    text += $" ({_MainContent.Index + 1}/{IndexMax + 1})";

                    if (Contents[1].IsValid)
                    {
                        string name = Contents[1].FullPath?.TrimEnd('\\').Replace('/', '\\').Replace("\\", " > ");
                        text += $" - {name} | {LoosePath.GetFileName(Contents[0].FullPath)}";
                    }
                    else if (Contents[0].IsValid)
                    {
                        string name = Contents[0].FullPath?.TrimEnd('\\').Replace('/', '\\').Replace("\\", " > ");
                        text += $" - {name}";
                    }
                }

                return text;
            }
        }

        // ロード中パス
        private string _LoadingPath;
        public string LoadingPath
        {
            get { return _LoadingPath; }
            set { _LoadingPath = value; OnPropertyChanged("WindowTitle"); }
        }

        #endregion

        // 通知テキスト(標準)
        #region Property: InfoText
        private string _InfoText;
        public string InfoText
        {
            get { return _InfoText; }
            set { _InfoText = value; OnPropertyChanged(); }
        }

        // 通知テキストフォントサイズ
        public double InfoTextFontSize { get; set; } = 24.0;

        #endregion

        // 通知テキスト(控えめ)
        #region Property: TinyInfoText
        private string _TinyInfoText;
        public string TinyInfoText
        {
            get { return _TinyInfoText; }
            set { _TinyInfoText = value; OnPropertyChanged(); }
        }
        #endregion

        // 本設定 公開
        public Book.Memento BookSetting => BookHub.BookMemento;

        // 最近使ったフォルダ
        #region Property: LastFiles
        private List<Book.Memento> _LastFiles;
        public List<Book.Memento> LastFiles
        {
            get { return _LastFiles; }
            set { _LastFiles = value; OnPropertyChanged(); }
        }
        #endregion

        // コンテンツ
        public ObservableCollection<ViewContent> Contents { get; private set; }
        // 見開き時のメインとなるコンテンツ
        private ViewContent _MainContent;

        // Foregroudh Brush：ファイルページのフォントカラー用
        private Brush _ForegroundBrush = Brushes.White;
        public Brush ForegroundBrush
        {
            get { return _ForegroundBrush; }
            set { if (_ForegroundBrush != value) { _ForegroundBrush = value; OnPropertyChanged(); } }
        }

        // Backgroud Brush
        #region Property: BackgroundBrush
        private Brush _BackgroundBrush = Brushes.Black;
        public Brush BackgroundBrush
        {
            get { return _BackgroundBrush; }
            set { if (_BackgroundBrush != value) { _BackgroundBrush = value; OnPropertyChanged(); UpdateForegroundBrush(); } }
        }
        #endregion


        // 本管理
        public BookHub BookHub { get; private set; }


        // 標準ウィンドウタイトル
        private string _DefaultWindowTitle;

        // ユーザー設定ファイル名
        private string _UserSettingFileName;


        #region 開発用

        // 開発用：JobEndine公開
        public JobEngine JobEngine => ModelContext.JobEngine;

        // 開発用：ページリスト
        public List<Page> PageList => BookHub.Current?.Pages;

        #endregion


        // コンストラクタ
        public MainWindowVM()
        {
            InitializeWindowIcons();

            // ModelContext
            ModelContext.Initialize();
            ModelContext.JobEngine.StatusChanged +=
                (s, e) => OnPropertyChanged(nameof(JobEngine));

            // BookHub
            BookHub = new BookHub();

            BookHub.Loading +=
                (s, e) =>
                {
                    LoadingPath = e;
                    Loading?.Invoke(s, e);
                };

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


            // CommandTable
            ModelContext.CommandTable.SetTarget(this, BookHub);

            // Contents
            Contents = new ObservableCollection<ViewContent>();
            Contents.Add(new ViewContent());
            Contents.Add(new ViewContent());

            // Window title
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = FileVersionInfo.GetVersionInfo(assembly.Location);
            _DefaultWindowTitle = $"{assembly.GetName().Name} {ver.FileMajorPart}.{ver.FileMinorPart}";
#if DEBUG
            _DefaultWindowTitle += " [Debug]";
#endif

            // UserSetting filename
            _UserSettingFileName = System.IO.Path.GetDirectoryName(assembly.Location) + "\\UserSetting.xml";

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

            OnPropertyChanged(nameof(Index));
            OnPropertyChanged(nameof(IndexMax));

            UpdateLastFiles();

            UpdatePageList(); // 開発用(重い)
        }

        // 開発用：ページ更新
        [Conditional("DEBUG")]
        private void UpdatePageList()
        {
            //OnPropertyChanged(nameof(PageList));
        }

        // 最近使ったファイル 更新
        private void UpdateLastFiles()
        {
            LastFiles = ModelContext.BookHistory.ListUp(10);
        }

        // 履歴削除
        public void ClearHistor()
        {
            ModelContext.BookHistory.Clear();
            UpdateLastFiles();
        }

        // Foregroud Brush 更新
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

        // Background Brush 更新
        private void UpdateBackgroundBrush()
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
                    BackgroundBrush = Contents[Contents[1].IsValid ? 1 : 0].Color;
                    break;
                case BackgroundStyle.Check:
                    BackgroundBrush = (DrawingBrush)App.Current.Resources["CheckerBrush"];
                    break;
            }
        }

        #region アプリ設定

        // アプリ設定作成
        public Setting CreateSetting()
        {
            var setting = new Setting();

            setting.ViewMemento = this.CreateMemento();
            setting.SusieMemento = ModelContext.SusieContext.CreateMemento();
            setting.BookHubMemento = BookHub.CreateMemento();
            setting.CommandMememto = ModelContext.CommandTable.CreateMemento();
            setting.BookHistoryMemento = ModelContext.BookHistory.CreateMemento();

            return setting;
        }

        // アプリ設定反映
        public void RestoreSetting(Setting setting)
        {
            this.Restore(setting.ViewMemento);
            ModelContext.SusieContext.Restore(setting.SusieMemento);
            BookHub.Restore(setting.BookHubMemento);

            ModelContext.CommandTable.Restore(setting.CommandMememto);
            InputGestureChanged?.Invoke(this, null);

            ModelContext.BookHistory.Restore(setting.BookHistoryMemento);
            UpdateLastFiles();
        }


        // アプリ設定読み込み
        public void LoadSetting(Window window)
        {
            Setting setting;

            // 設定の読み込み
            if (System.IO.File.Exists(_UserSettingFileName))
            {
                try
                {
                    setting = Setting.Load(_UserSettingFileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Messenger.MessageBox(this, "設定の読み込みに失敗しました。初期設定で起動します。", _DefaultWindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                    setting = new Setting();
                }
            }
            else
            {
                setting = new Setting();
            }

            // 設定反映
            RestoreSetting(setting);

            // ウィンドウ座標復元
            WindowPlacement.Restore(window, setting.WindowPlacement);
        }


        // アプリ設定保存
        public void SaveSetting(Window window)
        {
            // 現在の本を履歴に登録
            ModelContext.BookHistory.Add(BookHub.Current);

            var setting = CreateSetting();

            // ウィンドウ座標保存
            setting.WindowPlacement = WindowPlacement.CreateMemento(window);

            // 設定をファイルに保存
            setting.Save(_UserSettingFileName);
        }

        #endregion


        // 表示コンテンツ更新
        private void OnViewContentsChanged(object sender, EventArgs e)
        {
            var book = BookHub.Current;

            if (book?.Place != null)
            {
                var contents = new List<ViewContent>();
                
                // ViewContent作成
                foreach (var source in book.ViewContentSources)
                {
                    if (source != null)
                    {
                        contents.Add(new ViewContent()
                        {
                            Content = source.CreateControl(new Binding("ForegroundBrush") { Source = this }),
                            Size = new Size(source.Width, source.Height),
                            Color = new SolidColorBrush(source.Color),
                            FullPath = source.FullPath,
                            Index = source.Index,
                        });
                    }
                }

                // ページが存在しない場合、専用メッセージを表示する
                IsVisibleEmptyPageMessage = contents.Count == 0;

                // メインとなるコンテンツを指定
                _MainContent = contents.Count > 0 ? contents[0] : null;

                // 左開きの場合は反転
                if (book.BookReadOrder == PageReadOrder.LeftToRight)
                {
                    contents.Reverse();
                }

                // ViewModelプロパティに反映
                for (int index = 0; index < 2; ++index)
                {
                    Contents[index] = index < contents.Count ? contents[index] : new ViewContent();
                }
            }
            else
            {
                // 空欄設定
                _MainContent = null;
                Contents[0] = new ViewContent();
                Contents[1] = new ViewContent();
            }

            // 背景色更新
            UpdateBackgroundBrush();

            // コンテンツサイズ更新
            UpdateContentSize();

            // 表示更新を通知
            ViewChanged?.Invoke(this, null);
            OnPropertyChanged(nameof(WindowTitle));
        }


        // ページ番号の更新
        private void OnPageChanged(object sender, int e)
        {
            OnPropertyChanged(nameof(Index));
            OnPropertyChanged(nameof(IndexMax));
        }


        // ビューエリアサイズ
        private double _ViewWidth;
        private double _ViewHeight;

        // ビューエリアサイズを更新
        public void SetViewSize(double width, double height)
        {
            _ViewWidth = width;
            _ViewHeight = height;

            UpdateContentSize();
        }

        // コンテンツ表示サイズを更新
        private void UpdateContentSize()
        {
            if (!Contents.Any(e => e.IsValid)) return;

            var scales = CalcContentScale(_ViewWidth, _ViewHeight);

            for (int i = 0; i < 2; ++i)
            {
                var size = Contents[i].Size;
                Contents[i].Width = Math.Floor(size.Width * scales[i]);
                Contents[i].Height = Math.Floor(size.Height * scales[i]);
            }
        }

        // ストレッチモードに合わせて各コンテンツのスケールを計算する
        private double[] CalcContentScale(double width, double height)
        {
            var c0 = Contents[0].Size;
            var c1 = Contents[1].Size;

            // オリジナルサイズ
            if (this.StretchMode == PageStretchMode.None)
            {
                return new double[] { 1.0, 1.0 };
            }

            double rate0 = 1.0;
            double rate1 = 1.0;

            // 2ページ合わせたコンテンツの表示サイズを求める
            Size content;
            if (!Contents[1].IsValid)
            {
                content = c0;
            }
            else
            {
                // どちらもImageでない
                if (c0.Width < 0.1 && c1.Width < 0.1)
                {
                    return new double[] { 1.0, 1.0 };
                }

                if (c0.Width == 0) c0 = c1;
                if (c1.Width == 0) c1 = c0;

                // 高さを 高い方に合わせる
                if (c0.Height > c1.Height)
                {
                    rate1 = c0.Height / c1.Height;
                }
                else
                {
                    rate0 = c1.Height / c0.Height;
                }

                // 高さをあわせたときの幅の合計
                content = new Size(c0.Width * rate0 + c1.Width * rate1, c0.Height * rate0);
            }

            // ビューエリアサイズに合わせる場合のスケール
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

            return new double[] { rate0, rate1 };
        }



        // コマンド実行 
        public void Execute(CommandType type, object param)
        {
            // 通知
            if (ModelContext.CommandTable[type].IsShowMessage)
            {
                string message = ModelContext.CommandTable[type].ExecuteMessage(param);

                switch (CommandShowMessageStyle)
                {
                    case ShowMessageStyle.Normal:
                        Messenger.Send(this, new MessageEventArgs("MessageShow")
                        {
                            Parameter = new MessageShowParams(message)
                        });
                        break;
                    case ShowMessageStyle.Tiny:
                        TinyInfoText = message;
                        break;
                }
            }

            // 実行
            ModelContext.CommandTable[type].Execute(param);
        }


        // ゼスチャ表示
        public void ShowGesture(string gesture, string commandName)
        {
            if (string.IsNullOrEmpty(gesture) && string.IsNullOrEmpty(commandName)) return;

            switch (GestureShowMessageStyle)
            {
                case ShowMessageStyle.Normal:
                    Messenger.Send(this, new MessageEventArgs("MessageShow")
                    {
                        Parameter = new MessageShowParams(((commandName != null) ? commandName + "\n" : "") + gesture)
                    });
                    break;
                case ShowMessageStyle.Tiny:
                    TinyInfoText = gesture + ((commandName != null) ? " " + commandName : "");
                    break;
            }
        }




        // スライドショーの表示間隔
        public double SlideShowInterval => BookHub.SlideShowInterval;

        // スライドショー：次のスライドへ
        public void NextSlide()
        {
            BookHub.NextSlide();
        }



        // フォルダ読み込み
        public void Load(string path)
        {
            BookHub.Load(path, BookLoadOption.None);
        }


        // 廃棄処理
        public void Dispose()
        {
            ModelContext.Terminate();
        }


        #region Memento

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
            public ShowMessageStyle CommandShowMessageStyle { get; set; }

            [DataMember]
            public ShowMessageStyle GestureShowMessageStyle { get; set; }

            void Constructor()
            {
                IsLimitMove = true;
                IsSliderDirectionReversed = true;
                CommandShowMessageStyle = ShowMessageStyle.Normal;
                GestureShowMessageStyle = ShowMessageStyle.Normal;
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

        //
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
            memento.CommandShowMessageStyle = this.CommandShowMessageStyle;
            memento.GestureShowMessageStyle = this.GestureShowMessageStyle;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            this.IsLimitMove = memento.IsLimitMove;
            this.IsControlCenterImage = memento.IsControlCenterImage;
            this.IsAngleSnap = memento.IsAngleSnap;
            this.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
            this.StretchMode = memento.StretchMode;
            this.Background = memento.Background;
            this.IsSliderDirectionReversed = memento.IsSliderDirectionReversed;
            this.CommandShowMessageStyle = memento.CommandShowMessageStyle;
            this.GestureShowMessageStyle = memento.GestureShowMessageStyle;

            ViewModeChanged?.Invoke(this, null);
        }

        #endregion

    }
}
