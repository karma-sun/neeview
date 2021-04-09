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
            FontParameters.Current.AddPropertyChanged(nameof(FontParameters.DefaultFontName),
                (s, e) => ValidatePanelListItemProfile());

            FontParameters.Current.AddPropertyChanged(nameof(FontParameters.PaneFontSize),
                (s, e) => ValidatePanelListItemProfile());

            ValidatePanelListItemProfile();
        }

        public static string GetDecoratePlaceName(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return Config.Current.Panels.IsDecoratePlace ? LoosePath.GetPlaceName(s) : s;
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
                config.Fonts.FontName = FontName;
                config.Fonts.PanelFontScale = FontSize / SystemVisualParameters.Current.MessageFontSize;
                config.Fonts.FolderTreeFontScale = FolderTreeFontSize / SystemVisualParameters.Current.MessageFontSize;
                config.Panels.IsDecoratePlace = IsDecoratePlace;

                config.Panels.ContentItemProfile = ContentItemProfile ?? PanelListItemProfile.DefaultContentItemProfile.Clone();
                config.Panels.BannerItemProfile = BannerItemProfile ?? PanelListItemProfile.DefaultBannerItemProfile.Clone();
                config.Panels.ThumbnailItemProfile = ThumbnailItemProfile ?? PanelListItemProfile.DefaultThumbnailItemProfile.Clone();
            }
        }

        #endregion
    }

}
