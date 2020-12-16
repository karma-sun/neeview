using NeeLaboratory.ComponentModel;
using NeeView.Runtime.LayoutPanel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;

namespace NeeView
{
    public class PanelsConfig : BindableBase
    {
        private bool _isHidePanel;
        private bool _isSideBarEnabled = true;
        private double _opacity = 1.0;
        private bool _isHidePanelInAutoHideMode = true;
        private string _fontName = SystemFonts.MessageFontFamily.Source;
        private double _fontSize = 15.0;
        private double _folderTreeFontSize = 12.0;
        private bool _isDecoratePlace = true;
        private bool _openWithDoubleClick;
        private bool _isLeftRightKeyEnabled;
        private bool _isManipulationBoundaryFeedbackEnabled;
        private double _mouseWheelSpeedRate = 1.0;
        private double _leftPanelWidth = 300.0;
        private double _rightPanelWidth = 300.0;

        /// <summary>
        /// パネルを自動的に隠す
        /// </summary>
        [PropertyMember("@ParamPanelsIsAutoHide")]
        public bool IsHidePanel
        {
            get { return _isHidePanel; }
            set { SetProperty(ref _isHidePanel, value); }
        }

        /// <summary>
        /// パネルを自動的に隠す(自動非表示モード)
        /// </summary>
        [PropertyMember("@PanelsConfig.IsHidePanelInAutoHideMode")]
        public bool IsHidePanelInAutoHideMode
        {
            get { return _isHidePanelInAutoHideMode; }
            set { SetProperty(ref _isHidePanelInAutoHideMode, value); }
        }

        /// <summary>
        /// サイドバー表示フラグ 
        /// </summary>
        [PropertyMember("@ParamIsSideBarEnabled")]
        public bool IsSideBarEnabled
        {
            get { return _isSideBarEnabled; }
            set { SetProperty(ref _isSideBarEnabled, value); }
        }

        /// <summary>
        /// パネルの透明度
        /// </summary>
        [PropertyPercent("@ParamSidePanelOpacity", Tips = "@ParamSidePanelOpacityTips")]
        public double Opacity
        {
            get { return _opacity; }
            set { SetProperty(ref _opacity, value); }
        }

        /// <summary>
        /// ダブルクリックでブックを開く
        /// </summary>
        [PropertyMember("@ParamSidePanelOpenWithDoubleClick", Tips = "@ParamSidePanelOpenWithDoubleClickTips")]
        public bool OpenWithDoubleClick
        {
            get { return _openWithDoubleClick; }
            set { SetProperty(ref _openWithDoubleClick, value); }
        }

        /// <summary>
        /// パネルでの左右キー操作有効
        /// </summary>
        [PropertyMember("@ParamSidePanelIsLeftRightKeyEnabled", Tips = "@ParamSidePanelIsLeftRightKeyEnabledTips")]
        public bool IsLeftRightKeyEnabled
        {
            get { return _isLeftRightKeyEnabled; }
            set { SetProperty(ref _isLeftRightKeyEnabled, value); }
        }

        /// <summary>
        /// タッチパ操作でのリストバウンド効果
        /// </summary>
        [PropertyMember("@ParamSidePanelIsManipulationBoundaryFeedbackEnabled")]
        public bool IsManipulationBoundaryFeedbackEnabled
        {
            get { return _isManipulationBoundaryFeedbackEnabled; }
            set { SetProperty(ref _isManipulationBoundaryFeedbackEnabled, value); }
        }

        /// <summary>
        /// フォント名
        /// </summary>
        [PropertyMember("@ParamListItemFontName")]
        public string FontName
        {
            get { return _fontName; }
            set { SetProperty(ref _fontName, string.IsNullOrWhiteSpace(value) ? SystemFonts.MessageFontFamily.Source : value); }
        }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        [PropertyRange("@ParamListItemFontSize", 8, 24, TickFrequency = 0.5, IsEditable = true)]
        public double FontSize
        {
            get { return _fontSize; }
            set { SetProperty(ref _fontSize, Math.Max(1.0, value)); }
        }

        /// <summary>
        /// フォルダーツリーのフォントサイズ
        /// </summary>
        [PropertyRange("@ParamListItemFolderTreeFontSize", 8, 24, TickFrequency = 0.5, IsEditable = true)]
        public double FolderTreeFontSize
        {
            get { return _folderTreeFontSize; }
            set { SetProperty(ref _folderTreeFontSize, Math.Max(1.0, value)); }
        }

        /// <summary>
        /// パス表示形式を "CCC (C:\AAA\BBB) にする
        /// </summary>
        [PropertyMember("@ParamListItemContentIsDecoratePlace", Tips = "@ParamListItemContentIsDecoratePlaceTips")]
        public bool IsDecoratePlace
        {
            get { return _isDecoratePlace; }
            set { SetProperty(ref _isDecoratePlace, value); }
        }

        /// <summary>
        /// サムネイルリストのマウスホイール速度倍率
        /// </summary>
        [PropertyRange("@ParamMouseWheelSpeedRate", 0.1, 2.0, TickFrequency =0.1)]
        public double MouseWheelSpeedRate
        {
            get { return _mouseWheelSpeedRate; }
            set { SetProperty(ref _mouseWheelSpeedRate, Math.Max(value, 0.1)); }
        }


        [PropertyMapLabel("@WordStyleContent")]
        public PanelListItemProfile ContentItemProfile { get; set; } = PanelListItemProfile.DefaultContentItemProfile.Clone();

        [PropertyMapLabel("@WordStyleBanner")]
        public PanelListItemProfile BannerItemProfile { get; set; } = PanelListItemProfile.DefaultBannerItemProfile.Clone();

        [PropertyMapLabel("@WordStyleThumbnail")]
        public PanelListItemProfile ThumbnailItemProfile { get; set; } = PanelListItemProfile.DefaultThumbnailItemProfile.Clone();


        #region HiddenParameters

        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public double LeftPanelWidth
        {
            get { return _leftPanelWidth; }
            set { SetProperty(ref _leftPanelWidth, value); }
        }

        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public double RightPanelWidth
        {
            get { return _rightPanelWidth; }
            set { SetProperty(ref _rightPanelWidth, value); }
        }

        // ver 38
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public LayoutPanelManager.Memento Layout { get; set; }

        #endregion HiddenParameters


        #region Obsolete

        [Obsolete] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool IsHidePanelInFullscreen
        {
            get { return default; }
            set { IsHidePanelInAutoHideMode = value; }
        }

        [Obsolete] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Dictionary<string, PanelDock> PanelDocks { get; set; }

        [Obsolete] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string LeftPanelSeleted { get; set; }

        [Obsolete] // ver.38
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string RightPanelSeleted { get; set; }

        #endregion Obsolete

    }

    public enum PanelDock
    {
        Left,
        Right
    }
}