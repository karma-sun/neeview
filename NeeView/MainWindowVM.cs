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

    // パネル
    public enum PanelSide
    {
        Left,
        Right,
    }

    // ウィンドウタイトル更新項目
    [Flags]
    public enum UpdateWindowTitleMask
    {
        None = 0,
        Book = (1 << 0),
        Page = (1 << 1),
        View = (1 << 2),
        All = 0xFFFF
    }

    // 長押しモード
    public enum LongButtonDownMode
    {
        None,
        Loupe
    }

    // 自動回転タイプ
    public enum AutoRotateType
    {
        Right,
        Left,
    }

    //
    public static class LongButtonDownModeExtensions
    {
        public static string ToTips(this LongButtonDownMode element)
        {
            switch (element)
            {
                default:
                    return null;
                case LongButtonDownMode.Loupe:
                    return "一時的に画像を拡大表示します\nルーペ表示中にホイール操作で拡大率を変更できます";
            }
        }
    }

    // スライダーの方向
    public enum SliderDirection
    {
        LeftToRight, // 左から右
        RightToLeft, // 右から左
        SyncBookReadDirection, // 本を開く方向にあわせる
    }

    // スライダー数値表示の配置
    public enum SliderIndexLayout
    {
        None, // 表示なし
        Left, // 左
        Right, // 右
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

        // ショートカット変更を通知
        public event EventHandler InputGestureChanged;

        // 本を閉じた
        public event EventHandler BookUnloaded;

        // フォーカス初期化要求
        public event EventHandler ResetFocus;

        #endregion


        // 通知表示スタイル
        public ShowMessageStyle NoticeShowMessageStyle { get; set; }

        // ジェスチャー表示スタイル
        public ShowMessageStyle GestureShowMessageStyle { get; set; }

        // NowLoading表示スタイル
        public ShowMessageStyle NowLoadingShowMessageStyle { get; set; }


        /// <summary>
        /// IsVisibleLoupeInfo property.
        /// </summary>
        private bool _IsVisibleLoupeInfo;
        public bool IsVisibleLoupeInfo
        {
            get { return _IsVisibleLoupeInfo; }
            set { if (_IsVisibleLoupeInfo != value) { _IsVisibleLoupeInfo = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsLoupeCenter property.
        /// </summary>
        private bool _IsLoupeCenter;
        public bool IsLoupeCenter
        {
            get { return _IsLoupeCenter; }
            set { if (_IsLoupeCenter != value) { _IsLoupeCenter = value; RaisePropertyChanged(); } }
        }


        // スライダー方向
        #region Property: IsSliderDirectionReversed
        private bool _isSliderDirectionReversed;
        public bool IsSliderDirectionReversed
        {
            get { return _isSliderDirectionReversed; }
            private set
            {
                if (_isSliderDirectionReversed != value)
                {
                    _isSliderDirectionReversed = value;
                    RaisePropertyChanged();
                }
            }
        }

        //
        private void UpdateIsSliderDirectionReversed()
        {
            switch (SliderDirection)
            {
                default:
                case SliderDirection.LeftToRight:
                    IsSliderDirectionReversed = false;
                    break;
                case SliderDirection.RightToLeft:
                    IsSliderDirectionReversed = true;
                    break;
                case SliderDirection.SyncBookReadDirection:
                    IsSliderDirectionReversed = this.BookHub.BookMemento.BookReadOrder == PageReadOrder.RightToLeft;
                    break;
            }
        }

        //
        private SliderDirection _sliderDirection;
        public SliderDirection SliderDirection
        {
            get { return _sliderDirection; }
            set
            {
                _sliderDirection = value;
                UpdateIsSliderDirectionReversed();
            }
        }

        #endregion


        /// <summary>
        /// SliderIndexType property.
        /// </summary>
        private SliderIndexLayout _SliderIndexLayout;
        public SliderIndexLayout SliderIndexLayout
        {
            get { return _SliderIndexLayout; }
            set
            {
                if (_SliderIndexLayout != value)
                {
                    _SliderIndexLayout = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsSliderWithIndex));
                    RaisePropertyChanged(nameof(SliderIndexDock));
                }
            }
        }

        /// <summary>
        /// IsSliderWithIndex property.
        /// </summary>
        public bool IsSliderWithIndex => _SliderIndexLayout != SliderIndexLayout.None;

        /// <summary>
        /// SliderIndexDock property.
        /// </summary>
        public Dock SliderIndexDock => _SliderIndexLayout == SliderIndexLayout.Left ? Dock.Left : Dock.Right;


        // 左クリック長押しモード
        #region Property: LongLeftButtonDownMode
        private LongButtonDownMode _longLeftButtonDownMode;
        public LongButtonDownMode LongLeftButtonDownMode
        {
            get { return _longLeftButtonDownMode; }
            set { _longLeftButtonDownMode = value; RaisePropertyChanged(); }
        }
        #endregion


        // 背景スタイル
        #region Property: Background
        private BackgroundStyle _background;
        public BackgroundStyle Background
        {
            get { return _background; }
            set { _background = value; UpdateBackgroundBrush(); RaisePropertyChanged(); }
        }
        #endregion

        /// <summary>
        /// CustomBackground property.
        /// </summary>
        private BrushSource _CustomBackground;
        public BrushSource CustomBackground
        {
            get { return _CustomBackground; }
            set
            {
                if (_CustomBackground != value)
                {
                    _CustomBackground = value;
                    CustomBackgroundBrush = _CustomBackground.CreateBackBrush();
                    CustomBackgroundFrontBrush = _CustomBackground.CreateFrontBrush();
                }
            }
        }

        /// <summary>
        /// カスタム背景
        /// </summary>
        public Brush CustomBackgroundBrush { get; set; }

        /// <summary>
        /// カスタム背景
        /// </summary>
        public Brush CustomBackgroundFrontBrush { get; set; }

        /// <summary>
        /// チェック模様
        /// </summary>
        public Brush CheckBackgroundBrush { get; } = (DrawingBrush)App.Current.Resources["CheckerBrush"];


        // イメージエフェクト
        public ImageEffect ImageEffector => _models.ImageEffecct;


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
        /// WindowCaptionEmulator property.
        /// </summary>
        public WindowCaptionEmulator WindowCaptionEmulator
        {
            get { return _windowCaptionEmulator; }
            set { if (_windowCaptionEmulator != value) { _windowCaptionEmulator = value; RaisePropertyChanged(); } }
        }

        private WindowCaptionEmulator _windowCaptionEmulator;

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


        /// <summary>
        /// CanvasWidth property.
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

        // コマンドバインド用
        // TODO: メニュー系コントロールが分離したら不要になる？
        public Dictionary<CommandType, RoutedUICommand> BookCommands => RoutedCommandTable.Current.Commands;


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


        // ページスライダー表示フラグ
        public Visibility PageSliderVisibility => _models.BookOperation.GetPageCount() > 0 ? Visibility.Visible : Visibility.Hidden;

        // サムネイルリスト表示状態
        public Visibility ThumbnailListVisibility => _models.BookOperation.GetPageCount() > 0 ? Visibility.Visible : Visibility.Collapsed;


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

        // 本設定 公開
        public Book.Memento BookSetting => BookHub.BookMemento;

        // 最近使ったフォルダー
        #region Property: LastFiles
        private List<Book.Memento> _lastFiles = new List<Book.Memento>();
        public List<Book.Memento> LastFiles
        {
            get { return _lastFiles; }
            set { _lastFiles = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsEnableLastFiles)); }
        }
        #endregion

        // 最近使ったフォルダーの有効フラグ
        public bool IsEnableLastFiles { get { return LastFiles.Count > 0; } }


        // Foregroudh Brush：ファイルページのフォントカラー用
        #region Property: ForegroundBrush
        private Brush _foregroundBrush = Brushes.White;
        public Brush ForegroundBrush
        {
            get { return _foregroundBrush; }
            set { if (_foregroundBrush != value) { _foregroundBrush = value; RaisePropertyChanged(); } }
        }
        #endregion

        // Backgroud Brush
        #region Property: BackgroundBrush
        private Brush _backgroundBrush = Brushes.Black;
        public Brush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { if (_backgroundBrush != value) { _backgroundBrush = value; RaisePropertyChanged(); UpdateForegroundBrush(); } }
        }
        #endregion

        /// <summary>
        /// BackgroundFrontBrush property.
        /// </summary>
        private Brush _BackgroundFrontBrush;
        public Brush BackgroundFrontBrush
        {
            get { return _BackgroundFrontBrush; }
            set { if (_BackgroundFrontBrush != value) { _BackgroundFrontBrush = value; RaisePropertyChanged(); } }
        }


        #region Property: MenuColor
        private PanelColor _menuColor;
        public PanelColor PanelColor
        {
            get { return _menuColor; }
            set { if (_menuColor != value) { _menuColor = value; FlushPanelColor(); RaisePropertyChanged(); } }
        }
        public void FlushPanelColor()
        {
            if (App.Current == null) return;

            int alpha = _panelOpacity * 0xFF / 100;
            if (alpha > 0xff) alpha = 0xff;
            if (alpha < 0x00) alpha = 0x00;
            if (_menuColor == PanelColor.Dark)
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


        #region Property: Address
        private string _address;
        public string Address
        {
            get { return _address; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                if (_address != value)
                {
                    _address = value;
                    RaisePropertyChanged();
                    if (_address != BookHub.Address)
                    {
                        Load(value);
                    }
                }
                RaisePropertyChanged(nameof(IsBookmark));
            }
        }
        #endregion

        #region Property: IsBookmark
        public bool IsBookmark
        {
            get { return ModelContext.BookMementoCollection.Find(BookHub.Address)?.BookmarkNode != null; }
        }
        #endregion

        #region Property: ContextMenuSetting
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
        #endregion


        //
        #region Property: ContextMenu
        private ContextMenu _contextMenu;
        public ContextMenu ContextMenu
        {
            get { return _contextMenu; }
            set { _contextMenu = value; RaisePropertyChanged(); }
        }
        #endregion

        public void UpdateContextMenu()
        {
            ContextMenu = ContextMenuSetting.ContextMenu;
        }

        /// <summary>
        /// コンテキストメニューを開く
        /// ＃正常に機能しない
        /// </summary>
        public void OpenContextMenu()
        {
            if (ContextMenu != null)
            {
                ContextMenu.DataContext = this;
                ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                ContextMenu.IsOpen = true;
            }
        }

        public bool CanOpenContextMenu()
        {
            return ContextMenu != null && ContextMenu.IsOpen == false;
        }


        #region Property: MainMenu
        private Menu _mainMenu;
        public Menu MainMenu
        {
            get { return _mainMenu; }
            set { _mainMenu = value; RaisePropertyChanged(); }
        }
        #endregion

        public MenuTree MainMenuSource { get; set; }

        public void MainMenuInitialize()
        {
            MainMenuSource = MenuTree.CreateDefault();
            MainMenu = MainMenuSource.CreateMenu();
            BindingOperations.SetBinding(MainMenu, Menu.BackgroundProperty, new Binding("Background") { ElementName = "MainMenuJoint" });
            BindingOperations.SetBinding(MainMenu, Menu.ForegroundProperty, new Binding("Foreground") { ElementName = "MainMenuJoint" });
        }

        //
        public void OpenMainMenuHelp()
        {
            var groups = new Dictionary<string, List<MenuTree.TableData>>();

            //
            foreach (var group in MainMenuSource.Children)
            {
                groups.Add(group.Label, group.GetTable(0));
            }

            // 
            System.IO.Directory.CreateDirectory(Temporary.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.TempSystemDirectory, "MainMenuList.html");


            //
            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                var regex = new Regex(@"\(_(\w)\)");
                var regexReplace = @"($1)";

                writer.WriteLine(NVUtility.HtmlHelpHeader("NeeView MainMenu List"));

                writer.WriteLine("<body><h1>NeeView メインメニュー</h1>");

                foreach (var pair in groups)
                {
                    writer.WriteLine($"<h3>{regex.Replace(pair.Key, regexReplace)}</h3>");
                    writer.WriteLine("<table>");
                    writer.WriteLine($"<th>項目<th>説明<tr>");
                    foreach (var item in pair.Value)
                    {
                        string name = new string('　', item.Depth * 2) + regex.Replace(item.Element.Label, regexReplace);

                        writer.WriteLine($"<td>{name}<td>{item.Element.Note}<tr>");
                    }
                    writer.WriteLine("</table>");
                }
                writer.WriteLine("</body>");

                writer.WriteLine(NVUtility.HtmlHelpFooter());
            }

            System.Diagnostics.Process.Start(fileName);
        }


        // オンラインヘルプ
        public void OpenOnlineHelp()
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }


        // 本管理
        public BookHub BookHub { get; private set; }


        // サムネイル有効
        #region Property: IsEnableThumbnailList
        private bool _isEnableThumbnailList;
        public bool IsEnableThumbnailList
        {
            get { return _isEnableThumbnailList; }
            set
            {
                _isEnableThumbnailList = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        //
        public bool ToggleVisibleThumbnailList()
        {
            IsEnableThumbnailList = !IsEnableThumbnailList;
            return IsEnableThumbnailList;
        }

        // サムネイルを自動的に隠す
        #region Property: IsHideThumbnailList
        private bool _isHideThumbnailList;
        public bool IsHideThumbnailList
        {
            get { return _isHideThumbnailList; }
            set
            {
                _isHideThumbnailList = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        //
        public bool ToggleHideThumbnailList()
        {
            IsHideThumbnailList = !IsHideThumbnailList;
            return IsHideThumbnailList;
        }

        public bool CanHideThumbnailList => IsEnableThumbnailList && IsHideThumbnailList;

        // サムネイルリストとスライダー
        // ONのときはスライダーに連結
        // OFFのときはページ番号に連結
        public bool IsSliderLinkedThumbnailList { get; set; }

        public bool CanSliderLinkedThumbnailList => IsEnableThumbnailList && IsSliderLinkedThumbnailList;

        // サムネイルサイズ
        #region Property: ThumbnailSize
        private double _thumbnailSize;
        public double ThumbnailSize
        {
            get { return _thumbnailSize; }
            set
            {
                if (_thumbnailSize != value)
                {
                    _thumbnailSize = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ThumbnailDispSize));
                }
            }
        }
        #endregion

        // サムネイルサイズ(表示サイズ)
        public double ThumbnailDispSize => _thumbnailSize;


        // ページ番号の表示
        #region Property: IsVisibleThumbnailNumber
        private bool _isVisibleThumbnailNumber;
        public bool IsVisibleThumbnailNumber
        {
            get { return _isVisibleThumbnailNumber; }
            set { _isVisibleThumbnailNumber = value; RaisePropertyChanged(nameof(ThumbnailNumberVisibility)); }
        }
        #endregion

        // ページ番号の表示
        public Visibility ThumbnailNumberVisibility => IsVisibleThumbnailNumber ? Visibility.Visible : Visibility.Collapsed;

        // サムネイル項目の高さ
        public double ThumbnailItemHeight => ThumbnailSize + (IsVisibleThumbnailNumber ? 16 : 0) + 16;

        // サムネイル台紙の表示
        #region Property: IsVisibleThumbnailPlate
        private bool _isVisibleThumbnailPlate;
        public bool IsVisibleThumbnailPlate
        {
            get { return _isVisibleThumbnailPlate; }
            set { _isVisibleThumbnailPlate = value; RaisePropertyChanged(); }
        }
        #endregion



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

        /// <summary>
        /// IsVisibleDevPageList property.
        /// </summary>
        private bool _IsVisibleDevPageList;
        public bool IsVisibleDevPageList
        {
            get { return _IsVisibleDevPageList; }
            set { if (_IsVisibleDevPageList != value) { _IsVisibleDevPageList = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsVisibleDevInfo property.
        /// </summary>
        private bool _IsVisibleDevInfo;
        public bool IsVisibleDevInfo
        {
            get { return _IsVisibleDevInfo; }
            set { if (_IsVisibleDevInfo != value) { _IsVisibleDevInfo = value; RaisePropertyChanged(); } }
        }

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
        /// </summary>
        private bool _isBusyJobEngine;
        public bool IsBusyJobEngine
        {
            get { return _isBusyJobEngine; }
            set { if (_isBusyJobEngine != value) { _isBusyJobEngine = value; RaisePropertyChanged(); } }
        }



        // DPI倍率
        private DpiScale _Dpi => App.Config.Dpi;

        // DPIのXY比率が等しい？
        private bool _IsDpiSquare => App.Config.IsDpiSquare;


        // ダウンロード画像の保存場所
        public string UserDownloadPath { get; set; }

        public string DownloadPath => string.IsNullOrWhiteSpace(UserDownloadPath) ? Temporary.TempDownloadDirectory : UserDownloadPath;

        public string HistoryFileName { get; set; }
        public string BookmarkFileName { get; set; }
        public string PagemarkFileName { get; set; }

        private string _oldPagemarkFileName { get; set; }

        // 保存可否
        public bool IsEnableSave { get; set; } = true;

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
        /// コンストラクター
        /// </summary>
        /// <param name="window"></param>
        public MainWindowVM(MainWindow window)
        {
            MainWindowVM.Current = this;

            // window caption emulatr
            this.WindowCaptionEmulator = new WindowCaptionEmulator(window, window.MenuBar);
            this.WindowCaptionEmulator.IsEnabled = !WindowShape.Current.IsCaptionVisible || WindowShape.Current.IsFullScreen;

            // IsCaptionVisible か IsFullScreen の変更を監視すべきだが、処理が軽いためプロパティ名の判定をしない
            WindowShape.Current.PropertyChanged +=
                (s, e) => this.WindowCaptionEmulator.IsEnabled = !WindowShape.Current.IsCaptionVisible || WindowShape.Current.IsFullScreen;

            // Window Shape
            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.IsFullScreen), WindowShape_IsFullScreenPropertyChanged);


            // Models
            _models = new Models();


            // Side Panel
            _models.SidePanel.ResetFocus += (s, e) => ResetFocus?.Invoke(this, null);

            HistoryFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "History.xml");
            BookmarkFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Bookmark.xml");
            PagemarkFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Pagemark.xml");

            _oldPagemarkFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Pagekmark.xml");

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

            BookHub.BookChanging +=
                OnBookChanging;

            BookHub.BookChanged +=
                OnBookChanged;

            BookHub.SettingChanged +=
                (s, e) =>
                {
                    UpdateIsSliderDirectionReversed();
                    RaisePropertyChanged(nameof(BookSetting));
                    RaisePropertyChanged(nameof(BookHub));
                };

            BookHub.InfoMessage +=
                (s, e) =>
                {
                    _models.InfoMessage.SetMessage(NoticeShowMessageStyle, e);
                };

            BookHub.EmptyMessage +=
                (s, e) => EmptyPageMessage = e;

            BookHub.BookmarkChanged +=
                (s, e) => RaisePropertyChanged(nameof(IsBookmark));

            BookHub.AddressChanged +=
                (s, e) =>
                {
                    _address = BookHub.Address;
                    RaisePropertyChanged(nameof(Address));
                    RaisePropertyChanged(nameof(IsBookmark));
                };


            _models.BookOperation.InfoMessage +=
                (s, e) => _models.InfoMessage.SetMessage(NoticeShowMessageStyle, e);

            _models.PagemarkList.InfoMessage +=
                (s, e) => _models.InfoMessage.SetMessage(NoticeShowMessageStyle, e);


            // Contents
            _models.ContentCanvas.ContentChanged += ContentCanvas_ContentChanged;

            // messenger
            Messenger.AddReciever("UpdateLastFiles", (s, e) => UpdateLastFiles());

            // ダウンロードフォルダー生成
            if (!System.IO.Directory.Exists(Temporary.TempDownloadDirectory))
            {
                System.IO.Directory.CreateDirectory(Temporary.TempDownloadDirectory);
            }
        }

        // コンテンツ変更イベント処理
        private void ContentCanvas_ContentChanged(object sender, EventArgs e)
        {
            // 背景色更新
            UpdateBackgroundBrush();
        }


        // Loading表示状態変更
        public void OnLoading(object sender, string e)
        {
            _models.WindowTitle.LoadingPath = e;
            Loading?.Invoke(sender, e);
        }

        //
        private bool _isBookChanging;


        // 本が変更される
        private void OnBookChanging(object sender, EventArgs e)
        {
            _isBookChanging = true;

            // 未処理のサムネイル要求を解除
            ModelContext.JobEngine.Clear(QueueElementPriority.PageThumbnail);
        }

        // 本が変更された
        private void OnBookChanged(object sender, BookMementoType bookmarkType)
        {
            _isBookChanging = false;

            var title = LoosePath.GetFileName(BookHub.Address);

            App.Current?.Dispatcher.Invoke(() => _models.InfoMessage.SetMessage(NoticeShowMessageStyle, title, null, 2.0, bookmarkType));

            _models.BookOperation.UpdatePageList();
            UpdateLastFiles();

            _models.BookOperation.UpdateIndex();

            if (BookHub.Current == null)
            {
                BookUnloaded?.Invoke(this, null);
            }

            RaisePropertyChanged(nameof(PageSliderVisibility));
            RaisePropertyChanged(nameof(ThumbnailListVisibility));

            //
            CommandManager.InvalidateRequerySuggested();
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

        /// <summary>
        /// 履歴取得
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        internal List<string> GetHistory(int direction, int size)
        {
            return ModelContext.BookHistory.ListUp(this.BookHub.Current?.Address, direction, size);
        }

        /// <summary>
        /// MoveToHistory command.
        /// </summary>
        private RelayCommand<string> _MoveToHistory;
        public RelayCommand<string> MoveToHistory
        {
            get { return _MoveToHistory = _MoveToHistory ?? new RelayCommand<string>(MoveToHistory_Executed); }
        }

        private void MoveToHistory_Executed(string item)
        {
            if (item == null) return;
            this.BookHub.RequestLoad(item, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, true);
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

                ForegroundBrush = (y < 128.0) ? Brushes.White : Brushes.Black;
            }
            else
            {
                ForegroundBrush = Brushes.Black;
            }
        }

        // Background Brush 更新
        public void UpdateBackgroundBrush()
        {
            BackgroundBrush = CreateBackgroundBrush();
            BackgroundFrontBrush = CreateBackgroundFrontBrush(App.Config.Dpi);
        }


        /// <summary>
        /// 背景ブラシ作成
        /// </summary>
        /// <returns></returns>
        public Brush CreateBackgroundBrush()
        {
            switch (this.Background)
            {
                default:
                case BackgroundStyle.Black:
                    return Brushes.Black;
                case BackgroundStyle.White:
                    return Brushes.White;
                case BackgroundStyle.Auto:
                    return new SolidColorBrush(_models.ContentCanvas.GetContentColor());
                case BackgroundStyle.Check:
                    return null;
                case BackgroundStyle.Custom:
                    return CustomBackgroundBrush;
            }
        }

        /// <summary>
        /// 背景ブラシ(画像)作成
        /// </summary>
        /// <param name="dpi">適用するDPI</param>
        /// <returns></returns>
        public Brush CreateBackgroundFrontBrush(DpiScale dpi)
        {
            switch (this.Background)
            {
                default:
                case BackgroundStyle.Black:
                case BackgroundStyle.White:
                case BackgroundStyle.Auto:
                    return null;
                case BackgroundStyle.Check:
                    {
                        var brush = CheckBackgroundBrush.Clone();
                        brush.Transform = new ScaleTransform(1.0 / dpi.DpiScaleX, 1.0 / dpi.DpiScaleY);
                        return brush;
                    }
                case BackgroundStyle.Custom:
                    {
                        var brush = CustomBackgroundFrontBrush?.Clone();
                        // 画像タイルの場合はDPI考慮
                        if (brush is ImageBrush imageBrush && imageBrush.TileMode == TileMode.Tile)
                        {
                            brush.Transform = new ScaleTransform(1.0 / dpi.DpiScaleX, 1.0 / dpi.DpiScaleY);
                        }
                        return brush;
                    }
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
            setting.CommandMememto = CommandTable.Current.CreateMemento();
            setting.DragActionMemento = ModelContext.DragActionTable.CreateMemento();
            setting.ExporterMemento = Exporter.CreateMemento();
            setting.PreferenceMemento = Preference.Current.CreateMemento();
            ////setting.ImageEffectMemento = this.ImageEffector.CreateMemento();

            // new memento
            setting.Memento = Models.Current.CreateMemento();

            return setting;
        }

        // アプリ設定反映
        public void RestoreSetting(Setting setting, bool fromLoad)
        {
            Preference.Current.Restore(setting.PreferenceMemento);
            ModelContext.ApplyPreference();
            WindowShape.Current.WindowChromeFrame = Preference.Current.window_chrome_frame;
            PreferenceAccessor.Current.Reflesh();

            this.Restore(setting.ViewMemento);
            ////this.ImageEffector.Restore(setting.ImageEffectMemento, fromLoad);

            ModelContext.SusieContext.Restore(setting.SusieMemento);
            BookHub.Restore(setting.BookHubMemento);

            CommandTable.Current.Restore(setting.CommandMememto);
            ModelContext.DragActionTable.Restore(setting.DragActionMemento);
            InputGestureChanged?.Invoke(this, null);

            Exporter.Restore(setting.ExporterMemento);

            // new memento
            Models.Current.Resore(setting.Memento, fromLoad);

            // compatible before ver.22
            if (setting.ImageEffectMemento != null)
            {
                Debug.WriteLine($"[[Compatible]]: Restore ImageEffect");
                Models.Current.ImageEffecct.Restore(setting.ImageEffectMemento, fromLoad);
            }
        }

        // 履歴読み込み
        public void LoadHistory(Setting setting)
        {
            BookHistory.Memento memento;

            if (System.IO.File.Exists(HistoryFileName))
            {
                try
                {
                    memento = BookHistory.Memento.Load(HistoryFileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    new MessageDialog($"原因: {e.Message}", "履歴の読み込みに失敗しました").ShowDialog();
                    memento = new BookHistory.Memento();
                }
            }
            else
            {
                memento = new BookHistory.Memento();
            }

            // 設定ファイルに残っている履歴をマージ
            if (setting.BookHistoryMemento != null)
            {
                memento.Merge(setting.BookHistoryMemento);
            }

            // 履歴反映
            ModelContext.BookHistory.Restore(memento, true);
            UpdateLastFiles();

            // フォルダーリストの場所に反映
            _models.FolderList.ResetPlace(ModelContext.BookHistory.LastFolder);
        }

        // ブックマーク読み込み
        public void LoadBookmark(Setting setting)
        {
            BookmarkCollection.Memento memento;

            // ブックマーク読み込み
            if (System.IO.File.Exists(BookmarkFileName))
            {
                try
                {
                    memento = BookmarkCollection.Memento.Load(BookmarkFileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    new MessageDialog($"原因: {e.Message}", "ブックマークの読み込みに失敗しました").ShowDialog();
                    memento = new BookmarkCollection.Memento();
                }
            }
            else
            {
                memento = new BookmarkCollection.Memento();
            }

            // ブックマーク反映
            ModelContext.Bookmarks.Restore(memento);
        }


        // ページマーク読み込み
        public void LoadPagemark(Setting setting)
        {
            PagemarkCollection.Memento memento;

            // 読込ファイル名確定
            string filename = null;
            if (System.IO.File.Exists(PagemarkFileName))
            {
                filename = PagemarkFileName;
            }
            else if (System.IO.File.Exists(_oldPagemarkFileName))
            {
                filename = _oldPagemarkFileName;
            }

            // ページマーク読み込み
            if (filename != null)
            {
                try
                {
                    memento = PagemarkCollection.Memento.Load(filename);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    new MessageDialog($"原因: {e.Message}", "ページマークの読み込みに失敗しました").ShowDialog();
                    memento = new PagemarkCollection.Memento();
                }

                // 旧ファイル名の変更
                if (filename == _oldPagemarkFileName)
                {
                    System.IO.File.Move(filename, PagemarkFileName);
                }
            }
            else
            {
                memento = new PagemarkCollection.Memento();
            }

            // ページマーク反映
            ModelContext.Pagemarks.Restore(memento);
        }



        // アプリ設定保存
        public void SaveSetting()
        {
            if (!IsEnableSave) return;

            // 現在の本を履歴に登録
            BookHub.SaveBookMemento(); // TODO: タイミングに問題有り？

            // 設定
            var setting = CreateSetting();

            // ウィンドウ座標保存
            setting.WindowShape = WindowShape.Current.SnapMemento;

            try
            {
                // 設定をファイルに保存
                setting.Save(App.UserSettingFileName);
            }
            catch { }

            // 保存しないフラグ
            bool disableSave = Preference.Current.userdata_save_disable;

            try
            {
                if (disableSave)
                {
                    // 履歴ファイルを削除
                    RemoveFile(HistoryFileName);
                }
                else
                {
                    // 履歴をファイルに保存
                    var bookHistoryMemento = ModelContext.BookHistory.CreateMemento(true);
                    bookHistoryMemento.Save(HistoryFileName);
                }
            }
            catch { }

            try
            {
                if (disableSave)
                {
                    // ブックマークファイルを削除
                    RemoveFile(BookmarkFileName);
                }
                else
                {
                    // ブックマークをファイルに保存
                    var bookmarkMemento = ModelContext.Bookmarks.CreateMemento(true);
                    bookmarkMemento.Save(BookmarkFileName);
                }
            }
            catch { }

            try
            {
                if (disableSave)
                {
                    // ページマークファイルを削除
                    RemoveFile(PagemarkFileName);
                }
                else
                {
                    // ページマークをファイルに保存
                    var pagemarkMemento = ModelContext.Pagemarks.CreateMemento(true);
                    pagemarkMemento.Save(PagemarkFileName);
                }
            }
            catch { }
        }

        // ファイル削除
        private void RemoveFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }
        }

        #endregion


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


        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            var pageList = _models.BookOperation.PageList;

            if (pageList == null || ThumbnailSize < 8.0) return;

            // サムネイルリストが無効の場合、処理しない
            if (!IsEnableThumbnailList) return;

            // 本の切り替え中は処理しない
            if (_isBookChanging) return;

            // 未処理の要求を解除
            ModelContext.JobEngine.Clear(QueueElementPriority.PageThumbnail);

            // 要求. 中央値優先
            int center = start + count / 2;
            var pages = Enumerable.Range(start - margin, count + margin * 2 - 1)
                .Where(i => i >= 0 && i < pageList.Count)
                .Select(e => pageList[e])
                .OrderBy(e => Math.Abs(e.Index - center));

            foreach (var page in pages)
            {
                page.LoadThumbnail(QueueElementPriority.PageThumbnail);
            }
        }


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
                context.ViewEffect = _models.ImageEffecct.Effect;
                context.Background = CreateBackgroundBrush();
                context.BackgroundFront = CreateBackgroundFrontBrush(new DpiScale(1, 1));

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

            [DataMember]
            public BackgroundStyle Background { get; set; }

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

            [DataMember(Order = 8)]
            public bool IsEnableThumbnailList { get; set; }

            [DataMember(Order = 8)]
            public bool IsHideThumbnailList { get; set; }

            [DataMember(Order = 8)]
            public double ThumbnailSize { get; set; }

            [DataMember(Order = 8)]
            public bool IsSliderLinkedThumbnailList { get; set; }

            [DataMember(Order = 8)]
            public bool IsVisibleThumbnailNumber { get; set; }

            [DataMember(Order = 9)]
            public bool IsAutoGC { get; set; }

            [DataMember(Order = 9)]
            public bool IsVisibleThumbnailPlate { get; set; }

            [DataMember(Order = 10, EmitDefaultValue = false)]
            public ShowMessageStyle ViewTransformShowMessageStyle { get; set; } // no used (ver.23)

            [DataMember(Order = 10, EmitDefaultValue = false)]
            public bool IsOriginalScaleShowMessage { get; set; } // no used (ver.23)

            [DataMember(Order = 12, EmitDefaultValue = false)]
            public double ContentsSpace { get; set; } // no used (ver.23)

            [DataMember(Order = 12)]
            public LongButtonDownMode LongLeftButtonDownMode { get; set; }

            [DataMember(Order = 16)]
            public SliderDirection SliderDirection { get; set; }

            [DataMember(Order = 17)]
            public bool IsHidePageSlider { get; set; }

            [DataMember(Order = 18, EmitDefaultValue = false)]
            public bool IsAutoRotate { get; set; } // no used (ver.23)

            [DataMember(Order = 19)]
            public bool IsVisibleWindowTitle { get; set; }

            [DataMember(Order = 19)]
            public bool IsVisibleLoupeInfo { get; set; }

            [DataMember(Order = 20, EmitDefaultValue = false)]
            public bool IsSliderWithIndex { get; set; } // no used

            [DataMember(Order = 20)]
            public bool IsLoupeCenter { get; set; }

            [DataMember(Order = 21)]
            public SliderIndexLayout SliderIndexLayout { get; set; }

            [DataMember(Order = 21)]
            public BrushSource CustomBackground { get; set; }

            //
            private void Constructor()
            {
                _Version = App.Config.ProductVersionNumber;
                IsLimitMove = true; // no used.
                NoticeShowMessageStyle = ShowMessageStyle.Normal;
                CommandShowMessageStyle = ShowMessageStyle.Normal; // no used.
                GestureShowMessageStyle = ShowMessageStyle.Normal;
                NowLoadingShowMessageStyle = ShowMessageStyle.Normal;
                ////ViewTransformShowMessageStyle = ShowMessageStyle.None;
                ////StretchMode = PageStretchMode.Uniform;
                Background = BackgroundStyle.Black;
                PanelColor = PanelColor.Dark;
                WindowTitleFormat1 = WindowTitle.WindowTitleFormat1Default; // no used.
                WindowTitleFormat2 = WindowTitle.WindowTitleFormat2Default; // no used.
                IsSaveWindowPlacement = true;
                IsHidePanelInFullscreen = true;
                IsVisibleTitleBar = true; // no used
                ContextMenuSetting = new ContextMenuSetting();
                IsHideThumbnailList = true;
                ThumbnailSize = 96;
                IsSliderLinkedThumbnailList = true;
                IsAutoGC = true;
                IsVisibleThumbnailPlate = true;
                ////ContentsSpace = -1.0;
                LongLeftButtonDownMode = LongButtonDownMode.Loupe;
                IsDisableMultiBoot = true;
                SliderDirection = SliderDirection.RightToLeft;
                IsVisibleWindowTitle = true;
                IsVisibleLoupeInfo = true;
                SliderIndexLayout = SliderIndexLayout.Right;
                CustomBackground = new BrushSource();
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

            ////memento.IsLimitMove = this.IsLimitMove;
            ////memento.IsControlCenterImage = this.IsControlCenterImage;
            ////memento.AngleFrequency = this.AngleFrequency;
            ////memento.IsViewStartPositionCenter = this.IsViewStartPositionCenter;
            ////memento.StretchMode = this.StretchMode;
            memento.CustomBackground = this.CustomBackground;
            memento.Background = this.Background;
            memento.NoticeShowMessageStyle = this.NoticeShowMessageStyle;
            memento.CommandShowMessageStyle = ShowMessageStyle.None; // no used.
            memento.GestureShowMessageStyle = this.GestureShowMessageStyle;
            memento.NowLoadingShowMessageStyle = this.NowLoadingShowMessageStyle;
            ////memento.ViewTransformShowMessageStyle = this.ViewTransformShowMessageStyle;
            ////memento.IsEnabledNearestNeighbor = this.IsEnabledNearestNeighbor;
            ////memento.IsKeepScale = this.IsKeepScale;
            ////memento.IsKeepAngle = this.IsKeepAngle;
            ////memento.IsKeepFlip = this.IsKeepFlip;
            memento.IsDisableMultiBoot = this.IsDisableMultiBoot;
            memento.IsAutoPlaySlideShow = this.IsAutoPlaySlideShow;
            memento.IsSaveWindowPlacement = this.IsSaveWindowPlacement;
            memento.IsHideMenu = this.IsHideMenu;
            memento.IsHidePageSlider = this.IsHidePageSlider;
            memento.IsVisibleTitleBar = false; // no used.
            memento.IsSaveFullScreen = this.IsSaveFullScreen;
            memento.UserDownloadPath = this.UserDownloadPath;
            memento.PanelColor = this.PanelColor;
            ////memento.WindowTitleFormat1 = this.WindowTitleFormat1;
            ////memento.WindowTitleFormat2 = this.WindowTitleFormat2;
            memento.IsVisibleAddressBar = this.IsVisibleAddressBar;
            memento.IsHidePanel = this.IsHidePanel;
            memento.IsHidePanelInFullscreen = this.IsHidePanelInFullscreen;
            memento.ContextMenuSetting = this.ContextMenuSetting.Clone();
            memento.IsEnableThumbnailList = this.IsEnableThumbnailList;
            memento.IsHideThumbnailList = this.IsHideThumbnailList;
            memento.ThumbnailSize = this.ThumbnailSize;
            memento.IsSliderLinkedThumbnailList = this.IsSliderLinkedThumbnailList;
            memento.IsVisibleThumbnailNumber = this.IsVisibleThumbnailNumber;
            memento.IsAutoGC = this.IsAutoGC;
            memento.IsVisibleThumbnailPlate = this.IsVisibleThumbnailPlate;
            ////memento.IsOriginalScaleShowMessage = this.IsOriginalScaleShowMessage;
            ////memento.ContentsSpace = this.ContentsSpace;
            memento.LongLeftButtonDownMode = this.LongLeftButtonDownMode;
            memento.SliderDirection = this.SliderDirection;
            ////memento.IsAutoRotate = this.IsAutoRotate;
            memento.IsVisibleWindowTitle = this.IsVisibleWindowTitle;
            memento.IsVisibleLoupeInfo = this.IsVisibleLoupeInfo;
            memento.IsLoupeCenter = this.IsLoupeCenter;
            memento.SliderIndexLayout = this.SliderIndexLayout;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            ////this.IsLimitMove = memento.IsLimitMove;
            ////this.IsControlCenterImage = memento.IsControlCenterImage;
            ////this.AngleFrequency = memento.AngleFrequency;
            ////this.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
            ////this.StretchMode = memento.StretchMode;
            this.CustomBackground = memento.CustomBackground;
            this.Background = memento.Background;
            this.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
            this.GestureShowMessageStyle = memento.GestureShowMessageStyle;
            this.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
            ////this.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;
            ////this.IsEnabledNearestNeighbor = memento.IsEnabledNearestNeighbor;
            ////this.IsKeepScale = memento.IsKeepScale;
            ////this.IsKeepAngle = memento.IsKeepAngle;
            ////this.IsKeepFlip = memento.IsKeepFlip;
            this.IsDisableMultiBoot = memento.IsDisableMultiBoot;
            this.IsAutoPlaySlideShow = memento.IsAutoPlaySlideShow;
            this.IsSaveWindowPlacement = memento.IsSaveWindowPlacement;
            this.IsHideMenu = memento.IsHideMenu;
            this.IsHidePageSlider = memento.IsHidePageSlider;
            this.IsSaveFullScreen = memento.IsSaveFullScreen;
            this.UserDownloadPath = memento.UserDownloadPath;
            this.PanelColor = memento.PanelColor;
            this.IsHidePanel = memento.IsHidePanel;
            ////this.WindowTitleFormat1 = memento.WindowTitleFormat1;
            ////this.WindowTitleFormat2 = memento.WindowTitleFormat2;
            this.IsVisibleAddressBar = memento.IsVisibleAddressBar;
            this.IsHidePanelInFullscreen = memento.IsHidePanelInFullscreen;
            this.ContextMenuSetting = memento.ContextMenuSetting.Clone();
            this.IsEnableThumbnailList = memento.IsEnableThumbnailList;
            this.IsHideThumbnailList = memento.IsHideThumbnailList;
            this.ThumbnailSize = memento.ThumbnailSize;
            this.IsSliderLinkedThumbnailList = memento.IsSliderLinkedThumbnailList;
            this.IsVisibleThumbnailNumber = memento.IsVisibleThumbnailNumber;
            this.IsAutoGC = memento.IsAutoGC;
            this.IsVisibleThumbnailPlate = memento.IsVisibleThumbnailPlate;
            ////this.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
            ////this.ContentsSpace = memento.ContentsSpace;
            this.LongLeftButtonDownMode = memento.LongLeftButtonDownMode;
            this.SliderDirection = memento.SliderDirection;
            ////this.IsAutoRotate = memento.IsAutoRotate;
            this.IsVisibleWindowTitle = memento.IsVisibleWindowTitle;
            this.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
            this.IsLoupeCenter = memento.IsLoupeCenter;
            this.SliderIndexLayout = memento.SliderIndexLayout;

            ////ViewChanged?.Invoke(this, new ViewChangeArgs() { ResetViewTransform = true });
            ////UpdateContentSize();

            // compatible before ver.22
            if (memento.FileInfoSetting != null)
            {
                Debug.WriteLine($"[[Compatible]]: Restore FileInfoSetting");
                _models.FileInformation.IsUseExifDateTime = memento.FileInfoSetting.IsUseExifDateTime;
                _models.FileInformation.IsVisibleBitsPerPixel = memento.FileInfoSetting.IsVisibleBitsPerPixel;
                _models.FileInformation.IsVisibleLoader = memento.FileInfoSetting.IsVisibleLoader;
            }
            if (memento.FolderListSetting != null)
            {
                Debug.WriteLine($"[[Compatible]]: Restore FolderListSetting");
                _models.FolderList.IsVisibleBookmarkMark = memento.FolderListSetting.IsVisibleBookmarkMark;
                _models.FolderList.IsVisibleHistoryMark = memento.FolderListSetting.IsVisibleHistoryMark;
            }
            if (memento._Version < Config.GenerateProductVersionNumber(1, 22, 0))
            {
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

                _models.WindowTitle.WindowTitleFormat1 = memento.WindowTitleFormat1;
                _models.WindowTitle.WindowTitleFormat2 = memento.WindowTitleFormat2;
            }
        }

        #endregion
    }
}
