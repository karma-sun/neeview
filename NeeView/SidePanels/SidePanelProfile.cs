﻿using NeeLaboratory;
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
            Current = new SidePanelProfile();
        }

        public static SidePanelProfile Current { get; }

        private SolidColorBrush _backgroundBrush;
        private SolidColorBrush _baseBrush;
        private SolidColorBrush _iconBackgroundBrush;

        private SidePanelProfile()
        {
            SetFontFamilyResource(Config.Current.Panels.FontName);
            SetFontSizeResource(Config.Current.Panels.FontSize);
            SetFolderTreeFontSizeResource(Config.Current.Panels.FolderTreeFontSize);

            ThemeProfile.Current.ThemeColorChanged += (s, e) => RefreshBrushes();
            MainWindowModel.Current.CanHidePanelChanged += (s, e) => RefreshBrushes();

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.Opacity), (s, e) =>
            {
                RefreshBrushes();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.FontName), (s, e) =>
            {
                try
                {
                    SetFontFamilyResource(Config.Current.Panels.FontName);
                    ValidatePanelListItemProfile();
                }
                catch
                {
                    // nop.
                }
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.FontSize), (s, e) =>
            {
                SetFontSizeResource(Config.Current.Panels.FontSize);
                ValidatePanelListItemProfile();
            });

            Config.Current.Panels.AddPropertyChanged(nameof(PanelsConfig.FolderTreeFontSize), (s, e) =>
            {
                SetFolderTreeFontSizeResource(Config.Current.Panels.FolderTreeFontSize);
            });

            ValidatePanelListItemProfile();
            RefreshBrushes();
        }

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

        [PropertyRange("@ParamListItemContentImageWidth", 0, 512, TickFrequency = 8, IsEditable = true, Tips = "@ParamListItemContentImageWidthTips")]
        public int ContentItemImageWidth
        {
            get { return Config.Current.Panels.ContentItemProfile.ImageWidth; }
            set { Config.Current.Panels.ContentItemProfile.ImageWidth = MathUtility.Clamp(value, 0, 512); }
        }

        [PropertyMember("@ParamListItemContentImageShape")]
        public PanelListItemImageShape ContentItemImageShape
        {
            get { return Config.Current.Panels.ContentItemProfile.ImageShape; }
            set { Config.Current.Panels.ContentItemProfile.ImageShape = value; }
        }

        [PropertyMember("@ParamListItemContentImagePopup", Tips = "@ParamListItemContentImagePopupTips")]
        public bool ContentItemIsImagePopupEnabled
        {
            get { return Config.Current.Panels.ContentItemProfile.IsImagePopupEnabled; }
            set { Config.Current.Panels.ContentItemProfile.IsImagePopupEnabled = value; }
        }

        [PropertyMember("@ParamListItemContentIsTextWrapped")]
        public bool ContentItemIsTextWrapped
        {
            get { return Config.Current.Panels.ContentItemProfile.IsTextWrapped; }
            set { Config.Current.Panels.ContentItemProfile.IsTextWrapped = value; }
        }

        [PropertyRange("@ParamListItemContentNoteOpacity", 0.0, 1.0, Tips = "@ParamListItemContentNoteOpacityTips")]
        public double ContentItemNoteOpacity
        {
            get { return Config.Current.Panels.ContentItemProfile.NoteOpacity; }
            set { Config.Current.Panels.ContentItemProfile.NoteOpacity = value; }
        }

        [PropertyRange("@ParamListItemBannerImageWidth", 0, 512, TickFrequency = 8, IsEditable = true, Tips = "@ParamListItemBannerImageWidthTips")]
        public int BannerItemImageWidth
        {
            get { return Config.Current.Panels.BannerItemProfile.ImageWidth; }
            set { Config.Current.Panels.BannerItemProfile.ImageWidth = MathUtility.Clamp(value, 0, 512); }
        }

        [PropertyMember("@ParamListItemBannerIsTextWrapped")]
        public bool BannerItemIsTextWrapped
        {
            get { return Config.Current.Panels.BannerItemProfile.IsTextWrapped; }
            set { Config.Current.Panels.BannerItemProfile.IsTextWrapped = value; }
        }


        [PropertyRange("@ParamListItemThumbnailImageWidth", 64, 512, TickFrequency = 8, IsEditable = true, Tips = "@ParamListItemThumbnailImageWidthTips")]
        public int ThumbnailItemImageWidth
        {
            get { return Config.Current.Panels.ThumbnailItemProfile.ImageWidth; }
            set { Config.Current.Panels.ThumbnailItemProfile.ImageWidth = MathUtility.Clamp(value, 64, 512); }
        }

        [PropertyMember("@ParamListItemThumbnailImageShape")]
        public PanelListItemImageShape ThumbnailItemImageShape
        {
            get { return Config.Current.Panels.ThumbnailItemProfile.ImageShape; }
            set { Config.Current.Panels.ThumbnailItemProfile.ImageShape = value; }
        }

        [PropertyMember("@ParamListItemThumbnailNameVisibled")]
        public bool ThumbnailItemIsTextVisibled
        {
            get { return Config.Current.Panels.ThumbnailItemProfile.IsTextVisibled; }
            set { Config.Current.Panels.ThumbnailItemProfile.IsTextVisibled = value; }
        }

        [PropertyMember("@ParamListItemThumbnailIsTextWrapped")]
        public bool ThumbnailItemIsTextWrapped
        {
            get { return Config.Current.Panels.ThumbnailItemProfile.IsTextWrapped; }
            set { Config.Current.Panels.ThumbnailItemProfile.IsTextWrapped = value; }
        }


        public string GetDecoratePlaceName(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Config.Current.Panels.IsDecoratePlace ? LoosePath.GetPlaceName(s) : s;
        }

        private void RefreshBrushes()
        {
            var opacity = MainWindowModel.Current.CanHidePanel ? Config.Current.Panels.Opacity : 1.0;

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
            Config.Current.Panels.ContentItemProfile.UpdateTextHeight();
            Config.Current.Panels.BannerItemProfile.UpdateTextHeight();
            Config.Current.Panels.ThumbnailItemProfile.UpdateTextHeight();
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
                config.Panels.IsLeftRightKeyEnabled = IsLeftRightKeyEnabled;
                config.Panels.Opacity = Opacity;
                config.Panels.FontName = FontName;
                config.Panels.FontSize = FontSize;
                config.Panels.FolderTreeFontSize = FolderTreeFontSize;
                config.Panels.IsDecoratePlace = IsDecoratePlace;

                config.Panels.ContentItemProfile = ContentItemProfile ?? PanelListItemProfile.DefaultContentItemProfile.Clone();
                config.Panels.BannerItemProfile = BannerItemProfile ?? PanelListItemProfile.DefaultBannerItemProfile.Clone();
                config.Panels.ThumbnailItemProfile = ThumbnailItemProfile ?? PanelListItemProfile.DefaultThumbnailItemProfile.Clone();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsLeftRightKeyEnabled = Config.Current.Panels.IsLeftRightKeyEnabled;
            memento.Opacity = Config.Current.Panels.Opacity;
            memento.FontName = Config.Current.Panels.FontName;
            memento.FontSize = Config.Current.Panels.FontSize;
            memento.FolderTreeFontSize = Config.Current.Panels.FolderTreeFontSize;
            memento.ContentItemProfile = Config.Current.Panels.ContentItemProfile.Clone();
            memento.BannerItemProfile = Config.Current.Panels.BannerItemProfile.Clone();
            memento.ThumbnailItemProfile = Config.Current.Panels.ThumbnailItemProfile.Clone();
            memento.IsDecoratePlace = Config.Current.Panels.IsDecoratePlace;

            return memento;
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
