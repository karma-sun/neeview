using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class SidePanelProfile : BindableBase
    {
        static SidePanelProfile()
        {
            /*
            _defaultNormalItemProfile = new PanelListItemProfile(PanelListItemImageShape.Square, 0, false, true, false, 0.0);
            _defaultContentItemProfile = new PanelListItemProfile(PanelListItemImageShape.Square, 64, true, true, false, 0.5);
            _defaultBannerItemProfile = new PanelListItemProfile(PanelListItemImageShape.Banner, 200, false, true, false, 0.0);
            _defaultThumbnailItemProfile = new PanelListItemProfile(PanelListItemImageShape.Original, 128, false, true, true, 0.0);
            */
            Current = new SidePanelProfile();
        }

        public static SidePanelProfile Current { get; }

        /*
        private static PanelListItemProfile _defaultNormalItemProfile;
        private static PanelListItemProfile _defaultContentItemProfile;
        private static PanelListItemProfile _defaultBannerItemProfile;
        private static PanelListItemProfile _defaultThumbnailItemProfile;
        */

        ////private double _opacity = 1.0;
        private SolidColorBrush _backgroundBrush;
        private SolidColorBrush _baseBrush;
        private SolidColorBrush _iconBackgroundBrush;
        ////private string _fontName = SystemFonts.MessageFontFamily.Source;
        ////private double _folderTreeFontSize = 12;
        ////private double _fontSize = 15.0;
        ////private bool _isDecoratePlace = true;

        private SidePanelProfile()
        {
            ////_normalItemProfile = _defaultNormalItemProfile.Clone();
            ////Config.Current.Layout.Panels.ContentItemProfile = _defaultContentItemProfile.Clone();
            ////Config.Current.Layout.Panels.BannerItemProfile = _defaultBannerItemProfile.Clone();
            ////Config.Current.Layout.Panels.ThumbnailItemProfile = _defaultThumbnailItemProfile.Clone();

            SetFontFamilyResource(Config.Current.Layout.Panels.FontName);
            SetFontSizeResource(Config.Current.Layout.Panels.FontSize);
            SetFolderTreeFontSizeResource(Config.Current.Layout.Panels.FolderTreeFontSize);

            ThemeProfile.Current.ThemeColorChanged += (s, e) => RefreshBrushes();
            MainWindowModel.Current.CanHidePanelChanged += (s, e) => RefreshBrushes();

            Config.Current.Layout.Panels.AddPropertyChanged(nameof(PanelsConfig.Opacity), (s, e) =>
            {
                RefreshBrushes();
            });

            Config.Current.Layout.Panels.AddPropertyChanged(nameof(PanelsConfig.FontName), (s, e) =>
            {
                try
                {
                    SetFontFamilyResource(Config.Current.Layout.Panels.FontName);
                    ValidatePanelListItemProfile();
                }
                catch
                {
                    // nop.
                }
            });

            Config.Current.Layout.Panels.AddPropertyChanged(nameof(PanelsConfig.FontSize), (s, e) =>
            {
                SetFontSizeResource(Config.Current.Layout.Panels.FontSize);
                ValidatePanelListItemProfile();
            });

            Config.Current.Layout.Panels.AddPropertyChanged(nameof(PanelsConfig.FolderTreeFontSize), (s, e) =>
            {
                SetFolderTreeFontSizeResource(Config.Current.Layout.Panels.FolderTreeFontSize);
            });

            ValidatePanelListItemProfile();
            RefreshBrushes();
        }


#if false
        [PropertyMember("@ParamSidePanelIsLeftRightKeyEnabled", Tips = "@ParamSidePanelIsLeftRightKeyEnabledTips")]
        public bool IsLeftRightKeyEnabled { get; set; } = true;

        [PropertyPercent("@ParamSidePanelOpacity", Tips = "@ParamSidePanelOpacityTips")]
        public double Opacity
        {
            get { return _opacity; }
            set
            {
                if (SetProperty(ref _opacity, value))
                {
                    RefreshBrushes();
                }
            }
        }
#endif

        public SolidColorBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { SetProperty(ref _backgroundBrush, value); }
        }

        public SolidColorBrush BaseBrush
        {
            get { return _baseBrush; }
            set { SetProperty(ref _baseBrush, value); }
        }

        public SolidColorBrush IconBackgroundBrush
        {
            get { return _iconBackgroundBrush; }
            set { SetProperty(ref _iconBackgroundBrush, value); }
        }



