using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Windows;

namespace NeeView.Setting
{
    public class SettingPageArchiver : SettingPage
    {
        public SettingPageArchiver() : base(Properties.Resources.SettingPageArchive)
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageArchiverZip(),
                new SettingPageArchiverSevenZip(),
                new SettingPageArchivePdf(),
                new SettingPageArchiveMedia(),
                new SettingPageSusie(),
            };

            var section = new SettingItemSection(Properties.Resources.SettingPageImageCollection);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.SupportFileTypes)), new SettingItemImageCollection() { Collection = PictureProfile.Current.SupportFileTypes }) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.SvgFileTypes)), new SettingItemImageCollection(30.0, false) { Collection = PictureProfile.Current.SvgFileTypes }) { IsStretch = true });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Svg, nameof(ImageSvgConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAspectRatioEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAnimatedGifEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAllFileSupported))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPageArchiverZip : SettingPage
    {
        public SettingPageArchiverZip() : base(Properties.Resources.SettingPageArchiveZip)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageArchiveZipFeature);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Archive.Zip.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }));

            this.Items = new List<SettingItem>() { section };
        }
    }

    public class SettingPageArchiverSevenZip : SettingPage
    {
        public SettingPageArchiverSevenZip() : base(Properties.Resources.SettingPageArchiverSevenZip)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageArchiverSevenZipFeature);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Archive.SevenZip.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }));

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

    public class SettingPageArchivePdf : SettingPage
    {
        public SettingPageArchivePdf() : base(Properties.Resources.SettingPageArchivePdf)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageArchivePdfFeature);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.SupportFileTypes)),
                 new SettingItemCollectionControl() { Collection = Config.Current.Archive.Pdf.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.RenderSize))));

            this.Items = new List<SettingItem>() { section };

        }
    }

    public class SettingPageArchiveMedia : SettingPage
    {
        public SettingPageArchiveMedia() : base(Properties.Resources.SettingPageArchiveMedia)
        {
            var section = new SettingItemSection(Properties.Resources.SettingPageArchiveMediaFeature);

            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.SupportFileTypes)),
                new SettingItemCollectionControl() { Collection = Config.Current.Archive.Media.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.PageSeconds))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.MediaStartDelaySeconds))));

            this.Items = new List<SettingItem>() { section };
        }
    }



}
