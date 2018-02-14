using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Configure
{
    public class SettingPageExternal : SettingPage
    {
        public SettingPageExternal() : base("外部連携")
        {
            this.Children = new List<SettingPage>
            {
                new SettingPageExternalGeneral(),
                new SettingPageExternalProgram(),
            };
        }
    }

    public class SettingPageExternalGeneral : SettingPage
    {
        public SettingPageExternalGeneral() : base("外部連携全般")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("クリップボードへのファイルコピー",
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.MultiPageOption))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ClipboardUtility, nameof(ClipboardUtility.ArchiveOption)))),

                new SettingItemSection("ブラウザからのドラッグ&ドロップ",
                    new SettingItemProperty(PropertyMemberElement.Create(App.Current, nameof(App.DownloadPath))) { IsStretch = true }),

                new SettingItemSection("ファイル保存", "画像を保存するコマンドの設定です",
                    new SettingItemProperty(PropertyMemberElement.Create(ExporterProfile.Current, nameof(ExporterProfile.IsEnableExportFolder))),
                    new SettingItemProperty(PropertyMemberElement.Create(ExporterProfile.Current, nameof(ExporterProfile.QualityLevel)))),
            };
        }
    }
    public class SettingPageExternalProgram : SettingPage
    {
        public SettingPageExternalProgram() : base("外部アプリ")
        {
            this.Items = new List<SettingItem>
            {
                new SettingItemSection("外部アプリ設定",
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType))),
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Command))) { IsStretch = true },
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Parameter))) { IsStretch = true })
                    {
                        VisibleTrigger = new DataTriggerSource(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType), ExternalProgramType.Normal, true),
                    },
                    new SettingItemGroup(
                        new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.Protocol))) { IsStretch = true })
                    {
                        VisibleTrigger = new DataTriggerSource(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ProgramType), ExternalProgramType.Protocol, true),
                    },
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.MultiPageOption))),
                    new SettingItemProperty(PropertyMemberElement.Create(BookOperation.Current.ExternalApplication, nameof(ExternalApplication.ArchiveOption)))),
            };
        }
    }
}