#if false
        [PropertyMember("@ParamListItemFontName")]
        public string FontName
        {
            get
            {
                return _fontName;
            }
            set
            {
                value = value ?? SystemFonts.MessageFontFamily.Source;
                if (_fontName != value)
                {
                    try
                    {
                        _fontName = value;
                        SetFontFamilyResource(_fontName);
                        RaisePropertyChanged();

                        Config.Current.Layout.Panels.ContentItemProfile.UpdateTextHeight();
                        Config.Current.Layout.Panels.BannerItemProfile.UpdateTextHeight();
                        Config.Current.Layout.Panels.ThumbnailItemProfile.UpdateTextHeight();
                    }
                    catch
                    {
                        // nop.
                    }
                }
            }
        }


        [PropertyRange("@ParamListItemFontSize", 8, 24, TickFrequency = 0.5, IsEditable = true)]
        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                value = Math.Max(1, value);
                if (_fontSize != value)
                {
                    _fontSize = value;
                    SetFontSizeResource(_fontSize);
                    RaisePropertyChanged();

                    Config.Current.Layout.Panels.ContentItemProfile.UpdateTextHeight();
                    Config.Current.Layout.Panels.BannerItemProfile.UpdateTextHeight();
                    Config.Current.Layout.Panels.ThumbnailItemProfile.UpdateTextHeight();
                }
            }
        }
        [PropertyRange("@ParamListItemFolderTreeFontSize", 8, 24, TickFrequency = 0.5, IsEditable = true)]
        public double FolderTreeFontSize
        {
            get { return _folderTreeFontSize; }
            set
            {
                value = Math.Max(1, value);
                if (_folderTreeFontSize != value)
                {
                    _folderTreeFontSize = value;
                    SetFolderTreeFontSizeResource(_folderTreeFontSize);
                    RaisePropertyChanged();
                }
            }
        }

        private PanelListItemProfile _normalItemProfile;
        public PanelListItemProfile NormalItemProfile
        {
            get { return _normalItemProfile; }
            set { SetProperty(ref _normalItemProfile, value); }
        }


        private PanelListItemProfile Config.Current.Layout.Panels.ContentItemProfile;
        public PanelListItemProfile ContentItemProfile
        {
            get { return Config.Current.Layout.Panels.ContentItemProfile; }
            set { SetProperty(ref Config.Current.Layout.Panels.ContentItemProfile, value); }
        }


        private PanelListItemProfile Config.Current.Layout.Panels.BannerItemProfile;
        public PanelListItemProfile BannerItemProfile
        {
            get { return Config.Current.Layout.Panels.BannerItemProfile; }
            set { SetProperty(ref Config.Current.Layout.Panels.BannerItemProfile, value); }
        }


        private PanelListItemProfile Config.Current.Layout.Panels.ThumbnailItemProfile;
        public PanelListItemProfile ThumbnailItemProfile
        {
            get { return Config.Current.Layout.Panels.ThumbnailItemProfile; }
            set { SetProperty(ref Config.Current.Layout.Panels.ThumbnailItemProfile, value); }
        }
#endif



        [PropertyRange("@ParamListItemContentImageWidth", 0, 512, TickFrequency = 8, IsEditable = true, Tips = "@ParamListItemContentImageWidthTips")]
        public int ContentItemImageWidth
        {
            get { return Config.Current.Layout.Panels.ContentItemProfile.ImageWidth; }
            set { Config.Current.Layout.Panels.ContentItemProfile.ImageWidth = MathUtility.Clamp(value, 0, 512); }
        }

        [PropertyMember("@ParamListItemContentImageShape")]
        public PanelListItemImageShape ContentItemImageShape
        {
            get { return Config.Current.Layout.Panels.ContentItemProfile.ImageShape; }
            set { Config.Current.Layout.Panels.ContentItemProfile.ImageShape = value; }
        }

        [PropertyMember("@ParamListItemContentImagePopup", Tips = "@ParamListItemContentImagePopupTips")]
        public bool ContentItemIsImagePopupEnabled
        {
            get { return Config.Current.Layout.Panels.ContentItemProfile.IsImagePopupEnabled; }
            set { Config.Current.Layout.Panels.ContentItemProfile.IsImagePopupEnabled = value; }
        }

        [PropertyMember("@ParamListItemContentIsTextWrapped")]
        public bool ContentItemIsTextWrapped
        {
            get { return Config.Current.Layout.Panels.ContentItemProfile.IsTextWrapped; }
            set { Config.Current.Layout.Panels.ContentItemProfile.IsTextWrapped = value; }
        }

        [PropertyRange("@ParamListItemContentNoteOpacity", 0.0, 1.0, Tips = "@ParamListItemContentNoteOpacityTips")]
        public double ContentItemNoteOpacity
        {
            get { return Config.Current.Layout.Panels.ContentItemProfile.NoteOpacity; }
            set { Config.Current.Layout.Panels.ContentItemProfile.NoteOpacity = value; }
        }

