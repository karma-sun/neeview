using NeeView.Susie;
using NeeView.Text;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Windows;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: FileTypes
    /// </summary>
    public class SettingPageFileTypes : SettingPage
    {
        private class SettingItemCollectionDescription : ISettingItemCollectionDescription
        {
            public StringCollection GetDefaultCollection()
            {
                return PictureFileExtensionTools.CreateDefaultSupprtedFileTypes(Config.Current.Image.Standard.UseWicInformation);
            }
        }

        public SettingPageFileTypes() : base(Properties.Resources.SettingPage_Archive)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageArchiverZip(),
                new SettingPageArchiverSevenZip(),
                new SettingPageArchivePdf(),
                new SettingPageArchiveMedia(),
                new SettingPageSusie(),
            };

            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_FileTypes);

            var supportFileTypeEditor = new SettingItemCollectionControl() { Collection = (FileTypeCollection)PictureProfile.Current.SupportFileTypes.Clone(), AddDialogHeader = Properties.Resources.Word_Extension, IsAlwaysResetEnabled = true, Description = new SettingItemCollectionDescription() };
            supportFileTypeEditor.CollectionChanged += SupportFileTypeEditor_CollectionChanged;
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.SupportFileTypes)), supportFileTypeEditor));

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.UseWicInformation))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAllFileSupported))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAnimatedGifEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAspectRatioEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.System, nameof(SystemConfig.IsIgnoreImageDpi))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Background, nameof(BackgroundConfig.PageBackgroundColor))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Background, nameof(BackgroundConfig.IsPageBackgroundChecker))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_FileTypes_Svg);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Svg, nameof(ImageSvgConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Svg, nameof(ImageSvgConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Image.Svg.SupportFileTypes, AddDialogHeader = Properties.Resources.Word_Extension }));
            this.Items.Add(section);
        }

        private void SupportFileTypeEditor_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            var editor = (SettingItemCollectionControl)sender;
            PictureProfile.Current.SupportFileTypes = (FileTypeCollection)editor.Collection;
        }
    }


    /// <summary>
    /// SettingPage: Archive ZIP
    /// </summary>
    public class SettingPageArchiverZip : SettingPage
    {
        public SettingPageArchiverZip() : base(Properties.Resources.SettingPage_Archive_Zip)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Archive_ZipFeature);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Archive.Zip.SupportFileTypes, AddDialogHeader = Properties.Resources.Word_Extension }));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// SettingPage: Archive 7-Zip
    /// </summary>
    public class SettingPageArchiverSevenZip : SettingPage
    {
        public SettingPageArchiverSevenZip() : base(Properties.Resources.SettingPage_Archive_rSevenZip)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Archive_rSevenZipFeature);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Archive.SevenZip.SupportFileTypes, AddDialogHeader = Properties.Resources.Word_Extension }));

            if (!Environment.IsX64)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.X86DllPath))) { IsStretch = true, });
            }

            if (Environment.IsX64)
            {
                section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.X64DllPath))) { IsStretch = true });
            }

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// SettingPage: Archive PDF
    /// </summary>
    public class SettingPageArchivePdf : SettingPage
    {
        public SettingPageArchivePdf() : base(Properties.Resources.SettingPage_Archive_Pdf)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Archive_PdfFeature);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.SupportFileTypes)),
                 new SettingItemCollectionControl() { Collection = Config.Current.Archive.Pdf.SupportFileTypes, AddDialogHeader = Properties.Resources.Word_Extension }));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.RenderSize))));

            this.Items = new List<SettingItem>() { section };

        }
    }


    /// <summary>
    /// SettingPage: Archive Media
    /// </summary>
    public class SettingPageArchiveMedia : SettingPage
    {
        public SettingPageArchiveMedia() : base(Properties.Resources.SettingPage_Archive_Media)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPage_Archive_MediaFeature);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Archive.Media.SupportFileTypes, AddDialogHeader = Properties.Resources.Word_Extension }));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.PageSeconds))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.MediaStartDelaySeconds))));

            this.Items = new List<SettingItem>() { section };
        }
    }


    /// <summary>
    /// Setting: Susie
    /// </summary>
    public class SettingPageSusie : SettingPage
    {
        public SettingPageSusie() : base(Properties.Resources.SettingPage_Susie)
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(Properties.Resources.SettingPage_Susie_GeneralGeneral, Properties.Resources.SettingPage_Susie_GeneralGeneral_Remarks);
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.SusiePluginPath)))
            {
                IsStretch = true,
                IsEnabled = new IsEnabledPropertyValue(Config.Current.Susie, nameof(SusieConfig.IsEnabled)),
            });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsFirstOrderSusieImage))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Susie, nameof(SusieConfig.IsFirstOrderSusieArchive))));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Susie_ImagePlugin);
            section.Children.Add(new SettingItemSusiePlugin(SusiePluginType.Image));
            this.Items.Add(section);

            section = new SettingItemSection(Properties.Resources.SettingPage_Susie_ArchivePlugin);
            section.Children.Add(new SettingItemSusiePlugin(SusiePluginType.Archive));
            this.Items.Add(section);
        }
    }

}
