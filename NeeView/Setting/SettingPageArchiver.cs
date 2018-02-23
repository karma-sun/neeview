using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Windows;

namespace NeeView.Setting
{
    public class SettingPageArchiver : SettingPage
    {
        public SettingPageArchiver() : base("対応形式")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageArchiverZip(),
                new SettingPageArchiverSevenZip(),
                new SettingPageArchivePdf(),
                new SettingPageArchiveMedia(),
            };

            if (SusieContext.IsSupportedSusie)
            {
                this.Children.Add(new SettingPageSusie());
            }
        }
    }

    public class SettingPageArchiverZip : SettingPage
    {
        public SettingPageArchiverZip() : base("ZIP")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("機能",
                    new SettingItemProperty(PropertyMemberElement.Create(ZipArchiverProfile.Current, nameof(ZipArchiverProfile.IsEnabled)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(ZipArchiverProfile.Current, nameof(ZipArchiverProfile.SupportFileTypes)), new SettingItemCollectionControl() { Collection = ZipArchiverProfile.Current.SupportFileTypes, AddDialogHeader = "拡張子" }))
                {
                    IsEnabled = new IsEnabledPropertyValue(ZipArchiverProfile.Current, nameof(ZipArchiverProfile.IsEnabled)),
                }
            };
        }
    }

    public class SettingPageArchiverSevenZip : SettingPage
    {
        public SettingPageArchiverSevenZip() : base("7-Zip")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("機能",
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.IsEnabled)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.SupportFileTypes)), new SettingItemCollectionControl() { Collection = SevenZipArchiverProfile.Current.SupportFileTypes, AddDialogHeader = "拡張子" }),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.PreExtractSolidSize))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.LockTime))),
                    new SettingItemProperty(PropertyMemberElement.Create(SevenZipArchiverProfile.Current, nameof(SevenZipArchiverProfile.IsPreExtract))),
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
        public SettingPageArchivePdf() : base("PDF")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("機能",
                    new SettingItemProperty(PropertyMemberElement.Create(PdfArchiverProfile.Current, nameof(PdfArchiverProfile.IsEnabled)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(PdfArchiverProfile.Current, nameof(PdfArchiverProfile.RenderSize))))
                {
                    IsEnabled = new IsEnabledPropertyValue(PdfArchiverProfile.Current, nameof(PdfArchiverProfile.IsEnabled)),
                }
            };
        }
    }

    public class SettingPageArchiveMedia : SettingPage
    {
        public SettingPageArchiveMedia() : base("動画")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("機能",
                    new SettingItemProperty(PropertyMemberElement.Create(MediaArchiverProfile.Current, nameof(MediaArchiverProfile.IsEnabled)))),

                new SettingItemSection("詳細設定",
                    new SettingItemProperty(PropertyMemberElement.Create(MediaArchiverProfile.Current, nameof(MediaArchiverProfile.SupportFileTypes)), new SettingItemCollectionControl() { Collection = MediaArchiverProfile.Current.SupportFileTypes, AddDialogHeader = "拡張子" }),
                    new SettingItemProperty(PropertyMemberElement.Create(MediaControl.Current, nameof(MediaControl.PageSeconds))))
                {
                    IsEnabled = new IsEnabledPropertyValue(MediaArchiverProfile.Current, nameof(MediaArchiverProfile.IsEnabled)),
                }
            };
        }
    }



}