#if false
        [PropertyMember("@ParamListItemContentIsDecoratePlace", Tips = "@ParamListItemContentIsDecoratePlaceTips")]
        public bool IsDecoratePlace
        {
            get { return _isDecoratePlace; }
            set { SetProperty(ref _isDecoratePlace, value); }
        }
#endif


        [PropertyRange("@ParamListItemBannerImageWidth", 0, 512, TickFrequency = 8, IsEditable = true, Tips = "@ParamListItemBannerImageWidthTips")]
        public int BannerItemImageWidth
        {
            get { return Config.Current.Layout.Panels.BannerItemProfile.ImageWidth; }
            set { Config.Current.Layout.Panels.BannerItemProfile.ImageWidth = MathUtility.Clamp(value, 0, 512); }
        }

        [PropertyMember("@ParamListItemBannerIsTextWrapped")]
        public bool BannerItemIsTextWrapped
        {
            get { return Config.Current.Layout.Panels.BannerItemProfile.IsTextWrapped; }
            set { Config.Current.Layout.Panels.BannerItemProfile.IsTextWrapped = value; }
        }


        [PropertyRange("@ParamListItemThumbnailImageWidth", 64, 512, TickFrequency = 8, IsEditable = true, Tips = "@ParamListItemThumbnailImageWidthTips")]
        public int ThumbnailItemImageWidth
        {
            get { return Config.Current.Layout.Panels.ThumbnailItemProfile.ImageWidth; }
            set { Config.Current.Layout.Panels.ThumbnailItemProfile.ImageWidth = MathUtility.Clamp(value, 64, 512); }
        }

        [PropertyMember("@ParamListItemThumbnailImageShape")]
        public PanelListItemImageShape ThumbnailItemImageShape
        {
            get { return Config.Current.Layout.Panels.ThumbnailItemProfile.ImageShape; }
            set { Config.Current.Layout.Panels.ThumbnailItemProfile.ImageShape = value; }
        }

        [PropertyMember("@ParamListItemThumbnailNameVisibled")]
        public bool ThumbnailItemIsTextVisibled
        {
            get { return Config.Current.Layout.Panels.ThumbnailItemProfile.IsTextVisibled; }
            set { Config.Current.Layout.Panels.ThumbnailItemProfile.IsTextVisibled = value; }
        }

        [PropertyMember("@ParamListItemThumbnailIsTextWrapped")]
        public bool ThumbnailItemIsTextWrapped
        {
            get { return Config.Current.Layout.Panels.ThumbnailItemProfile.IsTextWrapped; }
            set { Config.Current.Layout.Panels.ThumbnailItemProfile.IsTextWrapped = value; }
        }


        public string GetDecoratePlaceName(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Config.Current.Layout.Panels.IsDecoratePlace ? LoosePath.GetPlaceName(s) : s;
        }

        private void RefreshBrushes()
        {
            var opacity = MainWindowModel.Current.CanHidePanel ? Config.Current.Layout.Panels.Opacity : 1.0;

            BackgroundBrush = CreatePanelBrush((SolidColorBrush)App.Current.Resources["NVBackground"], opacity);
            BaseBrush = CreatePanelBrush((SolidColorBrush)App.Current.Resources["NVBaseBrush"], opacity);
            IconBackgroundBrush = CreatePanelBrush((SolidColorBrush)App.Current.Resources["NVPanelIconBackground"], opacity);
        }

        private SolidColorBrush CreatePanelBrush(SolidColorBrush source, double opacity)
        {
            if (opacity < 1.0)
            {
                var color = source.Color;
                color.A = (byte)NeeLaboratory.MathUtility.Clamp((int)(opacity * 0xFF), 0x00, 0xFF);
                return new SolidColorBrush(color);
            }
            else
            {
                return source;
            }
        }


        // リソースにFontFamily適用
        private void SetFontFamilyResource(string fontName)
        {
            var fontFamily = fontName != null ? new FontFamily(fontName) : SystemFonts.MessageFontFamily;
            App.Current.Resources["PanelFontFamily"] = fontFamily;
        }

        // リソースにFontSize適用
        private void SetFontSizeResource(double fontSize)
        {
            App.Current.Resources["PanelFontSize"] = fontSize;
        }

        // リソースにFolderTreeFontSize適用
        private void SetFolderTreeFontSizeResource(double fontSize)
        {
            App.Current.Resources["FolderTreeFontSize"] = fontSize;
        }


        public void ValidatePanelListItemProfile()
        {
            Config.Current.Layout.Panels.ContentItemProfile.UpdateTextHeight();
            Config.Current.Layout.Panels.BannerItemProfile.UpdateTextHeight();
            Config.Current.Layout.Panels.ThumbnailItemProfile.UpdateTextHeight();
        }

