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

            };

            this.Items = new List<SettingItem>
            {
                new SettingItemSection(Properties.Resources.SettingPageImageCollection,
                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.SupportFileTypes)), new SettingItemImageCollection() { Collection = PictureProfile.Current.SupportFileTypes }) { IsStretch = true },

                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.SvgFileTypes)), new SettingItemImageCollection(30.0, false) { Collection = PictureProfile.Current.SvgFileTypes }) { IsStretch = true },
                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.IsSvgEnabled))),

                    new SettingItemProperty(PropertyMemberElement.Create(PictureProfile.Current, nameof(PictureProfile.IsAspectRatioEnabled))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsEnableAnimatedGif))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookProfile.Current, nameof(BookProfile.IsAllFileAnImage)))),
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
                    new SettingItemProperty(PropertyMemberElement.Create(ZipArchiverProfile.Current, nameof(ZipArchiverProfile.IsEnabled)))),

                new SettingItemSection(Properties.Resources.SettingPageArchiveZipAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(ZipArchiverProfile.Current, nameof(ZipArchiverProfile.SupportFileTypes)), new SettingItemCollectionControl() { Collection = ZipArchiverProfile.Current.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }))
                {
                    IsEnabled = new IsEnabledPropertyValue(ZipArchiverProfile.Current, nameof(ZipArchiverProfile.IsEnabled)),
                }
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
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.IsEnabled)))),

                new SettingItemSection(Properties.Resources.SettingPageArchiverSevenZipAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.SupportFileTypes)), new SettingItemCollectionControl() { Collection = SevenZipArchiverProfile.Current.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.X86DllPath)))
                    {
                        Visibility = new VisibilityPropertyValue(Config.IsX64 ? Visibility.Collapsed : Visibility.Visible),
                        IsStretch = true,
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.X64DllPath)))
                    {
                        Visibility = new VisibilityPropertyValue(Config.IsX64 ? Visibility.Visible : Visibility.Collapsed),
                        IsStretch = true
                    })
                {
                    IsEnabled = new IsEnabledPropertyValue(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.IsEnabled)),
                }
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
                    new SettingItemProperty(PropertyMemberElement.Create(PdfArchiverProfile.Current, nameof(PdfArchiverProfile.IsEnabled)))),

                new SettingItemSection(Properties.Resources.SettingPageArchivePdfAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(PdfArchiverProfile.Current, nameof(PdfArchiverProfile.RenderSize))))
                {
                    IsEnabled = new IsEnabledPropertyValue(PdfArchiverProfile.Current, nameof(PdfArchiverProfile.IsEnabled)),
                }
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
                    new SettingItemProperty(PropertyMemberElement.Create(MediaArchiverProfile.Current, nameof(MediaArchiverProfile.IsEnabled)))),

                new SettingItemSection(Properties.Resources.SettingPageArchiveMediaAdvance,
                    new SettingItemProperty(PropertyMemberElement.Create(MediaArchiverProfile.Current, nameof(MediaArchiverProfile.SupportFileTypes)), new SettingItemCollectionControl() { Collection = MediaArchiverProfile.Current.SupportFileTypes, AddDialogHeader = Properties.Resources.WordExtension }),
                    new SettingItemProperty(PropertyMemberElement.Create(MediaControl.Current, nameof(MediaControl.PageSeconds))),
                    new SettingItemProperty(PropertyMemberElement.Create(MediaControl.Current, nameof(MediaControl.MediaStartDelaySeconds)))
                    )
                {
                    IsEnabled = new IsEnabledPropertyValue(MediaArchiverProfile.Current, nameof(MediaArchiverProfile.IsEnabled)),
                }
            };
        }
    }



}
