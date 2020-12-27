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
    public class SidePanelProfile
    {
        public void Initialize()
        {
            SetFontFamilyResource(Config.Current.Panels.FontName);
            SetFontSizeResource(Config.Current.Panels.FontSize);
            SetFolderTreeFontSizeResource(Config.Current.Panels.FolderTreeFontSize);

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
        }

        public static string GetDecoratePlaceName(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Config.Current.Panels.IsDecoratePlace ? LoosePath.GetPlaceName(s) : s;
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

        #endregion
    }

}