#region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(true)]
            public bool IsLeftRightKeyEnabled { get; set; }

            [DataMember, DefaultValue(1.0)]
            public double Opacity { get; set; }

            [DataMember]
            public string FontName { get; set; }

            [DataMember, DefaultValue(15.0)]
            public double FontSize { get; set; }

            [DataMember, DefaultValue(12.0)]
            public double FolderTreeFontSize { get; set; }

            [DataMember]
            public PanelListItemProfile ContentItemProfile { get; set; }

            [DataMember]
            public PanelListItemProfile BannerItemProfile { get; set; }

            [DataMember]
            public PanelListItemProfile ThumbnailItemProfile { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsDecoratePlace { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
                if (BannerItemProfile != null)
                {
                    BannerItemProfile.ImageShape = PanelListItemImageShape.Banner;
                }
            }

            public void RestoreConfig(Config config)
            {
                // TODO: PanelListItemProfile

                config.Layout.Panels.IsLeftRightKeyEnabled = IsLeftRightKeyEnabled;
                config.Layout.Panels.Opacity = Opacity;
                config.Layout.Panels.FontName = FontName;
                config.Layout.Panels.FontSize = FontSize;
                config.Layout.Panels.FolderTreeFontSize = FolderTreeFontSize;
                config.Layout.Panels.IsDecoratePlace = IsDecoratePlace;

                config.Layout.Panels.ContentItemProfile = ContentItemProfile ?? PanelListItemProfile.DefaultContentItemProfile.Clone();
                config.Layout.Panels.BannerItemProfile = BannerItemProfile ?? PanelListItemProfile.DefaultBannerItemProfile.Clone();
                config.Layout.Panels.ThumbnailItemProfile = ThumbnailItemProfile ?? PanelListItemProfile.DefaultThumbnailItemProfile.Clone();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsLeftRightKeyEnabled = Config.Current.Layout.Panels.IsLeftRightKeyEnabled;
            memento.Opacity = Config.Current.Layout.Panels.Opacity;
            memento.FontName = Config.Current.Layout.Panels.FontName;
            memento.FontSize = Config.Current.Layout.Panels.FontSize;
            memento.FolderTreeFontSize = Config.Current.Layout.Panels.FolderTreeFontSize;
            memento.ContentItemProfile = Config.Current.Layout.Panels.ContentItemProfile.Clone();
            memento.BannerItemProfile = Config.Current.Layout.Panels.BannerItemProfile.Clone();
            memento.ThumbnailItemProfile = Config.Current.Layout.Panels.ThumbnailItemProfile.Clone();
            memento.IsDecoratePlace = Config.Current.Layout.Panels.IsDecoratePlace;

            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            ////this.IsLeftRightKeyEnabled = memento.IsLeftRightKeyEnabled;
            ////this.Opacity = memento.Opacity;
            ////this.FontName = memento.FontName;
            ////this.FontSize = memento.FontSize;
            ////this.FolderTreeFontSize = memento.FolderTreeFontSize;
            ////this.ContentItemProfile = memento.ContentItemProfile ?? _defaultContentItemProfile.Clone();
            ////this.BannerItemProfile = memento.BannerItemProfile ?? _defaultBannerItemProfile.Clone();
            ////this.ThumbnailItemProfile = memento.ThumbnailItemProfile ?? _defaultThumbnailItemProfile.Clone();
            ////this.IsDecoratePlace = memento.IsDecoratePlace;

            ValidatePanelListItemProfile();
        }

#endregion
    }

    // TODO: 定義位置
    public static class ObjectExtensions
    {
        public static T DeepCopy<T>(T source)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var mem = new MemoryStream())
            {
                serializer.WriteObject(mem, source);
                mem.Position = 0;
                return (T)serializer.ReadObject(mem);
            }
        }
    }
}
