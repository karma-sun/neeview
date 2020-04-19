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

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageImageCollection,
               
                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.SupportFileTypes)), new SettingItemImageCollection() { Collection = PictureProfile.Current.SupportFileTypes }) { IsStretch = true },

                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.SvgFileTypes)), new SettingItemImageCollection(30.0, false) { Collection = PictureProfile.Current.SvgFileTypes }) { IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Svg, nameof(ImageSvgConfig.IsEnabled))),

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAspectRatioEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAnimatedGifEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Image.Standard, nameof(ImageStandardConfig.IsAllFileSupported)))
                )
            };
        }
    }

    public class SettingPageArchiverZip : SettingPage
    {
        public SettingPageArchiverZip() : base(Properties.Resources.SettingPageArchiveZip)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageArchiveZipFeature,

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.IsEnabled))),

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Zip, nameof(ZipArchiveConfig.SupportFileTypes)),
                        new SettingItemCollectionControl() { Collection = Config.Current.Archive.Zip.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension })
                )
            };
        }
    }

    public class SettingPageArchiverSevenZip : SettingPage
    {
        public SettingPageArchiverSevenZip() : base(Properties.Resources.SettingPageArchiverSevenZip)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageArchiverSevenZipFeature,
                    
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.IsEnabled))),

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.SupportFileTypes)),
                        new SettingItemCollectionControl() { Collection = Config.Current.Archive.SevenZip.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }),

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.X86DllPath)))
                    {
                        Visibility = new VisibilityPropertyValue(Environment.IsX64 ? Visibility.Collapsed : Visibility.Visible),
                        IsStretch = true,
                    },

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.SevenZip, nameof(SevenZipArchiveConfig.X64DllPath)))
                    {
                        Visibility = new VisibilityPropertyValue(Environment.IsX64 ? Visibility.Visible : Visibility.Collapsed),
                        IsStretch = true
                    }
                )
            };
        }
    }

    public class SettingPageArchivePdf : SettingPage
    {
        public SettingPageArchivePdf() : base(Properties.Resources.SettingPageArchivePdf)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageArchivePdfFeature,
                    
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.IsEnabled))),

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.SupportFileTypes)),
                        new SettingItemCollectionControl() { Collection = Config.Current.Archive.Pdf.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }),
                
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Pdf, nameof(PdfArchiveConfig.RenderSize)))
                )
            };
        }
    }

    public class SettingPageArchiveMedia : SettingPage
    {
        public SettingPageArchiveMedia() : base(Properties.Resources.SettingPageArchiveMedia)
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageArchiveMediaFeature,

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.IsEnabled))),

                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.SupportFileTypes)), new SettingItemCollectionControl() { Collection = Config.Current.Archive.Media.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.PageSeconds))),
                    new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Archive.Media, nameof(MediaArchiveConfig.MediaStartDelaySeconds)))
                )
            };
        }
    }



}
