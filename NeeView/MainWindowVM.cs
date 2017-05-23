// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Effects;
using NeeView.Utility;
using NeeView.Windows.Controls;
using NeeView.Windows.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace NeeView
{
    // 通知表示の種類
    public enum ShowMessageStyle
    {
        None,
        Normal,
        Tiny,
    }


    // パネルカラー
    public enum PanelColor
    {
        Dark,
        Light,
    }




    /// <summary>
    /// MainWindow : ViewModel
    /// TODO : モデルの分離
    /// </summary>
    public class MainWindowVM : BindableBase, IDisposable
    {
        public static MainWindowVM Current { get; private set; }

        #region Events

        // ロード中通知
        public event EventHandler<string> Loading;

        // フォーカス初期化要求
        public event EventHandler ResetFocus;

        #endregion


        // 通知表示スタイル
        public ShowMessageStyle NoticeShowMessageStyle { get; set; }

        // ジェスチャー表示スタイル
        public ShowMessageStyle GestureShowMessageStyle { get; set; }

        // NowLoading表示スタイル
        public ShowMessageStyle NowLoadingShowMessageStyle { get; set; }


        // メニューを自動的に隠す
        #region Property: IsHideMenu
        private bool _isHideMenu;
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set
            {
                _isHideMenu = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHideMenu));
                UpdateSidePanelMargin();
            }
        }

        //
        public bool ToggleHideMenu()
        {
            IsHideMenu = !IsHideMenu;
            return IsHideMenu;
        }

        //
        public bool CanHideMenu => IsHideMenu || IsFullScreen;

        #endregion

        // スライダーを自動的に隠す
        #region Property: IsHidePageSlider
        private bool _isIsHidePageSlider;
        public bool IsHidePageSlider
        {
            get { return _isIsHidePageSlider; }
            set
            {
                _isIsHidePageSlider = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHidePageSlider));
                UpdateSidePanelMargin();
            }
        }

        //
        public bool ToggleHidePageSlider()
        {
            IsHidePageSlider = !IsHidePageSlider;
            return IsHidePageSlider;
        }

        //
        public bool CanHidePageSlider => IsHidePageSlider || IsFullScreen;

        #endregion

        // パネルを自動的に隠す
        #region Property: IsHidePanel
        private bool _isHidePanel; // = true;
        public bool IsHidePanel
        {
            get { return _isHidePanel; }
            set
            {
                _isHidePanel = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHidePanel));
            }
        }

        public bool ToggleHidePanel()
        {
            IsHidePanel = !IsHidePanel;
            return IsHidePanel;
        }

        /// <summary>
        /// フルスクリーン時にパネルを隠す
        /// </summary>
        public bool IsHidePanelInFullscreen
        {
            get { return _IsHidePanelInFullscreen; }
            set { if (_IsHidePanelInFullscreen != value) { _IsHidePanelInFullscreen = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(CanHidePanel)); } }
        }

        //
        private bool _IsHidePanelInFullscreen;



        // パネルを自動的に隠せるか
        public bool CanHidePanel => IsHidePanel || (IsHidePanelInFullscreen && IsFullScreen);

        #endregion


        /// <summary>
        /// IsVisibleWindowTitle property.
        /// タイトルバーが表示されておらず、スライダーにフォーカスがある場合等にキャンバスにタイトルを表示する
        /// </summary>
        private bool _IsVisibleWindowTitle;
        public bool IsVisibleWindowTitle
        {
            get { return _IsVisibleWindowTitle; }
            set { if (_IsVisibleWindowTitle != value) { _IsVisibleWindowTitle = value; RaisePropertyChanged(); } }
        }


        // アドレスバーON/OFF
        #region Property: IsVisibleAddressBar
        private bool _isVisibleAddressBar;
        public bool IsVisibleAddressBar
        {
            get { return _isVisibleAddressBar; }
            set { _isVisibleAddressBar = value; RaisePropertyChanged(); }
        }
        public bool ToggleVisibleAddressBar()
        {
            IsVisibleAddressBar = !IsVisibleAddressBar;
            return IsVisibleAddressBar;
        }
        #endregion


        #region SidePanels

        /// <summary>
        /// SidePanelMargin property.
        /// </summary>
        public Thickness SidePanelMargin
        {
            get { return _SidePanelMargin; }
            set { if (_SidePanelMargin != value) { _SidePanelMargin = value; RaisePropertyChanged(); } }
        }

        //
        private Thickness _SidePanelMargin;

        //
        private void UpdateSidePanelMargin()
        {
            SidePanelMargin = new Thickness(0, CanHideMenu ? 20 : 0, 0, CanHidePageSlider ? 20 : 0);
        }

        #region Canvas Size

        /// <summary>
        /// CanvasWidth property.
        /// キャンバスサイズ。SidePanelから引き渡される
        /// </summary>
        public double CanvasWidth
        {
            get { return _CanvasWidth; }
            set { if (_CanvasWidth != value) { _CanvasWidth = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasWidth;


        /// <summary>
        /// CanvasHeight property.
        /// </summary>
        public double CanvasHeight
        {
            get { return _CanvasHeight; }
            set { if (_CanvasHeight != value) { _CanvasHeight = value; RaisePropertyChanged(); } }
        }

        //
        private double _CanvasHeight;

        #endregion

        #endregion


        //
        private bool IsFullScreen => WindowShape.Current.IsFullScreen;

        //
        private void WindowShape_IsFullScreenPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanHidePanel));
            UpdateSidePanelMargin();
        }



        // フルスクリーン状態を復元するフラグ
        public bool IsSaveFullScreen { get; set; }


        // マルチブートを禁止する
        public bool IsDisableMultiBoot { get; set; }

        // スライドショーの自動開始
        public bool IsAutoPlaySlideShow { get; set; }

        // ウィンドウ座標を復元する
        public bool IsSaveWindowPlacement { get; set; }


        // 空フォルダー通知表示の詳細テキスト
        // TODO: ContentCanvasじゃね？
        #region Property: EmptyPageMessage
        private string _emptyPageMessage;
        public string EmptyPageMessage
        {
            get { return _emptyPageMessage; }
            set { _emptyPageMessage = value; RaisePropertyChanged(); }
        }
        #endregion


        // オートGC
        public bool IsAutoGC
        {
            get { return MemoryControl.Current.IsAutoGC; }
            set { MemoryControl.Current.IsAutoGC = value; }
        }



        #region Window Icon

        // ウィンドウアイコン：標準
        private ImageSource _windowIconDefault;

        // ウィンドウアイコン：スライドショー再生中
        private ImageSource _windowIconPlay;

        // ウィンドウアイコン初期化
        private void InitializeWindowIcons()
        {
            _windowIconDefault = null;
            _windowIconPlay = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/Play.ico", UriKind.RelativeOrAbsolute));
        }

        // 現在のウィンドウアイコン取得
        public ImageSource WindowIcon
            => SlideShow.Current.IsPlayingSlideShow ? _windowIconPlay : _windowIconDefault;

        #endregion


        // ウィンドタイトル
        public WindowTitle WindowTitle => _models.WindowTitle;




        #region Property: PanelColor
        private PanelColor _panelColor;
        public PanelColor PanelColor
        {
            get { return _panelColor; }
            set { if (_panelColor != value) { _panelColor = value; FlushPanelColor(); RaisePropertyChanged(); } }
        }
        public void FlushPanelColor()
        {
            if (App.Current == null) return;

            int alpha = _panelOpacity * 0xFF / 100;
            if (alpha > 0xff) alpha = 0xff;
            if (alpha < 0x00) alpha = 0x00;
            if (_panelColor == PanelColor.Dark)
            {
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0x11, 0x11, 0x11));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0x22, 0x22, 0x22));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x90, 0xEE, 0x90));
                App.Current.Resources["NVPanelIconBackground"] = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
                App.Current.Resources["NVPanelIconForeground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                App.Current.Resources["NVFolderPen"] = null;
            }
            else
            {
                App.Current.Resources["NVBackground"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0xF8, 0xF8, 0xF8));
                App.Current.Resources["NVForeground"] = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                App.Current.Resources["NVBaseBrush"] = new SolidColorBrush(Color.FromArgb((byte)alpha, 0xEE, 0xEE, 0xEE));
                App.Current.Resources["NVDefaultBrush"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD));
                App.Current.Resources["NVMouseOverBrush"] = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                App.Current.Resources["NVPressedBrush"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                App.Current.Resources["NVCheckMarkBrush"] = new SolidColorBrush(Color.FromRgb(0x44, 0xBB, 0x44));
                App.Current.Resources["NVPanelIconBackground"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                App.Current.Resources["NVPanelIconForeground"] = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
                App.Current.Resources["NVFolderPen"] = new Pen(new SolidColorBrush(Color.FromRgb(0xDE, 0xB9, 0x82)), 1);
            }
        }
        #endregion


        #region Property: PanelOpacity
        private int _panelOpacity = 100;
        public int PanelOpacity
        {
            get { return _panelOpacity; }
            set { _panelOpacity = value; FlushPanelColor(); RaisePropertyChanged(); }
        }
        #endregion


        #region ContextMenu

        private ContextMenuSetting _contextMenuSetting;
        public ContextMenuSetting ContextMenuSetting
        {
            get { return _contextMenuSetting; }
            set
            {
                _contextMenuSetting = value;
                _contextMenuSetting.Validate();
                UpdateContextMenu();
            }
        }

        //
        private ContextMenu _contextMenu;
        public ContextMenu ContextMenu
        {
            get { return _contextMenu; }
            set { _contextMenu = value; RaisePropertyChanged(); }
        }

        public void UpdateContextMenu()
        {
            ContextMenu = ContextMenuSetting.ContextMenu;
        }

        #endregion


        // オンラインヘルプ
        public void OpenOnlineHelp()
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }


        // 本管理
        public BookHub BookHub { get; private set; }

        //
        public BookOperation BookOperation { get; private set; }


        #region 開発用

        // 開発用：JobEndine公開
        public JobEngine JobEngine => ModelContext.JobEngine;

        // 開発用：コンテンツ座標
        #region Property: ContentPosition
        private Point _contentPosition;
        public Point ContentPosition
        {
            get { return _contentPosition; }
            set { _contentPosition = value; RaisePropertyChanged(); }
        }
        #endregion

        // 開発用：コンテンツ座標情報更新
        public void UpdateContentPosition()
        {
            ContentPosition = ContentCanvas.MainContent.View.PointToScreen(new Point(0, 0));
        }


        // 開発用：
        public Development Development { get; private set; } = new Development();

        /// <summary>
        /// DevUpdateContentPosition command.
        /// </summary>
        private RelayCommand _DevUpdateContentPosition;
        public RelayCommand DevUpdateContentPosition
        {
            get { return _DevUpdateContentPosition = _DevUpdateContentPosition ?? new RelayCommand(DevUpdateContentPosition_Executed); }
        }

        private void DevUpdateContentPosition_Executed()
        {
            UpdateContentPosition();
        }

        #endregion

        /// <summary>
        /// IsBusyJobEngine property.
        /// アクセス中マーク表示用
        /// </summary>
        private bool _isBusyJobEngine;
        public bool IsBusyJobEngine
        {
            get { return _isBusyJobEngine; }
            set { if (_isBusyJobEngine != value) { _isBusyJobEngine = value; RaisePropertyChanged(); } }
        }


        // ダウンロード画像の保存場所
        public string UserDownloadPath { get; set; }

        public string DownloadPath => string.IsNullOrWhiteSpace(UserDownloadPath) ? Temporary.TempDownloadDirectory : UserDownloadPath;


        // WindowShape
        public WindowShape WindowShape => WindowShape.Current;

        //
        public ContentCanvas ContentCanvas => _models.ContentCanvas;

        /// <summary>
        /// Model群。ひとまず。
        /// </summary>
        private Models _models;
        public Models Models => _models;


        /// <summary>
        /// Model property.
        /// </summary>
        public MainWindowModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private MainWindowModel _model;


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="window"></param>
        public MainWindowVM(MainWindow window)
        {
            Current = this;

            //
            _model = new MainWindowModel();

            // Window Shape
            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.IsFullScreen), WindowShape_IsFullScreenPropertyChanged);


            // Models
            ////_models = new Models();
            _models = _model.Models;


            // Side Panel
            _models.SidePanel.ResetFocus += (s, e) => ResetFocus?.Invoke(this, null);



            InitializeWindowIcons();

            SlideShow.Current.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(SlideShow.IsPlayingSlideShow):
                        RaisePropertyChanged(nameof(WindowIcon));
                        break;
                }
            };

            // ModelContext
            ModelContext.JobEngine.StatusChanged +=
                (s, e) => RaisePropertyChanged(nameof(JobEngine));

            ModelContext.JobEngine.IsBusyChanged +=
                (s, e) => IsBusyJobEngine = ModelContext.JobEngine.IsBusy && !SlideShow.Current.IsPlayingSlideShow;



            // BookHub
            BookHub = _models.BookHub;

            BookHub.Loading +=
                OnLoading;

            BookHub.BookChanged +=
                OnBookChanged;

            BookHub.SettingChanged +=
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(BookHub));
                };

            BookHub.InfoMessage +=
                (s, e) =>
                {
                    _models.InfoMessage.SetMessage(NoticeShowMessageStyle, e);
                };

            BookHub.EmptyMessage +=
                (s, e) => EmptyPageMessage = e;


            // BookOperation
            BookOperation = _models.BookOperation;

            BookOperation.InfoMessage +=
                (s, e) => _models.InfoMessage.SetMessage(NoticeShowMessageStyle, e);

            //
            _models.PagemarkList.InfoMessage +=
                (s, e) => _models.InfoMessage.SetMessage(NoticeShowMessageStyle, e);

            // ダウンロードフォルダー生成
            if (!System.IO.Directory.Exists(Temporary.TempDownloadDirectory))
            {
                System.IO.Directory.CreateDirectory(Temporary.TempDownloadDirectory);
            }
        }


        // Loading表示状態変更
        public void OnLoading(object sender, string e)
        {
            _models.WindowTitle.LoadingPath = e;
            Loading?.Invoke(sender, e);
        }


        // 本が変更された
        private void OnBookChanged(object sender, BookMementoType bookmarkType)
        {
            var title = LoosePath.GetFileName(BookHub.Address);

            App.Current?.Dispatcher.Invoke(() => _models.InfoMessage.SetMessage(NoticeShowMessageStyle, title, null, 2.0, bookmarkType));

            _models.MenuBar.UpdateLastFiles();

            //
            CommandManager.InvalidateRequerySuggested();
        }


        // 履歴削除
        public void ClearHistor()
        {
            ModelContext.BookHistory.Clear();
            _models.MenuBar.UpdateLastFiles();
        }


        // 最後に開いたフォルダーを開く
        public void LoadLastFolder()
        {
            if (!Preference.Current.bootup_lastfolder) return;

            string place = ModelContext.BookHistory.LastAddress;
            if (place != null || System.IO.Directory.Exists(place) || System.IO.File.Exists(place))
            {
                Load(place, BookLoadOption.Resume);
            }
        }


        // ジェスチャー表示
        public void ShowGesture(string gesture, string commandName)
        {
            if (string.IsNullOrEmpty(gesture) && string.IsNullOrEmpty(commandName)) return;

            _models.InfoMessage.SetMessage(
                GestureShowMessageStyle,
                ((commandName != null) ? commandName + "\n" : "") + gesture,
                gesture + ((commandName != null) ? " " + commandName : ""));
        }


        // フォルダー読み込み
        public void Load(string path, BookLoadOption option = BookLoadOption.None)
        {
            if (Utility.FileShortcut.IsShortcut(path) && (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)))
            {
                var shortcut = new Utility.FileShortcut(path);
                path = shortcut.TargetPath;
            }

            BookHub.RequestLoad(path, null, option, true);
        }

        // ドラッグ＆ドロップ取り込み失敗
        public void LoadError(string message)
        {
            EmptyPageMessage = message ?? "コンテンツの読み込みに失敗しました";
            BookHub?.RequestUnload(true);
        }


        #region クリップボード関連

        //
        private BitmapSource CurrentBitmapSource
        {
            get { return (ContentCanvas.MainContent?.Content as BitmapContent)?.BitmapSource; }
        }

        //
        public bool CanCopyImageToClipboard()
        {
            return CurrentBitmapSource != null;
        }


        // クリップボードに画像をコピー
        public void CopyImageToClipboard()
        {
            try
            {
                if (CanCopyImageToClipboard())
                {
                    ClipboardUtility.CopyImage(CurrentBitmapSource);
                }
            }
            catch (Exception e)
            {
                new MessageDialog($"原因: {e.Message}", "コピーに失敗しました").ShowDialog();
            }
        }

        #endregion


        #region 印刷

        /// <summary>
        /// 印刷可能判定
        /// </summary>
        /// <returns></returns>
        public bool CanPrint()
        {
            return ContentCanvas.MainContent != null && ContentCanvas.MainContent.IsValid;
        }

        /// <summary>
        /// 印刷
        /// </summary>
        public void Print(Window owner, FrameworkElement element, Transform transform, double width, double height)
        {
            if (!CanPrint()) return;

            // 掃除しておく
            GC.Collect();

            var contents = ContentCanvas.Contents;
            var mainContent = ContentCanvas.MainContent;

            // スケールモード退避
            var scaleModeMemory = contents.ToDictionary(e => e, e => e.BitmapScalingMode);

            // アニメーション停止
            foreach (var content in contents)
            {
                content.AnimationImageVisibility = Visibility.Visible;
                content.AnimationPlayerVisibility = Visibility.Collapsed;
            }

            // スライドショー停止
            SlideShow.Current.PauseSlideShow();

            try
            {
                var context = new PrintContext();
                context.MainContent = mainContent;
                context.Contents = contents;
                context.View = element;
                context.ViewTransform = transform;
                context.ViewWidth = width;
                context.ViewHeight = height;
                context.ViewEffect = _models.ImageEffect.Effect;
                context.Background = _models.ContentCanvasBrush.CreateBackgroundBrush();
                context.BackgroundFront = _models.ContentCanvasBrush.CreateBackgroundFrontBrush(new DpiScale(1, 1));

                var dialog = new PrintWindow(context);
                dialog.Owner = owner;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.ShowDialog();
            }
            finally
            {
                // スケールモード、アニメーション復元
                foreach (var content in contents)
                {
                    content.BitmapScalingMode = scaleModeMemory[content];
                    content.AnimationImageVisibility = Visibility.Collapsed;
                    content.AnimationPlayerVisibility = Visibility.Visible;
                }

                // スライドショー再開
                SlideShow.Current.ResumeSlideShow();
            }
        }

        #endregion

        // 廃棄処理
        public void Dispose()
        {
            BookHub.Dispose();

            ModelContext.Terminate();

            Debug.WriteLine("MainWindowVM: Disposed.");
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public bool IsLimitMove { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public bool IsControlCenterImage { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public bool IsAngleSnap { get; set; } // no used (ver.23)

            [DataMember(Order = 19, EmitDefaultValue = false)]
            public double AngleFrequency { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public bool IsViewStartPositionCenter { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public PageStretchMode StretchMode { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public BackgroundStyle Background { get; set; } // no used (ver.23)

            [DataMember(EmitDefaultValue = false)]
            public bool IsSliderDirectionReversed { get; set; } // no used

            [DataMember(Order = 4)]
            public ShowMessageStyle NoticeShowMessageStyle { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public ShowMessageStyle CommandShowMessageStyle { get; set; } // no used (ver.22)

            [DataMember]
            public ShowMessageStyle GestureShowMessageStyle { get; set; }

            [DataMember(Order = 4)]
            public ShowMessageStyle NowLoadingShowMessageStyle { get; set; }

            [DataMember(Order = 1, EmitDefaultValue = false)]
            public bool IsEnabledNearestNeighbor { get; set; } // no used (ver.22)

            [DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsKeepScale { get; set; } // no used(ver.23)

            [DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsKeepAngle { get; set; }  // no used(ver.23)

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsKeepFlip { get; set; } // no used(ver.23)

            [DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsLoadLastFolder { get; set; } // no used (ver.22)

            [DataMember(Order = 2)]
            public bool IsDisableMultiBoot { get; set; }

            [DataMember(Order = 4)]
            public bool IsAutoPlaySlideShow { get; set; }

            [DataMember(Order = 7)]
            public bool IsSaveWindowPlacement { get; set; }

            [DataMember(Order = 2)]
            public bool IsHideMenu { get; set; }

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsHideTitleBar { get; set; } // no used

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsVisibleTitleBar { get; set; } // no used (ver.22)

            [DataMember(Order = 4)]
            public bool IsSaveFullScreen { get; set; }

            [DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsTopmost { get; set; } // no used (ver.22)

            [DataMember(Order = 5, EmitDefaultValue = false)]
            public FileInfoSetting FileInfoSetting { get; set; } // no used

            [DataMember(Order = 5)]
            public string UserDownloadPath { get; set; }

            [DataMember(Order = 6, EmitDefaultValue = false)]
            public FolderListSetting FolderListSetting { get; set; } // no used

            [DataMember(Order = 6)]
            public PanelColor PanelColor { get; set; }

            [DataMember(Order = 7, EmitDefaultValue = false)]
            public string WindowTitleFormat1 { get; set; } // no used (ver.23)

            [DataMember(Order = 7, EmitDefaultValue = false)]
            public string WindowTitleFormat2 { get; set; } // no used (ver.23)

            [DataMember(Order = 8)]
            public bool IsVisibleAddressBar { get; set; }

            [DataMember(Order = 8)]
            public bool IsHidePanel { get; set; }

            [DataMember(Order = 8)]
            public bool IsHidePanelInFullscreen { get; set; }

            [DataMember(Order = 8)]
            public ContextMenuSetting ContextMenuSetting { get; set; }

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsEnableThumbnailList { get; set; } // no used (ver.23)

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsHideThumbnailList { get; set; } // no used (ver.23)

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public double ThumbnailSize { get; set; } // no used (ver.23)

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsSliderLinkedThumbnailList { get; set; } // no used (ver.23)

            [DataMember(Order = 8, EmitDefaultValue = false)]
            public bool IsVisibleThumbnailNumber { get; set; } // no used (ver.23)

            [DataMember(Order = 9)]
            public bool IsAutoGC { get; set; }

            [DataMember(Order = 9, EmitDefaultValue = false)]
            public bool IsVisibleThumbnailPlate { get; set; } // no used (ver.23)

            [DataMember(Order = 10, EmitDefaultValue = false)]
            public ShowMessageStyle ViewTransformShowMessageStyle { get; set; } // no used (ver.23)

            [DataMember(Order = 10, EmitDefaultValue = false)]
            public bool IsOriginalScaleShowMessage { get; set; } // no used (ver.23)

            [DataMember(Order = 12, EmitDefaultValue = false)]
            public double ContentsSpace { get; set; } // no used (ver.23)

            [DataMember(Order = 12, EmitDefaultValue = false)]
            public LongButtonDownMode LongLeftButtonDownMode { get; set; } // no used (ver.23)

            [DataMember(Order = 16, EmitDefaultValue = false)]
            public SliderDirection SliderDirection { get; set; } // no used (ver.23)

            [DataMember(Order = 17)]
            public bool IsHidePageSlider { get; set; }

            [DataMember(Order = 18, EmitDefaultValue = false)]
            public bool IsAutoRotate { get; set; } // no used (ver.23)

            [DataMember(Order = 19)]
            public bool IsVisibleWindowTitle { get; set; }

            [DataMember(Order = 19, EmitDefaultValue = false)]
            public bool IsVisibleLoupeInfo { get; set; } // no used (ver.23)

            [DataMember(Order = 20, EmitDefaultValue = false)]
            public bool IsSliderWithIndex { get; set; } // no used

            [DataMember(Order = 20, EmitDefaultValue = false)]
            public bool IsLoupeCenter { get; set; } // no used (ver.23)

            [DataMember(Order = 21, EmitDefaultValue = false)]
            public SliderIndexLayout SliderIndexLayout { get; set; } // no used (ver.23)

            [DataMember(Order = 21, EmitDefaultValue = false)]
            public BrushSource CustomBackground { get; set; } // no used (ver.23)

            //
            private void Constructor()
            {
                _Version = App.Config.ProductVersionNumber;
                NoticeShowMessageStyle = ShowMessageStyle.Normal;
                GestureShowMessageStyle = ShowMessageStyle.Normal;
                NowLoadingShowMessageStyle = ShowMessageStyle.Normal;
                PanelColor = PanelColor.Dark;
                IsSaveWindowPlacement = true;
                IsHidePanelInFullscreen = true;
                ContextMenuSetting = new ContextMenuSetting();
                IsAutoGC = true;
                IsDisableMultiBoot = true;
                IsVisibleWindowTitle = true;
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

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
                if (_Version < Config.GenerateProductVersionNumber(1, 16, 0))
                {
                    SliderDirection = IsSliderDirectionReversed ? SliderDirection.RightToLeft : SliderDirection.LeftToRight;
                }
                IsSliderDirectionReversed = false;

                if (_Version < Config.GenerateProductVersionNumber(1, 17, 0))
                {
                    IsHidePageSlider = IsHideMenu;
                    IsHideMenu = false;
                }

                if (_Version < Config.GenerateProductVersionNumber(1, 19, 0))
                {
                    AngleFrequency = IsAngleSnap ? 45 : 0;
                }
                IsAngleSnap = false;

                if (_Version < Config.GenerateProductVersionNumber(1, 21, 0))
                {
                    SliderIndexLayout = IsSliderWithIndex ? SliderIndexLayout.Right : SliderIndexLayout.None;
                }
                IsSliderWithIndex = false;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento._Version = App.Config.ProductVersionNumber;

            memento.NoticeShowMessageStyle = this.NoticeShowMessageStyle;
            memento.GestureShowMessageStyle = this.GestureShowMessageStyle;
            memento.NowLoadingShowMessageStyle = this.NowLoadingShowMessageStyle;
            memento.IsDisableMultiBoot = this.IsDisableMultiBoot;
            memento.IsAutoPlaySlideShow = this.IsAutoPlaySlideShow;
            memento.IsSaveWindowPlacement = this.IsSaveWindowPlacement;
            memento.IsHideMenu = this.IsHideMenu;
            memento.IsHidePageSlider = this.IsHidePageSlider;
            memento.IsSaveFullScreen = this.IsSaveFullScreen;
            memento.UserDownloadPath = this.UserDownloadPath;
            memento.PanelColor = this.PanelColor;
            memento.IsVisibleAddressBar = this.IsVisibleAddressBar;
            memento.IsHidePanel = this.IsHidePanel;
            memento.IsHidePanelInFullscreen = this.IsHidePanelInFullscreen;
            memento.ContextMenuSetting = this.ContextMenuSetting.Clone();
            memento.IsAutoGC = this.IsAutoGC;
            memento.IsVisibleWindowTitle = this.IsVisibleWindowTitle;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            this.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
            this.GestureShowMessageStyle = memento.GestureShowMessageStyle;
            this.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
            this.IsDisableMultiBoot = memento.IsDisableMultiBoot;
            this.IsAutoPlaySlideShow = memento.IsAutoPlaySlideShow;
            this.IsSaveWindowPlacement = memento.IsSaveWindowPlacement;
            this.IsHideMenu = memento.IsHideMenu;
            this.IsHidePageSlider = memento.IsHidePageSlider;
            this.IsSaveFullScreen = memento.IsSaveFullScreen;
            this.UserDownloadPath = memento.UserDownloadPath;
            this.PanelColor = memento.PanelColor;
            this.IsHidePanel = memento.IsHidePanel;
            this.IsVisibleAddressBar = memento.IsVisibleAddressBar;
            this.IsHidePanelInFullscreen = memento.IsHidePanelInFullscreen;
            this.ContextMenuSetting = memento.ContextMenuSetting.Clone();
            this.IsAutoGC = memento.IsAutoGC;
            this.IsVisibleWindowTitle = memento.IsVisibleWindowTitle;

            // compatible before ver.22
            if (memento._Version < Config.GenerateProductVersionNumber(1, 22, 0))
            {
                if (memento.FileInfoSetting != null)
                {
                    _models.FileInformation.IsUseExifDateTime = memento.FileInfoSetting.IsUseExifDateTime;
                    _models.FileInformation.IsVisibleBitsPerPixel = memento.FileInfoSetting.IsVisibleBitsPerPixel;
                    _models.FileInformation.IsVisibleLoader = memento.FileInfoSetting.IsVisibleLoader;
                }
                if (memento.FolderListSetting != null)
                {
                    _models.FolderList.IsVisibleBookmarkMark = memento.FolderListSetting.IsVisibleBookmarkMark;
                    _models.FolderList.IsVisibleHistoryMark = memento.FolderListSetting.IsVisibleHistoryMark;
                }

                _models.RoutedCommandTable.CommandShowMessageStyle = memento.CommandShowMessageStyle;
                WindowShape.Current.IsTopmost = memento.IsTopmost;
                WindowShape.Current.IsCaptionVisible = memento.IsVisibleTitleBar;
                Preference.Current.bootup_lastfolder = memento.IsLoadLastFolder;
            }

            // compatible before ver.23
            if (memento._Version < Config.GenerateProductVersionNumber(1, 23, 0))
            {
                _models.ContentCanvasTransform.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;
                _models.ContentCanvasTransform.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
                _models.ContentCanvasTransform.IsLimitMove = memento.IsLimitMove;
                _models.ContentCanvasTransform.AngleFrequency = memento.AngleFrequency;
                _models.ContentCanvasTransform.IsControlCenterImage = memento.IsControlCenterImage;
                _models.ContentCanvasTransform.IsKeepAngle = memento.IsKeepAngle;
                _models.ContentCanvasTransform.IsKeepFlip = memento.IsKeepFlip;
                _models.ContentCanvasTransform.IsKeepScale = memento.IsKeepScale;
                _models.ContentCanvasTransform.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;

                _models.ContentCanvas.StretchMode = memento.StretchMode;
                _models.ContentCanvas.IsEnabledNearestNeighbor = memento.IsEnabledNearestNeighbor;
                _models.ContentCanvas.ContentsSpace = memento.ContentsSpace;
                _models.ContentCanvas.IsAutoRotate = memento.IsAutoRotate;

                _models.ContentCanvasBrush.CustomBackground = memento.CustomBackground;
                _models.ContentCanvasBrush.Background = memento.Background;

                _models.WindowTitle.WindowTitleFormat1 = memento.WindowTitleFormat1;
                _models.WindowTitle.WindowTitleFormat2 = memento.WindowTitleFormat2;

                _models.PageSlider.SliderIndexLayout = memento.SliderIndexLayout;
                _models.PageSlider.SliderDirection = memento.SliderDirection;
                _models.PageSlider.IsSliderLinkedThumbnailList = memento.IsSliderLinkedThumbnailList;

                _models.ThumbnailList.IsEnableThumbnailList = memento.IsEnableThumbnailList;
                _models.ThumbnailList.IsHideThumbnailList = memento.IsHideThumbnailList;
                _models.ThumbnailList.ThumbnailSize = memento.ThumbnailSize;
                _models.ThumbnailList.IsVisibleThumbnailNumber = memento.IsVisibleThumbnailNumber;
                _models.ThumbnailList.IsVisibleThumbnailPlate = memento.IsVisibleThumbnailPlate;

                _models.MouseInput.LongLeftButtonDownMode = memento.LongLeftButtonDownMode;
                _models.MouseInput.IsLoupeCenter = memento.IsLoupeCenter;
                _models.MouseInput.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
            }
        }

        #endregion
    }
}
