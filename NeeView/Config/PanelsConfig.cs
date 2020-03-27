using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Windows;

namespace NeeView
{
    public class PanelsConfig : BindableBase
    {
        private bool _isHidePanel;
        private bool _isSideBarEnabled = true;
        private double _opacity = 1.0;
        private bool _isHidePanelInFullscreen = true;
        private string _fontName = SystemFonts.MessageFontFamily.Source;
        private double _fontSize = 15.0;
        private double _folderTreeFontSize = 12.0;
        private bool _isDecoratePlace = true;



        // パネルを自動的に隠す
        public bool IsHidePanel
        {
            get { return _isHidePanel; }
            set { SetProperty(ref _isHidePanel, value); }
        }


        /// <summary>
        /// サイドバー表示フラグ 
        /// </summary>
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
        /// フルスクリーン時にパネルを隠す
        /// </summary>
        [PropertyMember("@ParamIsHidePanelInFullscreen")]
        public bool IsHidePanelInFullscreen
        {
            get { return _isHidePanelInFullscreen; }
            set { SetProperty(ref _isHidePanelInFullscreen, value); }
        }

        /// <summary>
        /// パネルでの左右キー操作有効
        /// </summary>
        [PropertyMember("@ParamSidePanelIsLeftRightKeyEnabled", Tips = "@ParamSidePanelIsLeftRightKeyEnabledTips")]
        public bool IsLeftRightKeyEnabled { get; set; } = true;

        /// <summary>
        /// タッチパ操作でのリストバウンド効果
        /// </summary>
        [PropertyMember("@ParamSidePanelIsManipulationBoundaryFeedbackEnabled")]
        public bool IsManipulationBoundaryFeedbackEnabled { get; set; }


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


        public PanelListItemProfile ContentItemProfile { get; set; } = PanelListItemProfile.DefaultContentItemProfile.Clone();

        public PanelListItemProfile BannerItemProfile { get; set; } = PanelListItemProfile.DefaultBannerItemProfile.Clone();

        public PanelListItemProfile ThumbnailItemProfile { get; set; } = PanelListItemProfile.DefaultThumbnailItemProfile.Clone();

        /*
        [PropertyMapIgnore]
        public SidePanelConfig LeftPanel { get; set; } = new SidePanelConfig();

        [PropertyMapIgnore]
        public SidePanelConfig RightPanel { get; set; } = new SidePanelConfig();
        */

        private Dictionary<string, PanelDock> _panelDocks = new Dictionary<string, PanelDock>();

        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public Dictionary<string, PanelDock> PanelDocks
        {
            get { return _panelDocks; }
            set { SetProperty(ref _panelDocks, value ?? new Dictionary<string, PanelDock>()); }
        } 

        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public string LeftPanelSeleted { get; set; }

        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public double LeftPanelWidth { get; set; } = 300.0;

        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public string RightPanelSeleted { get; set; }

        [PropertyMapIgnore]
        [ObjectMergeIgnore]
        public double RightPanelWidth { get; set; } = 300.0;

    }

    public enum PanelDock
    {
        Left,
        Right
    }

    /*
    public class SidePanelConfig : BindableBase
    {
        [PropertyMergeReferenceCopy]
        public List<string> PanelTypeCodes { get; set; }

        public string SelectedPanelTypeCode { get; set; }

        public double Width { get; set; } = 300.0;
    }
    */
}