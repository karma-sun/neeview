// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Effects;
using NeeView.Utility;
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

    // ViewChangedイベント引数
    public class ViewChangeArgs
    {
        public int PageDirection { get; set; }

        public DragViewOrigin ViewOrigin { get; set; }

        public double Angle { get; set; }

        public bool ResetViewTransform { get; set; }
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

    // パネル種類
    public enum PanelType
    {
        None,
        FileInfo,
        EffectInfo,
        FolderList,
        HistoryList,
        BookmarkList,
        PagemarkList,
        PageList, // 廃止。イベントで使用される
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
    /// ViewModel
    /// </summary>
    public class MainWindowVM : INotifyPropertyChanged, IDisposable
    {
        #region Events

        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        // ロード中通知
        public event EventHandler<string> Loading;

        // 表示変更を通知
        public event EventHandler<ViewChangeArgs> ViewChanged;

        // ショートカット変更を通知
        public event EventHandler InputGestureChanged;

        // ウィンドウモード変更通知
        public event EventHandler NotifyMenuVisibilityChanged;

        // ページリスト更新
        public event EventHandler PageListChanged;

        // インデックス更新
        public event EventHandler IndexChanged;

        // 本を閉じた
        public event EventHandler BookUnloaded;

        //
        public event EventHandler<PanelType> LeftPanelVisibled;
        public event EventHandler<PanelType> RightPanelVisibled;

        #endregion


        // 移動制限モード
        public bool IsLimitMove { get; set; }

        // 回転、拡縮をコンテンツの中心基準にする
        public bool IsControlCenterImage { get; set; }

        // 回転単位
        public double AngleFrequency { get; set; }

        // 表示開始時の基準
        public bool IsViewStartPositionCenter { get; set; }

        // 通知表示スタイル
        public ShowMessageStyle NoticeShowMessageStyle { get; set; }

        // コマンド表示スタイル
        public ShowMessageStyle CommandShowMessageStyle { get; set; }

        // ジェスチャー表示スタイル
        public ShowMessageStyle GestureShowMessageStyle { get; set; }

        // NowLoading表示スタイル
        public ShowMessageStyle NowLoadingShowMessageStyle { get; set; }

        // View変換情報表示スタイル
        public ShowMessageStyle ViewTransformShowMessageStyle { get; set; }

        // View変換情報表示のスケール表示をオリジナルサイズ基準にする
        public bool IsOriginalScaleShowMessage { get; set; }

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

        // 長押し判定時間(秒)
        #region Property: LongButtonDownTick
        private double _longButtonDownTick;
        public double LongButtonDownTick
        {
            get { return _longButtonDownTick; }
            set { _longButtonDownTick = value; RaisePropertyChanged(); }
        }
        #endregion




        // スケールモード
        #region Property: StretchMode
        private PageStretchMode _stretchModePrev = PageStretchMode.Uniform;
        private PageStretchMode _stretchMode = PageStretchMode.Uniform;
        public PageStretchMode StretchMode
        {
            get { return _stretchMode; }
            set
            {
                if (_stretchMode != value)
                {
                    _stretchModePrev = _stretchMode;
                    _stretchMode = value;
                    RaisePropertyChanged();
                    UpdateContentSize();
                    ViewChanged?.Invoke(this, new ViewChangeArgs() { ResetViewTransform = true });
                }
            }
        }

        // トグル
        public PageStretchMode GetToggleStretchMode(ToggleStretchModeCommandParameter param)
        {
            PageStretchMode mode = StretchMode;
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                var next = (int)mode + 1;
                if (!param.IsLoop && next >= length) return StretchMode;
                mode = (PageStretchMode)(next % length);
                if (param.StretchModes[mode]) return mode;
            }
            while (count++ < length);
            return StretchMode;
        }

        // 逆トグル
        public PageStretchMode GetToggleStretchModeReverse(ToggleStretchModeCommandParameter param)
        {
            PageStretchMode mode = StretchMode;
            int length = Enum.GetNames(typeof(PageStretchMode)).Length;
            int count = 0;
            do
            {
                var prev = (int)mode - 1;
                if (!param.IsLoop && prev < 0) return StretchMode;
                mode = (PageStretchMode)((prev + length) % length);
                if (param.StretchModes[mode]) return mode;
            }
            while (count++ < length);
            return StretchMode;
        }


        //
        public void SetStretchMode(PageStretchMode mode, bool isToggle)
        {
            StretchMode = GetFixedStretchMode(mode, isToggle);
        }

        //
        public bool TestStretchMode(PageStretchMode mode, bool isToggle)
        {
            return mode == GetFixedStretchMode(mode, isToggle);
        }

        //
        private PageStretchMode GetFixedStretchMode(PageStretchMode mode, bool isToggle)
        {
            if (isToggle && StretchMode == mode)
            {
                return (mode == PageStretchMode.None) ? _stretchModePrev : PageStretchMode.None;
            }
            else
            {
                return mode;
            }
        }

        #endregion


        //
        /// <summary>
        /// IsAutoRotate property.
        /// </summary>
        private bool _isAutoRotate;
        public bool IsAutoRotate
        {
            get { return _isAutoRotate; }
            set { if (_isAutoRotate != value) { _isAutoRotate = value; RaisePropertyChanged(); } }
        }

        public event EventHandler AutoRotateChanged;

        public bool ToggleAutoRotate()
        {
            IsAutoRotate = !IsAutoRotate;
            UpdateContentSize(GetAutoRotateAngle());
            AutoRotateChanged?.Invoke(this, null);
            return IsAutoRotate;
        }


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
        public ImageEffect ImageEffector { get; set; } = new ImageEffect();

        // ドットのまま拡大
        #region Property: IsEnabledNearestNeighbor
        private bool _isEnabledNearestNeighbor;
        public bool IsEnabledNearestNeighbor
        {
            get { return _isEnabledNearestNeighbor; }
            set
            {
                if (_isEnabledNearestNeighbor != value)
                {
                    _isEnabledNearestNeighbor = value;
                    RaisePropertyChanged();
                    UpdateContentScalingMode();
                }
            }
        }
        #endregion

        // 拡大率キープ
        public bool IsKeepScale { get; set; }

        // 回転キープ
        public bool IsKeepAngle { get; set; }

        // 反転キープ
        public bool IsKeepFlip { get; set; }

        // メニューを自動的に隠す
        #region Property: IsHideMenu
        private bool _isHideMenu;
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set { _isHideMenu = value; RaisePropertyChanged(); NotifyMenuVisibilityChanged?.Invoke(this, null); }
        }

        //
        public bool ToggleHideMenu()
        {
            IsHideMenu = !IsHideMenu;
            return IsHideMenu;
        }
        #endregion

        // スライダーを自動的に隠す
        #region Property: IsHidePageSlider
        private bool _isIsHidePageSlider;
        public bool IsHidePageSlider
        {
            get { return _isIsHidePageSlider; }
            set { _isIsHidePageSlider = value; RaisePropertyChanged(); NotifyMenuVisibilityChanged?.Invoke(this, null); }
        }

        //
        public bool ToggleHidePageSlider()
        {
            IsHidePageSlider = !IsHidePageSlider;
            return IsHidePageSlider;
        }
        #endregion

        // パネルを自動的に隠す
        #region Property: IsHidePanel
        private bool _isHidePanel;
        public bool IsHidePanel
        {
            get { return _isHidePanel; }
            set { _isHidePanel = value; RaisePropertyChanged(); NotifyMenuVisibilityChanged?.Invoke(this, null); }
        }
        public bool ToggleHidePanel()
        {
            IsHidePanel = !IsHidePanel;
            return IsHidePanel;
        }

        // フルスクリーン時にパネルを隠す
        public bool IsHidePanelInFullscreen { get; set; }

        // パネルを自動的に隠せるか
        public bool CanHidePanel => IsHidePanel || (IsHidePanelInFullscreen && IsFullScreen);

        #endregion


        /// <summary>
        /// IsContentPanelStyle property.
        /// </summary>
        public bool IsContentPanelStyle
        {
            get { return this.FolderListItemStyle == FolderListItemStyle.Picture; }
            set { this.FolderListItemStyle = value ? FolderListItemStyle.Picture : FolderListItemStyle.Normal; RaisePropertyChanged(); }
        }

        public void TogglePanelStyle()
        {
            IsContentPanelStyle = !IsContentPanelStyle;
        }


        /// <summary>
        /// IsContentPageListStyle property.
        /// </summary>
        public bool IsContentPageListStyle
        {
            get { return this.PageListItemStyle == FolderListItemStyle.Picture; }
            set { this.PageListItemStyle = value ? FolderListItemStyle.Picture : FolderListItemStyle.Normal; RaisePropertyChanged(); }
        }

        public void TogglePageListStyle()
        {
            IsContentPageListStyle = !IsContentPageListStyle;
        }





        // タイトルバーON/OFF
        #region Property: IsVisibleTitleBar
        private bool _isVisibleTitleBar;
        public bool IsVisibleTitleBar
        {
            get { return _isVisibleTitleBar; }
            set
            {
                _isVisibleTitleBar = value;
                FullScreenManager.WindowStyleMemento = value ? WindowStyle.SingleBorderWindow : WindowStyle.None;
                RaisePropertyChanged();
                NotifyMenuVisibilityChanged?.Invoke(this, null);
            }
        }
        public bool ToggleVisibleTitleBar()
        {
            IsVisibleTitleBar = !IsVisibleTitleBar;
            return IsVisibleTitleBar;
        }
        #endregion

        /// <summary>
        /// IsVisibleWindowTitle property.
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
            set { _isVisibleAddressBar = value; RaisePropertyChanged(); NotifyMenuVisibilityChanged?.Invoke(this, null); }
        }
        public bool ToggleVisibleAddressBar()
        {
            IsVisibleAddressBar = !IsVisibleAddressBar;
            return IsVisibleAddressBar;
        }
        #endregion



        // ファイル情報表示ON/OFF
        public bool IsVisibleFileInfo
        {
            get { return RightPanel == PanelType.FileInfo; }
            set { RightPanel = value ? PanelType.FileInfo : PanelType.None; }
        }

        public bool ToggleVisibleFileInfo(bool byMenu)
        {
            IsVisibleFileInfo = byMenu ? !IsVisibleFileInfo : !(IsVisibleFileInfo && IsVisibleRightPanel);
            return IsVisibleFileInfo;
        }


        // エフェクト情報表示ON/OFF
        public bool IsVisibleEffectInfo
        {
            get { return RightPanel == PanelType.EffectInfo; }
            set { RightPanel = value ? PanelType.EffectInfo : PanelType.None; }
        }

        public bool ToggleVisibleEffectInfo(bool byMenu)
        {
            IsVisibleEffectInfo = byMenu ? !IsVisibleEffectInfo : !(IsVisibleEffectInfo && IsVisibleRightPanel);
            return IsVisibleEffectInfo;
        }


        // フォルダーリスト表示ON/OFF
        public bool IsVisibleFolderList
        {
            get { return LeftPanel == PanelType.FolderList; }
            set { LeftPanel = value ? PanelType.FolderList : PanelType.None; }
        }

        //
        public bool ToggleVisibleFolderList(bool byMenu)
        {
            IsVisibleFolderList = byMenu ? !IsVisibleFolderList : !(IsVisibleFolderList && IsVisibleLeftPanel);
            return IsVisibleFolderList;
        }

        // ページリスト表示ON/OFF
        #region Property: IsVisiblePageList
        private bool _isVisiblePageList = true;
        public bool IsVisiblePageList
        {
            get { return _isVisiblePageList; }
            set
            {
                if (_isVisiblePageList != value)
                {
                    _isVisiblePageList = value;
                    if (_isVisiblePageList)
                    {
                        FolderListGridRow0 = "*";
                        FolderListGridRow2 = "*";
                    }
                    else
                    {
                        FolderListGridRow0 = "*";
                        FolderListGridRow2 = "0";
                    }
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsVisiblePageListMenu));
                }
            }
        }
        #endregion

        //
        public bool IsVisiblePageListMenu => IsVisiblePageList && IsVisibleFolderList;

        //
        public bool ToggleVisiblePageList()
        {
            IsVisiblePageList = !IsVisiblePageListMenu;
            IsVisibleFolderList = true;
            LeftPanelVisibled?.Invoke(this, PanelType.PageList);
            return IsVisiblePageList;
        }



        // 履歴リスト表示ON/OFF
        public bool IsVisibleHistoryList
        {
            get { return LeftPanel == PanelType.HistoryList; }
            set { LeftPanel = value ? PanelType.HistoryList : PanelType.None; }
        }

        //
        public bool ToggleVisibleHistoryList(bool byMenu)
        {
            IsVisibleHistoryList = byMenu ? !IsVisibleHistoryList : !(IsVisibleHistoryList && IsVisibleLeftPanel);
            return IsVisibleHistoryList;
        }


        // ブックマークリスト表示ON/OFF
        public bool IsVisibleBookmarkList
        {
            get { return LeftPanel == PanelType.BookmarkList; }
            set { LeftPanel = value ? PanelType.BookmarkList : PanelType.None; }
        }

        //
        public bool ToggleVisibleBookmarkList(bool byMenu)
        {
            IsVisibleBookmarkList = byMenu ? !IsVisibleBookmarkList : !(IsVisibleBookmarkList && IsVisibleLeftPanel);
            return IsVisibleBookmarkList;
        }


        // ページマークリスト表示ON/OFF
        public bool IsVisiblePagemarkList
        {
            get { return LeftPanel == PanelType.PagemarkList; }
            set { LeftPanel = value ? PanelType.PagemarkList : PanelType.None; }
        }

        //
        public bool ToggleVisiblePagemarkList(bool byMenu)
        {
            IsVisiblePagemarkList = byMenu ? !IsVisiblePagemarkList : !(IsVisiblePagemarkList && IsVisibleLeftPanel);
            return IsVisiblePagemarkList;
        }

        // 左パネル
        #region Property: LeftPanel
        private PanelType _leftPanel;
        public PanelType LeftPanel
        {
            get { return _leftPanel; }
            set
            {
                _leftPanel = value;
                if (_leftPanel == PanelType.PageList) _leftPanel = PanelType.FolderList; // PageList廃止の補正
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsVisibleFolderList));
                RaisePropertyChanged(nameof(IsVisibleHistoryList));
                RaisePropertyChanged(nameof(IsVisibleBookmarkList));
                RaisePropertyChanged(nameof(IsVisiblePagemarkList));
                RaisePropertyChanged(nameof(IsVisiblePageListMenu));
                NotifyMenuVisibilityChanged?.Invoke(this, null);

                LeftPanelVisibled?.Invoke(this, _leftPanel);
            }
        }
        #endregion

        public bool IsVisibleLeftPanel { get; set; } = false;

        // 右パネル
        #region Property: RightPanel
        private PanelType _rightPanel;
        public PanelType RightPanel
        {
            get { return _rightPanel; }
            set
            {
                _rightPanel = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsVisibleFileInfo));
                RaisePropertyChanged(nameof(IsVisibleEffectInfo));
                UpdateFileInfoContent();
                NotifyMenuVisibilityChanged?.Invoke(this, null);

                RightPanelVisibled?.Invoke(this, _rightPanel);
            }
        }
        #endregion

        public bool IsVisibleRightPanel { get; set; } = false;


        // パネル幅
        public double LeftPanelWidth { get; set; } = 250;
        public double RightPanelWidth { get; set; } = 250;

        #region Property: FolderListGridLength0
        private GridLength _folderListGridLength0 = new GridLength(1, GridUnitType.Star);
        public GridLength FolderListGridLength0
        {
            get { return _folderListGridLength0; }
            set { _folderListGridLength0 = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Property: FolderListGridLength2
        private GridLength _folderListGridLength2 = new GridLength(0, GridUnitType.Pixel);
        public GridLength FolderListGridLength2
        {
            get { return _folderListGridLength2; }
            set { _folderListGridLength2 = value; RaisePropertyChanged(); }
        }
        #endregion

        //
        public string FolderListGridRow0
        {
            get { return FolderListGridLength0.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        var converter = new GridLengthConverter();
                        FolderListGridLength0 = (GridLength)converter.ConvertFromString(value);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        //
        public string FolderListGridRow2
        {
            get { return FolderListGridLength2.ToString(); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        var converter = new GridLengthConverter();
                        FolderListGridLength2 = (GridLength)converter.ConvertFromString(value);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        // フォルダーリスト項目表示種類
        #region Property: FolderListItemStyle
        private FolderListItemStyle _folderListItemStyle;
        public FolderListItemStyle FolderListItemStyle
        {
            get { return _folderListItemStyle; }
            set
            {
                _folderListItemStyle = value;
                RaisePropertyChanged();
                PanelContext.FolderListItemStyle = _folderListItemStyle;
                RaisePropertyChanged(nameof(IsContentPanelStyle));
            }
        }
        #endregion


        // ページリスト項目表示種類
        #region Property: PageListItemStyle
        private FolderListItemStyle _pageListItemStyle;
        public FolderListItemStyle PageListItemStyle
        {
            get { return _pageListItemStyle; }
            set
            {
                _pageListItemStyle = value;
                RaisePropertyChanged();
                PanelContext.PageListItemStyle = _pageListItemStyle;
                RaisePropertyChanged(nameof(IsContentPageListStyle));
            }
        }
        #endregion



        // フルスクリーン
        #region Property: FullScreenManager

        //フルスクリーン管理
        public FullScreenManager FullScreenManager { get; set; }

        //
        public bool IsFullScreen
        {
            get { return FullScreenManager.IsFullScreen; }
            set { FullScreenManager.IsFullScreen = value; }
        }

        //
        public bool ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
            return IsFullScreen;
        }

        //
        public bool IsSaveFullScreen { get; set; }

        #endregion


        // 常に手前に表示
        #region Property: IsTopmost
        private bool _isTopmost;
        public bool IsTopmost
        {
            get { return _isTopmost; }
            set { _isTopmost = value; RaisePropertyChanged(); }
        }
        public bool ToggleTopmost()
        {
            IsTopmost = !IsTopmost;
            return IsTopmost;
        }
        #endregion

        // 最後のフォルダーを開く
        public bool IsLoadLastFolder { get; set; }

        // マルチブートを禁止する
        public bool IsDisableMultiBoot { get; set; }

        // スライドショーの自動開始
        public bool IsAutoPlaySlideShow { get; set; }

        // ウィンドウ座標を復元する
        public bool IsSaveWindowPlacement { get; set; }

        // コマンドバインド用
        // View側で定義されます
        public Dictionary<CommandType, RoutedUICommand> BookCommands { get; set; }

        // 空フォルダー通知表示のON/OFF
        #region Property: IsVisibleEmptyPageMessage
        private bool _isVisibleEmptyPageMessage = false;
        public bool IsVisibleEmptyPageMessage
        {
            get { return _isVisibleEmptyPageMessage; }
            set { if (_isVisibleEmptyPageMessage != value) { _isVisibleEmptyPageMessage = value; RaisePropertyChanged(); } }
        }
        #endregion

        // 空フォルダー通知表示の詳細テキスト
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


        public bool IsPermitSliderCall { get; set; } = true;

        // 現在ページ番号
        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                _index = NVUtility.Clamp(value, 0, IndexMax);
                if (!CanSliderLinkedThumbnailList)
                {
                    BookHub.SetPageIndex(_index);
                }
                RaisePropertyChanged();
                IndexChanged?.Invoke(this, null);
            }
        }

        // 最大ページ番号
        public int IndexMax
        {
            get { return BookHub.GetPageCount(); }
        }

        //
        private void UpdateIndex()
        {
            _index = BookHub.GetPageIndex();
            RaisePropertyChanged(nameof(Index));
            RaisePropertyChanged(nameof(IndexMax));
            IndexChanged?.Invoke(this, null);
        }

        //
        public void SetIndex(int index)
        {
            _index = index;
            BookHub.SetPageIndex(_index);
            RaisePropertyChanged(nameof(Index));
            RaisePropertyChanged(nameof(IndexMax));
            IndexChanged?.Invoke(this, null);
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
            => AppContext.Current.IsPlayingSlideShow ? _windowIconPlay : _windowIconDefault;

        #endregion

        #region Window Title

        // ウィンドウタイトル
        #region Property: WindowTitle
        private string _windowTitle = "";
        public string WindowTitle
        {
            get { return _windowTitle; }
            private set { _windowTitle = value; RaisePropertyChanged(); }
        }
        #endregion

        // ウィンドウタイトル更新
        public void UpdateWindowTitle(UpdateWindowTitleMask mask)
        {
            if (LoadingPath != null)
                WindowTitle = LoosePath.GetFileName(LoadingPath) + " (読込中)";

            else if (BookHub.CurrentBook?.Place == null)
                WindowTitle = _defaultWindowTitle;

            else if (MainContent == null)
                WindowTitle = NVUtility.PlaceToTitle(BookHub.CurrentBook.Place);

            else
                WindowTitle = CreateWindowTitle(mask);
        }

        // ウィンドウタイトル用キーワード置換
        private ReplaceString _windowTitleFormatter = new ReplaceString();

        public const string WindowTitleFormat1Default = "$Book($Page/$PageMax) - $FullName";
        public const string WindowTitleFormat2Default = "$Book($Page/$PageMax) - $FullNameL | $NameR";

        // ウィンドウタイトルフォーマット
        private string _windowTitleFormat1;
        public string WindowTitleFormat1
        {
            get { return _windowTitleFormat1; }
            set { _windowTitleFormat1 = value; _windowTitleFormatter.SetFilter(_windowTitleFormat1 + " " + _windowTitleFormat2); }
        }

        private string _windowTitleFormat2;
        public string WindowTitleFormat2
        {
            get { return _windowTitleFormat2; }
            set { _windowTitleFormat2 = value; _windowTitleFormatter.SetFilter(_windowTitleFormat1 + " " + _windowTitleFormat2); }
        }

        // ウィンドウタイトル作成
        private string CreateWindowTitle(UpdateWindowTitleMask mask)
        {
            string format = Contents[1].IsValid ? WindowTitleFormat2 : WindowTitleFormat1;

            bool isMainContent0 = MainContent == Contents[0];

            if ((mask & UpdateWindowTitleMask.Book) != 0)
            {
                string bookName = NVUtility.PlaceToTitle(BookHub.CurrentBook.Place);
                _windowTitleFormatter.Set("$Book", bookName);
            }

            if ((mask & UpdateWindowTitleMask.Page) != 0)
            {
                string pageNum = (MainContent.Source.PartSize == 2)
                ? (MainContent.Position.Index + 1).ToString()
                : (MainContent.Position.Index + 1).ToString() + (MainContent.Position.Part == 1 ? ".5" : ".0");
                _windowTitleFormatter.Set("$PageMax", (IndexMax + 1).ToString());
                _windowTitleFormatter.Set("$Page", pageNum);

                string path0 = Contents[0].IsValid ? Contents[0].FullPath.Replace("/", " > ").Replace("\\", " > ") + Contents[0].GetPartString() : "";
                string path1 = Contents[1].IsValid ? Contents[1].FullPath.Replace("/", " > ").Replace("\\", " > ") + Contents[1].GetPartString() : "";
                _windowTitleFormatter.Set("$FullName", isMainContent0 ? path0 : path1);
                _windowTitleFormatter.Set("$FullNameL", path1);
                _windowTitleFormatter.Set("$FullNameR", path0);

                string name0 = Contents[0].IsValid ? LoosePath.GetFileName(Contents[0].FullPath) + Contents[0].GetPartString() : "";
                string name1 = Contents[1].IsValid ? LoosePath.GetFileName(Contents[1].FullPath) + Contents[1].GetPartString() : "";
                _windowTitleFormatter.Set("$Name", isMainContent0 ? name0 : name1);
                _windowTitleFormatter.Set("$NameL", name1);
                _windowTitleFormatter.Set("$NameR", name0);

                var bitmapContent0 = Contents[0].Content as BitmapContent;
                var bitmapContent1 = Contents[1].Content as BitmapContent;

                string size0 = bitmapContent0?.BitmapInfo != null ? $"{bitmapContent0.Size.Width}×{bitmapContent0.Size.Height}" : "";
                string size1 = bitmapContent1?.BitmapInfo != null ? $"{bitmapContent1.Size.Width}×{bitmapContent1.Size.Height}" : "";
                _windowTitleFormatter.Set("$Size", isMainContent0 ? size0 : size1);
                _windowTitleFormatter.Set("$SizeL", size1);
                _windowTitleFormatter.Set("$SizeR", size0);

                string bpp0 = bitmapContent0?.BitmapInfo != null ? size0 + "×" + bitmapContent0.BitmapInfo.BitsPerPixel.ToString() : "";
                string bpp1 = bitmapContent1?.BitmapInfo != null ? size1 + "×" + bitmapContent1.BitmapInfo.BitsPerPixel.ToString() : "";
                _windowTitleFormatter.Set("$SizeEx", isMainContent0 ? bpp0 : bpp1);
                _windowTitleFormatter.Set("$SizeExL", bpp1);
                _windowTitleFormatter.Set("$SizeExR", bpp0);
            }

            if ((mask & UpdateWindowTitleMask.View) != 0)
            {
                _windowTitleFormatter.Set("$ViewScale", $"{(int)(_viewScale * 100 + 0.1)}%");
            }

            if ((mask & (UpdateWindowTitleMask.Page | UpdateWindowTitleMask.View)) != 0)
            {
                string scale0 = Contents[0].IsValid ? $"{(int)(_viewScale * Contents[0].Scale * _Dpi.DpiScaleX * 100 + 0.1)}%" : "";
                string scale1 = Contents[1].IsValid ? $"{(int)(_viewScale * Contents[1].Scale * _Dpi.DpiScaleX * 100 + 0.1)}%" : "";
                _windowTitleFormatter.Set("$Scale", isMainContent0 ? scale0 : scale1);
                _windowTitleFormatter.Set("$ScaleL", scale1);
                _windowTitleFormatter.Set("$ScaleR", scale0);
            }

            return _windowTitleFormatter.Replace(format);
        }


        // ロード中パス
        private string _loadingPath;
        public string LoadingPath
        {
            get { return _loadingPath; }
            set { _loadingPath = value; UpdateWindowTitle(UpdateWindowTitleMask.All); }
        }

        #endregion

        // 通知テキスト(標準)
        #region Property: InfoText
        private string _infoText;
        public string InfoText
        {
            get { return _infoText; }
            set { _infoText = value; RaisePropertyChanged(); }
        }

        // 通知テキストフォントサイズ
        public double InfoTextFontSize { get; set; } = 24.0;
        // 通知テキストフォントサイズ
        public double InfoTextMarkSize { get; set; } = 30.0;

        #endregion

        // 通知テキスト(控えめ)
        #region Property: TinyInfoText
        private string _tinyInfoText;
        public string TinyInfoText
        {
            get { return _tinyInfoText; }
            set { _tinyInfoText = value; RaisePropertyChanged(); }
        }
        #endregion

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

        // コンテンツ
        public ObservableCollection<ViewContent> Contents { get; private set; }

        // コンテンツマージン
        #region Property: ContentsMargin
        private Thickness _contentsMargin;
        public Thickness ContentsMargin
        {
            get { return _contentsMargin; }
            set { _contentsMargin = value; RaisePropertyChanged(); }
        }
        #endregion

        //
        #region Property: ContentSpace
        private double _contentSpace = -1.0;
        public double ContentsSpace
        {
            get { return _contentSpace; }
            set { _contentSpace = value; RaisePropertyChanged(); }
        }
        #endregion




        // 見開き時のメインとなるコンテンツ
        #region Property: MainContent
        private ViewContent _mainContent;
        public ViewContent MainContent
        {
            get { return _mainContent; }
            set
            {
                _mainContent = value;
                RaisePropertyChanged();
                UpdateFileInfoContent();
            }
        }
        #endregion

        /// <summary>
        /// FileInfoControlViewModel property.
        /// </summary>
        public FileInfoControlViewModel FileInfoControlViewModel { get; } = new FileInfoControlViewModel();

        //
        private void UpdateFileInfoContent()
        {
            FileInfoControlViewModel.ViewContent = IsVisibleFileInfo ? _mainContent : null; ;
        }

        #region Property: FileInfoSetting
        public FileInfoSetting FileInfoSetting
        {
            get { return FileInfoControlViewModel.Setting; }
            set { FileInfoControlViewModel.Setting = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Property: FolderListSetting
        private FolderListSetting _folderListSetting;
        public FolderListSetting FolderListSetting
        {
            get { return _folderListSetting; }
            set { _folderListSetting = value; RaisePropertyChanged(); }
        }
        #endregion

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

        #region Property: IsPagemark
        public bool IsPagemark
        {
            get { return BookHub.IsMarked(); }
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



        /// <summary>
        /// アプリ動的情報。
        /// MVVM用。
        /// </summary>
        public AppContext AppContext => AppContext.Current;

        // 本管理
        public BookHub BookHub { get; private set; }



        // 標準ウィンドウタイトル
        private string _defaultWindowTitle;

        // ページリスト(表示部用)
        #region Property: PageList
        private ObservableCollection<Page> _pageList;
        public ObservableCollection<Page> PageList
        {
            get { return _pageList; }
            set { _pageList = value; RaisePropertyChanged(); }
        }
        #endregion

        // ページリスト更新
        // TODO: クリアしてもサムネイルのListBoxは項目をキャッシュしてしまうので、なんとかせよ
        // サムネイル用はそれに特化したパーツのみ提供する？
        // いや、ListBoxを独立させ、それ自体を作り直す方向で。
        // 問い合わせがいいな。
        // 問い合わせといえば、BitmapImageでOutOfMemoryが取得できない問題も。
        private void UpdatePageList()
        {
            var pages = BookHub.CurrentBook?.Pages;
            PageList = pages != null ? new ObservableCollection<Page>(pages) : null;

            PageListChanged?.Invoke(this, null);

            RaisePropertyChanged(nameof(IsPagemark));
        }

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
                NotifyMenuVisibilityChanged?.Invoke(this, null);
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
                NotifyMenuVisibilityChanged?.Invoke(this, null);
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
            ContentPosition = MainContent.View.PointToScreen(new Point(0, 0));
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

        // コンストラクタ
        public MainWindowVM(MainWindow window)
        {
            FullScreenManager = new FullScreenManager(window);
            FullScreenManager.Changed += (s, e) => NotifyMenuVisibilityChanged?.Invoke(this, null);

            HistoryFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "History.xml");
            BookmarkFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Bookmark.xml");
            PagemarkFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Pagemark.xml");

            _oldPagemarkFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Pagekmark.xml");

            InitializeWindowIcons();

            // AppContext
            AppContext.Current.IsPlayingSlideShowChanged +=
                (s, e) => RaisePropertyChanged(nameof(WindowIcon));

            // ModelContext
            //ModelContext.Initialize();
            ModelContext.JobEngine.StatusChanged +=
                (s, e) => RaisePropertyChanged(nameof(JobEngine));

            ModelContext.JobEngine.IsBusyChanged +=
                (s, e) => IsBusyJobEngine = ModelContext.JobEngine.IsBusy && !AppContext.Current.IsPlayingSlideShow;

            // BookHub
            BookHub = new BookHub();

            BookHub.Loading +=
                OnLoading;

            BookHub.BookChanged +=
                OnBookChanged;

            BookHub.PageChanged +=
                OnPageChanged;

            BookHub.ViewContentsChanged +=
                OnViewContentsChanged;

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
                    DispMessage(NoticeShowMessageStyle, e);
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
                    RaisePropertyChanged(nameof(IsPagemark));
                };

            BookHub.PagesSorted +=
                (s, e) =>
                {
                    UpdatePageList();
                };


            BookHub.PageRemoved +=
                (s, e) =>
                {
                    UpdatePageList();
                };

            BookHub.PagemarkChanged +=
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(IsPagemark));
                };

            // CommandTable
            ModelContext.CommandTable.SetTarget(this, BookHub);

            // Contents
            Contents = new ObservableCollection<ViewContent>();
            Contents.Add(new ViewContent());
            Contents.Add(new ViewContent());

            // Window title
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = FileVersionInfo.GetVersionInfo(assembly.Location);
            _defaultWindowTitle = $"{assembly.GetName().Name} {ver.FileMajorPart}.{ver.FileMinorPart}";
            if (ver.FileBuildPart > 0) _defaultWindowTitle += $".{ver.FileBuildPart}";
#if DEBUG
            _defaultWindowTitle += " [Debug]";
#endif
            UpdateWindowTitle(UpdateWindowTitleMask.All);

            // messenger
            Messenger.AddReciever("UpdateLastFiles", (s, e) => UpdateLastFiles());

            // ダウンロードフォルダー生成
            if (!System.IO.Directory.Exists(Temporary.TempDownloadDirectory))
            {
                System.IO.Directory.CreateDirectory(Temporary.TempDownloadDirectory);
            }
        }

        // Loading表示状態変更
        public void OnLoading(object sender, string e)
        {
            LoadingPath = e;
            Loading?.Invoke(sender, e);
        }


        // 本が変更された
        private void OnBookChanged(object sender, BookMementoType bookmarkType)
        {
            var title = LoosePath.GetFileName(BookHub.Address);

            App.Current?.Dispatcher.Invoke(() => DispMessage(NoticeShowMessageStyle, title, null, 2.0, bookmarkType));

            UpdatePageList();
            UpdateLastFiles();

            UpdateIndex();

            if (BookHub.Current == null)
            {
                BookUnloaded?.Invoke(this, null);
            }



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
                    return new SolidColorBrush(Contents[Contents[1].IsValid ? 1 : 0].Color);
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
            setting.CommandMememto = ModelContext.CommandTable.CreateMemento();
            setting.DragActionMemento = ModelContext.DragActionTable.CreateMemento();
            setting.ExporterMemento = Exporter.CreateMemento();
            setting.PreferenceMemento = Preference.Current.CreateMemento();
            setting.ImageEffectMemento = this.ImageEffector.CreateMemento();

            return setting;
        }

        // アプリ設定反映
        public void RestoreSetting(Setting setting, bool fromLoad)
        {
            Preference.Current.Restore(setting.PreferenceMemento);
            ModelContext.ApplyPreference();

            this.Restore(setting.ViewMemento);
            this.ImageEffector.Restore(setting.ImageEffectMemento, fromLoad);

            ModelContext.SusieContext.Restore(setting.SusieMemento);
            BookHub.Restore(setting.BookHubMemento);

            ModelContext.CommandTable.Restore(setting.CommandMememto);
            ModelContext.DragActionTable.Restore(setting.DragActionMemento);
            InputGestureChanged?.Invoke(this, null);

            Exporter.Restore(setting.ExporterMemento);
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
                    Messenger.MessageBox(this, "履歴の読み込みに失敗しました。", _defaultWindowTitle, MessageBoxButton.OK, MessageBoxExImage.Warning);
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
                    Messenger.MessageBox(this, "ブックマークの読み込みに失敗しました。", _defaultWindowTitle, MessageBoxButton.OK, MessageBoxExImage.Warning);
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
                    Messenger.MessageBox(this, "ページマークの読み込みに失敗しました。", _defaultWindowTitle, MessageBoxButton.OK, MessageBoxExImage.Warning);
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


        //
        private WindowPlacement.Memento _windowPlacement;

        // ウィンドウ状態を保存
        public void StoreWindowPlacement(MainWindow window)
        {
            // パネル幅保存
            LeftPanelWidth = window.LeftPanel.Width;
            RightPanelWidth = window.RightPanel.Width;

            // ウィンドウ状態保存
            _windowPlacement = WindowPlacement.CreateMemento(window);
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
            setting.WindowPlacement = _windowPlacement;

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
            if (!IsLoadLastFolder) return;

            var list = ModelContext.BookHistory.ListUp(1);
            if (list.Count > 0)
            {
                string place = list[0].Place;
                if (System.IO.Directory.Exists(place) || System.IO.File.Exists(place))
                {
                    Load(place, BookLoadOption.Resume);
                }
            }
        }

        /// <summary>
        /// 表示コンテンツ更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewContentsChanged(object sender, ViewSource e)
        {
            var contents = new List<ViewContent>();

            // ViewContent作成
            if (e?.Sources != null)
            {
                foreach (var source in e.Sources)
                {
                    if (source != null)
                    {
                        var old = Contents[contents.Count];
                        var content = new ViewContent(source, old);
                        contents.Add(content);
                    }
                }
            }

            // ページが存在しない場合、専用メッセージを表示する
            IsVisibleEmptyPageMessage = e != null && contents.Count == 0;

            // メインとなるコンテンツを指定
            MainContent = contents.Count > 0 ? (contents.First().Position < contents.Last().Position ? contents.First() : contents.Last()) : null;

            // ViewModelプロパティに反映
            for (int index = 0; index < 2; ++index)
            {
                Contents[index] = index < contents.Count ? contents[index] : new ViewContent();
            }

            // 背景色更新
            UpdateBackgroundBrush();

            // 自動回転...
            var angle = GetAutoRotateAngle();

            // コンテンツサイズ更新
            UpdateContentSize(angle);

            // 表示更新を通知
            var args = new ViewChangeArgs()
            {
                PageDirection = e != null ? e.Direction : 0,
                ViewOrigin = NextViewOrigin,
                Angle = angle,
            };
            ViewChanged?.Invoke(this, args);
            NextViewOrigin = DragViewOrigin.None;

            UpdateWindowTitle(UpdateWindowTitleMask.All);

            // GC
            MemoryControl.Current.GarbageCollect();
        }

        /// <summary>
        /// ContentAngle property.
        /// </summary>
        private double _contentAngle;
        public double ContentAngle
        {
            get { return _contentAngle; }
            set { if (_contentAngle != value) { _contentAngle = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ページ開始時の回転
        /// </summary>
        /// <returns></returns>
        public double GetAutoRotateAngle()
        {
            var parameter = (AutoRotateCommandParameter)ModelContext.CommandTable[CommandType.ToggleIsAutoRotate].Parameter;

            double angle = this.IsAutoRotateCondition()
                        ? parameter.AutoRotateType == AutoRotateType.Left ? -90.0 : 90.0
                        : 0.0;

            return angle;
        }


        /// <summary>
        /// 次のページ更新時の表示開始位置
        /// TODO: ちゃんとBookから情報として上げるようにするべき
        /// </summary>
        public DragViewOrigin NextViewOrigin { get; set; }




        // ページ番号の更新
        private void OnPageChanged(object sender, int e)
        {
            UpdateIndex();
            RaisePropertyChanged(nameof(IsPagemark));
        }


        // ビューエリアサイズ
        private double _viewWidth;
        private double _viewHeight;

        // ビューエリアサイズを更新
        public void SetViewSize(double width, double height)
        {
            _viewWidth = width;
            _viewHeight = height;

            UpdateContentSize();
        }


        //
        public void UpdateContentSize(double angle)
        {
            this.ContentAngle = angle;
            UpdateContentSize();
        }

        // コンテンツ表示サイズを更新
        public void UpdateContentSize()
        {
            if (!Contents.Any(e => e.IsValid)) return;

            // 2ページ表示時は重なり補正を行う
            double offsetWidth = 0;
            if (Contents[0].Size.Width > 0.5 && Contents[1].Size.Width > 0.5)
            {
                offsetWidth = ContentsSpace / _Dpi.DpiScaleX + ContentsSpace;
                ContentsMargin = new Thickness(offsetWidth, 0, 0, 0);
            }
            else
            {
                ContentsMargin = new Thickness(0);
            }

            var sizes = CalcContentSize(_viewWidth * _Dpi.DpiScaleX + offsetWidth, _viewHeight * _Dpi.DpiScaleY, _contentAngle);

            for (int i = 0; i < 2; ++i)
            {
                Contents[i].Width = sizes[i].Width / _Dpi.DpiScaleX;
                Contents[i].Height = sizes[i].Height / _Dpi.DpiScaleY;
            }

            UpdateContentScalingMode();
        }

        // ビュー回転
        private double _viewAngle;


        // ビュースケール
        private double _viewScale;
        private double _finalViewScale;

        // ビュー反転
        private bool _isViewFlipHorizontal;
        private bool _isViewFlipVertical;



        // ビュー変換を更新
        public void SetViewTransform(TransformEventArgs e)
        {
            _viewAngle = e.Angle;
            _viewScale = e.Scale;
            _finalViewScale = e.Scale * e.LoupeScale;
            _isViewFlipHorizontal = e.IsFlipHorizontal;
            _isViewFlipVertical = e.IsFlipVertical;

            UpdateContentScalingMode();

            // メッセージとして状態表示
            if (ViewTransformShowMessageStyle != ShowMessageStyle.None)
            {
                switch (e.ActionType)
                {
                    case TransformActionType.Scale:
                        string scaleText = IsOriginalScaleShowMessage && MainContent.IsValid
                            ? $"{(int)(_viewScale * MainContent.Scale * _Dpi.DpiScaleX * 100 + 0.1)}%"
                            : $"{(int)(_viewScale * 100.0 + 0.1)}%";
                        DispMessage(ViewTransformShowMessageStyle, scaleText);
                        break;
                    case TransformActionType.Angle:
                        DispMessage(ViewTransformShowMessageStyle, $"{(int)(e.Angle)}°");
                        break;
                    case TransformActionType.FlipHorizontal:
                        DispMessage(ViewTransformShowMessageStyle, "左右反転 " + (_isViewFlipHorizontal ? "ON" : "OFF"));
                        break;
                    case TransformActionType.FlipVertical:
                        DispMessage(ViewTransformShowMessageStyle, "上下反転 " + (_isViewFlipVertical ? "ON" : "OFF"));
                        break;
                    case TransformActionType.LoupeScale:
                        if (e.LoupeScale > 1.5)
                        {
                            DispMessage(ViewTransformShowMessageStyle, $"×{e.LoupeScale:0.0}");
                        }
                        break;
                }
            }

            // スケール変更時はウィンドウタイトルを更新
            if (e.ChangeType == TransformChangeType.Scale)
            {
                UpdateWindowTitle(UpdateWindowTitleMask.View);
            }
        }

        // コンテンツスケーリングモードを更新
        private void UpdateContentScalingMode()
        {
            var dpiScaleX = App.Config.RawDpi.DpiScaleX;
            foreach (var content in Contents)
            {
                if (content.View != null && content.View.Element is Rectangle)
                {
                    double diff = Math.Abs(content.Size.Width - content.Width * dpiScaleX);
                    if (_IsDpiSquare && diff < 0.1 && _viewAngle == 0.0 && Math.Abs(_finalViewScale - 1.0) < 0.001)
                    {
                        content.BitmapScalingMode = BitmapScalingMode.NearestNeighbor;
                    }
                    else
                    {
                        content.BitmapScalingMode = (IsEnabledNearestNeighbor && content.Size.Width < content.Width * dpiScaleX * _finalViewScale) ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality;
                    }
                }
            }
        }

        //
        public bool IsAutoRotateCondition()
        {
            if (!IsAutoRotate) return false;

            var margin = 0.1;
            var viewRatio = GetViewAreaAspectRatio();
            var contentRatio = GetContentAspectRatio();
            return viewRatio >= 1.0 ? contentRatio < (1.0 - margin) : contentRatio > (1.0 + margin);
        }

        //
        public double GetViewAreaAspectRatio()
        {
            return _viewWidth / _viewHeight;
        }

        //
        public double GetContentAspectRatio()
        {
            var size = GetContentSize();
            return size.Width / size.Height;
        }

        //
        private Size GetContentSize()
        {
            var c0 = Contents[0].Size;
            var c1 = Contents[1].Size;

            double rate0 = 1.0;
            double rate1 = 1.0;

            // 2ページ合わせたコンテンツサイズを求める
            if (!Contents[1].IsValid)
            {
                return c0;
            }
            // オリジナルサイズ
            else if (this.StretchMode == PageStretchMode.None)
            {
                return new Size(c0.Width + c1.Width, Math.Max(c0.Height, c1.Height));
            }
            else
            {
                // どちらもImageでない
                if (c0.Width < 0.1 && c1.Width < 0.1)
                {
                    return new Size(1.0, 1.0);
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
                return new Size(c0.Width * rate0 + c1.Width * rate1, c0.Height * rate0);
            }
        }


        // ストレッチモードに合わせて各コンテンツのスケールを計算する
        private Size[] CalcContentSize(double width, double height, double angle)
        {
            var c0 = Contents[0].Size;
            var c1 = Contents[1].Size;

            // オリジナルサイズ
            if (this.StretchMode == PageStretchMode.None)
            {
                return new Size[] { c0, c1 };
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
                    return new Size[] { c0, c1 };
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

            // 回転反映
            {
                //var angle = 45.0;
                var rect = new Rect(content);
                var m = new Matrix();
                m.Rotate(angle);
                rect.Transform(m);

                content = new Size(rect.Width, rect.Height);
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

            // 面積をあわせる
            if (this.StretchMode == PageStretchMode.UniformToSize)
            {
                var viewSize = width * height;
                var contentSize = content.Width * content.Height;
                var rate = Math.Sqrt(viewSize / contentSize);
                rate0 *= rate;
                rate1 *= rate;
            }
            // 高さを合わせる
            else if (this.StretchMode == PageStretchMode.UniformToVertical)
            {
                rate0 *= rateH;
                rate1 *= rateH;
            }
            // 枠いっぱいに広げる
            else if (this.StretchMode == PageStretchMode.UniformToFill)
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

            var s0 = new Size(c0.Width * rate0, c0.Height * rate0);
            var s1 = new Size(c1.Width * rate1, c1.Height * rate1);
            return new Size[] { s0, s1 };
        }


        /// <summary>
        /// メッセージ表示
        /// </summary>
        /// <param name="style">メッセージスタイル</param>
        /// <param name="message">メッセージ</param>
        public void DispMessage(ShowMessageStyle style, string message, string messageTiny = null, double dispTime = MessageShowParams.DefaultDispTime, BookMementoType bookmarkType = BookMementoType.None)
        {
            switch (style)
            {
                case ShowMessageStyle.Normal:
                    Messenger.Send(this, new MessageEventArgs("MessageShow")
                    {
                        Parameter = new MessageShowParams(message)
                        {
                            BookmarkType = bookmarkType,
                            DispTime = dispTime
                        }
                    });
                    break;
                case ShowMessageStyle.Tiny:
                    TinyInfoText = messageTiny ?? message;
                    break;
            }
        }


        // コマンド実行 
        public void Execute(CommandType type, object sender, object param)
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
            ModelContext.CommandTable[type].Execute(sender, param);
        }


        // ジェスチャー表示
        public void ShowGesture(string gesture, string commandName)
        {
            if (string.IsNullOrEmpty(gesture) && string.IsNullOrEmpty(commandName)) return;

            DispMessage(
                GestureShowMessageStyle,
                ((commandName != null) ? commandName + "\n" : "") + gesture,
                gesture + ((commandName != null) ? " " + commandName : ""));
        }




        // スライドショーの表示間隔
        public double SlideShowInterval => BookHub.SlideShowInterval;

        // カーソルでスライドを止める
        public bool IsCancelSlideByMouseMove => BookHub.IsCancelSlideByMouseMove;

        // スライドショー：次のスライドへ
        public void NextSlide()
        {
            BookHub.NextSlide();
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
            if (PageList == null || ThumbnailSize < 8.0) return;

            // サムネイルリストが無効の場合、処理しない
            if (!IsEnableThumbnailList) return;

            // 未処理の要求を解除
            ModelContext.JobEngine.Clear(QueueElementPriority.PageThumbnail);

            // 要求. 中央値優先
            int center = start + count / 2;
            var pages = Enumerable.Range(start - margin, count + margin * 2 - 1)
                .Where(i => i >= 0 && i < PageList.Count)
                .Select(e => PageList[e])
                .OrderBy(e => Math.Abs(e.Index - center));

            foreach (var page in pages)
            {
                page.LoadThumbnail(QueueElementPriority.PageThumbnail);
            }
        }


        //
        private BitmapSource CurrentBitmapSource
        {
            get { return (this.MainContent?.Content as BitmapContent)?.BitmapSource; }
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
                Messenger.MessageBox(this, $"コピーに失敗しました\n\n原因: {e.Message}", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
            }
        }


        /// <summary>
        /// 印刷可能判定
        /// </summary>
        /// <returns></returns>
        public bool CanPrint()
        {
            return MainContent != null && MainContent.IsValid;
        }

        /// <summary>
        /// 印刷
        /// </summary>
        public void Print(Window owner, FrameworkElement element, Transform transform, double width, double height)
        {
            if (!CanPrint()) return;

            // 掃除しておく
            GC.Collect();

            // スケールモード退避
            var scaleModeMemory = Contents.ToDictionary(e => e, e => e.BitmapScalingMode);

            // アニメーション停止
            foreach (var content in Contents)
            {
                content.AnimationImageVisibility = Visibility.Visible;
                content.AnimationPlayerVisibility = Visibility.Collapsed;
            }

            // スライドショー停止
            AppContext.Current.PauseSlideShow();

            try
            {
                var context = new PrintContext();
                context.MainContent = this.MainContent;
                context.Contents = this.Contents;
                context.View = element;
                context.ViewTransform = transform;
                context.ViewWidth = width;
                context.ViewHeight = height;
                context.ViewEffect = ImageEffector.Effect;
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
                foreach (var content in Contents)
                {
                    content.BitmapScalingMode = scaleModeMemory[content];
                    content.AnimationImageVisibility = Visibility.Collapsed;
                    content.AnimationPlayerVisibility = Visibility.Visible;
                }

                // スライドショー再開
                AppContext.Current.ResumeSlideShow();
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

            [DataMember]
            public bool IsLimitMove { get; set; }

            [DataMember]
            public bool IsControlCenterImage { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public bool IsAngleSnap { get; set; } // no used

            [DataMember(Order = 19)]
            public double AngleFrequency { get; set; }

            [DataMember]
            public bool IsViewStartPositionCenter { get; set; }

            [DataMember]
            public PageStretchMode StretchMode { get; set; }

            [DataMember]
            public BackgroundStyle Background { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public bool IsSliderDirectionReversed { get; set; } // no used

            [DataMember(Order = 4)]
            public ShowMessageStyle NoticeShowMessageStyle { get; set; }

            [DataMember]
            public ShowMessageStyle CommandShowMessageStyle { get; set; }

            [DataMember]
            public ShowMessageStyle GestureShowMessageStyle { get; set; }

            [DataMember(Order = 4)]
            public ShowMessageStyle NowLoadingShowMessageStyle { get; set; }

            [DataMember(Order = 1)]
            public bool IsEnabledNearestNeighbor { get; set; }

            [DataMember(Order = 2)]
            public bool IsKeepScale { get; set; }

            [DataMember(Order = 2)]
            public bool IsKeepAngle { get; set; }

            [DataMember(Order = 4)]
            public bool IsKeepFlip { get; set; }

            [DataMember(Order = 2)]
            public bool IsLoadLastFolder { get; set; }

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

            [DataMember(Order = 8)]
            public bool IsVisibleTitleBar { get; set; }

            [DataMember(Order = 4)]
            public bool IsFullScreen { get; set; }

            [DataMember(Order = 4)]
            public bool IsSaveFullScreen { get; set; }

            [DataMember(Order = 4)]
            public bool IsTopmost { get; set; }

            [DataMember(Order = 5)]
            public FileInfoSetting FileInfoSetting { get; set; }

            [DataMember(Order = 5)]
            public string UserDownloadPath { get; set; }

            [DataMember(Order = 6)]
            public FolderListSetting FolderListSetting { get; set; }

            [DataMember(Order = 6)]
            public PanelColor PanelColor { get; set; }

            [DataMember(Order = 7)]
            public PanelType LeftPanel { get; set; }

            [DataMember(Order = 7)]
            public PanelType RightPanel { get; set; }

            [DataMember(Order = 7)]
            public double LeftPanelWidth { get; set; }

            [DataMember(Order = 7)]
            public double RightPanelWidth { get; set; }

            private string _windowTitleFormat1;
            [DataMember(Order = 7)]
            public string WindowTitleFormat1
            {
                get { return _windowTitleFormat1; }
                set { _windowTitleFormat1 = string.IsNullOrEmpty(value) ? MainWindowVM.WindowTitleFormat1Default : value; }
            }

            private string _windowTitleFormat2;
            [DataMember(Order = 7)]
            public string WindowTitleFormat2
            {
                get { return _windowTitleFormat2; }
                set { _windowTitleFormat2 = string.IsNullOrEmpty(value) ? MainWindowVM.WindowTitleFormat2Default : value; }
            }

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

            [DataMember(Order = 10)]
            public string FolderListGridRow0 { get; set; }

            [DataMember(Order = 10)]
            public string FolderListGridRow2 { get; set; }

            [DataMember(Order = 10)]
            public bool IsVisiblePageList { get; set; }

            [DataMember(Order = 10)]
            public FolderListItemStyle FolderListItemStyle { get; set; }

            [DataMember(Order = 10)]
            public ShowMessageStyle ViewTransformShowMessageStyle { get; set; }

            [DataMember(Order = 10)]
            public bool IsOriginalScaleShowMessage { get; set; }

            [DataMember(Order = 12)]
            public double ContentsSpace { get; set; }

            [DataMember(Order = 12)]
            public LongButtonDownMode LongLeftButtonDownMode { get; set; }

            [DataMember(Order = 12)]
            public double LongButtonDownTick { get; set; }

            [DataMember(Order = 16)]
            public SliderDirection SliderDirection { get; set; }

            [DataMember(Order = 17)]
            public bool IsHidePageSlider { get; set; }

            [DataMember(Order = 18)]
            public bool IsAutoRotate { get; set; }

            [DataMember(Order = 19)]
            public bool IsVisibleWindowTitle { get; set; }

            [DataMember(Order = 19)]
            public bool IsVisibleLoupeInfo { get; set; }

            [DataMember(Order = 20, EmitDefaultValue = false)]
            public bool IsSliderWithIndex { get; set; } // no used

            [DataMember(Order = 20)]
            public bool IsLoupeCenter { get; set; }

            [DataMember(Order = 20)]
            public FolderListItemStyle PageListItemStyle { get; set; }

            [DataMember(Order = 21)]
            public SliderIndexLayout SliderIndexLayout { get; set; }

            [DataMember(Order = 21)]
            public BrushSource CustomBackground { get; set; }

            //
            private void Constructor()
            {
                IsLimitMove = true;
                NoticeShowMessageStyle = ShowMessageStyle.Normal;
                CommandShowMessageStyle = ShowMessageStyle.Normal;
                GestureShowMessageStyle = ShowMessageStyle.Normal;
                NowLoadingShowMessageStyle = ShowMessageStyle.Normal;
                ViewTransformShowMessageStyle = ShowMessageStyle.None;
                StretchMode = PageStretchMode.Uniform;
                Background = BackgroundStyle.Black;
                FileInfoSetting = new FileInfoSetting();
                FolderListSetting = new FolderListSetting();
                PanelColor = PanelColor.Dark;
                LeftPanelWidth = 250;
                RightPanelWidth = 250;
                WindowTitleFormat1 = MainWindowVM.WindowTitleFormat1Default;
                WindowTitleFormat2 = MainWindowVM.WindowTitleFormat2Default;
                IsSaveWindowPlacement = true;
                IsHidePanelInFullscreen = true;
                IsVisibleTitleBar = true;
                ContextMenuSetting = new ContextMenuSetting();
                IsHideThumbnailList = true;
                ThumbnailSize = 96;
                IsSliderLinkedThumbnailList = true;
                IsAutoGC = true;
                IsVisibleThumbnailPlate = true;
                FolderListGridRow0 = "*";
                FolderListGridRow2 = "*";
                ContentsSpace = -1.0;
                LongLeftButtonDownMode = LongButtonDownMode.Loupe;
                LongButtonDownTick = 1.0;
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
                if (_Version < Config.GenerateProductVersionNumber(1, 10, 0))
                {
                    IsVisibleTitleBar = !IsHideTitleBar;
                }
                IsHideTitleBar = false;

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
            memento.IsLimitMove = this.IsLimitMove;
            memento.IsControlCenterImage = this.IsControlCenterImage;
            memento.AngleFrequency = this.AngleFrequency;
            memento.IsViewStartPositionCenter = this.IsViewStartPositionCenter;
            memento.StretchMode = this.StretchMode;
            memento.CustomBackground = this.CustomBackground;
            memento.Background = this.Background;
            memento.NoticeShowMessageStyle = this.NoticeShowMessageStyle;
            memento.CommandShowMessageStyle = this.CommandShowMessageStyle;
            memento.GestureShowMessageStyle = this.GestureShowMessageStyle;
            memento.NowLoadingShowMessageStyle = this.NowLoadingShowMessageStyle;
            memento.ViewTransformShowMessageStyle = this.ViewTransformShowMessageStyle;
            memento.IsEnabledNearestNeighbor = this.IsEnabledNearestNeighbor;
            memento.IsKeepScale = this.IsKeepScale;
            memento.IsKeepAngle = this.IsKeepAngle;
            memento.IsKeepFlip = this.IsKeepFlip;
            memento.IsLoadLastFolder = this.IsLoadLastFolder;
            memento.IsDisableMultiBoot = this.IsDisableMultiBoot;
            memento.IsAutoPlaySlideShow = this.IsAutoPlaySlideShow;
            memento.IsSaveWindowPlacement = this.IsSaveWindowPlacement;
            memento.IsHideMenu = this.IsHideMenu;
            memento.IsHidePageSlider = this.IsHidePageSlider;
            memento.IsVisibleTitleBar = this.IsVisibleTitleBar;
            memento.IsFullScreen = this.IsFullScreen;
            memento.IsSaveFullScreen = this.IsSaveFullScreen;
            memento.IsTopmost = this.IsTopmost;
            memento.FileInfoSetting = this.FileInfoSetting.Clone();
            memento.UserDownloadPath = this.UserDownloadPath;
            memento.FolderListSetting = this.FolderListSetting.Clone();
            memento.PanelColor = this.PanelColor;
            memento.LeftPanel = this.LeftPanel;
            memento.RightPanel = this.RightPanel;
            memento.LeftPanelWidth = this.LeftPanelWidth;
            memento.RightPanelWidth = this.RightPanelWidth;
            memento.WindowTitleFormat1 = this.WindowTitleFormat1;
            memento.WindowTitleFormat2 = this.WindowTitleFormat2;
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
            memento.FolderListGridRow0 = this.FolderListGridRow0;
            memento.FolderListGridRow2 = this.FolderListGridRow2;
            memento.IsVisiblePageList = this.IsVisiblePageList;
            memento.FolderListItemStyle = this.FolderListItemStyle;
            memento.IsOriginalScaleShowMessage = this.IsOriginalScaleShowMessage;
            memento.ContentsSpace = this.ContentsSpace;
            memento.LongLeftButtonDownMode = this.LongLeftButtonDownMode;
            memento.LongButtonDownTick = this.LongButtonDownTick;
            memento.SliderDirection = this.SliderDirection;
            memento.IsAutoRotate = this.IsAutoRotate;
            memento.IsVisibleWindowTitle = this.IsVisibleWindowTitle;
            memento.IsVisibleLoupeInfo = this.IsVisibleLoupeInfo;
            memento.IsLoupeCenter = this.IsLoupeCenter;
            memento.PageListItemStyle = this.PageListItemStyle;
            memento.SliderIndexLayout = this.SliderIndexLayout;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            this.IsLimitMove = memento.IsLimitMove;
            this.IsControlCenterImage = memento.IsControlCenterImage;
            this.AngleFrequency = memento.AngleFrequency;
            this.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
            this.StretchMode = memento.StretchMode;
            this.CustomBackground = memento.CustomBackground;
            this.Background = memento.Background;
            this.NoticeShowMessageStyle = memento.NoticeShowMessageStyle;
            this.CommandShowMessageStyle = memento.CommandShowMessageStyle;
            this.GestureShowMessageStyle = memento.GestureShowMessageStyle;
            this.NowLoadingShowMessageStyle = memento.NowLoadingShowMessageStyle;
            this.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;
            this.IsEnabledNearestNeighbor = memento.IsEnabledNearestNeighbor;
            this.IsKeepScale = memento.IsKeepScale;
            this.IsKeepAngle = memento.IsKeepAngle;
            this.IsKeepFlip = memento.IsKeepFlip;
            this.IsLoadLastFolder = memento.IsLoadLastFolder;
            this.IsDisableMultiBoot = memento.IsDisableMultiBoot;
            this.IsAutoPlaySlideShow = memento.IsAutoPlaySlideShow;
            this.IsSaveWindowPlacement = memento.IsSaveWindowPlacement;
            this.IsHideMenu = memento.IsHideMenu;
            this.IsHidePageSlider = memento.IsHidePageSlider;
            this.IsVisibleTitleBar = memento.IsVisibleTitleBar;
            this.IsSaveFullScreen = memento.IsSaveFullScreen;
            if (this.IsSaveFullScreen) this.IsFullScreen = memento.IsFullScreen;
            this.IsTopmost = memento.IsTopmost;
            this.FileInfoSetting = memento.FileInfoSetting.Clone();
            this.UserDownloadPath = memento.UserDownloadPath;
            this.FolderListSetting = memento.FolderListSetting.Clone();
            this.PanelColor = memento.PanelColor;
            this.IsHidePanel = memento.IsHidePanel;
            this.LeftPanel = memento.LeftPanel;
            this.RightPanel = memento.RightPanel;
            this.LeftPanelWidth = memento.LeftPanelWidth;
            this.RightPanelWidth = memento.RightPanelWidth;
            this.WindowTitleFormat1 = memento.WindowTitleFormat1;
            this.WindowTitleFormat2 = memento.WindowTitleFormat2;
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
            this.FolderListGridRow0 = memento.FolderListGridRow0;
            this.FolderListGridRow2 = memento.FolderListGridRow2;
            this.IsVisiblePageList = memento.IsVisiblePageList;
            this.FolderListItemStyle = memento.FolderListItemStyle;
            this.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
            this.ContentsSpace = memento.ContentsSpace;
            this.LongLeftButtonDownMode = memento.LongLeftButtonDownMode;
            this.LongButtonDownTick = memento.LongButtonDownTick;
            this.SliderDirection = memento.SliderDirection;
            this.IsAutoRotate = memento.IsAutoRotate;
            this.IsVisibleWindowTitle = memento.IsVisibleWindowTitle;
            this.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
            this.IsLoupeCenter = memento.IsLoupeCenter;
            this.PageListItemStyle = memento.PageListItemStyle;
            this.SliderIndexLayout = memento.SliderIndexLayout;


            NotifyMenuVisibilityChanged?.Invoke(this, null);
            ViewChanged?.Invoke(this, new ViewChangeArgs() { ResetViewTransform = true });
            UpdateContentSize();
        }

        #endregion
    }
}
