using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using System.Threading;

namespace NeeView
{
    public class ExternalApp : BindableBase, ICloneable
    {
        private string _name;
        private string _command;
        private string _parameter = OpenExternalAppCommandParameter.DefaultParameter;
        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private string _workingDirectory;


        // 表示名
        [JsonIgnore]
        public string DispName => _name ?? (string.IsNullOrWhiteSpace(_command) ? Properties.Resources.Word_DefaultApp : LoosePath.GetFileNameWithoutExtension(_command));

        // 名前
        public string Name
        {
            get { return _name; }
            set { if (SetProperty(ref _name, value)) RaisePropertyChanged(nameof(DispName)); }
        }

        // コマンド
        public string Command
        {
            get { return _command; }
            set { if (SetProperty(ref _command, value?.Trim())) RaisePropertyChanged(nameof(DispName)); }
        }

        // コマンドパラメータ
        // $FILE = 渡されるファイルパス
        public string Parameter
        {
            get { return _parameter; }
            set { SetProperty(ref _parameter, string.IsNullOrWhiteSpace(value) ? OpenExternalAppCommandParameter.DefaultParameter : value); }
        }

        // 圧縮ファイルのときの動作
        public ArchivePolicy ArchivePolicy
        {
            get { return _archivePolicy; }
            set { SetProperty(ref _archivePolicy, value); }
        }

        // 作業フォルダー
        public string WorkingDirectory
        {
            get { return _workingDirectory; }
            set { SetProperty(ref _workingDirectory, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }


        private OpenExternalAppCommandParameter CreateCommandParameter()
        {
            var parameter = new OpenExternalAppCommandParameter()
            {
                Command = Command,
                Parameter = Parameter,
                MultiPagePolicy = MultiPagePolicy.All,
                ArchivePolicy = ArchivePolicy,
                WorkingDirectory = WorkingDirectory,
            };

            return parameter;
        }

        public void Execute(IEnumerable<Page> pages)
        {
            var external = new ExternalAppUtility();
            try
            {
                external.Call(pages, CreateCommandParameter(), CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.OpenApplicationErrorDialog_Title).ShowDialog();
            }
        }

        public void Execute(IEnumerable<string> files)
        {
            var external = new ExternalAppUtility();
            try
            {
                external.Call(files, CreateCommandParameter());
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.OpenApplicationErrorDialog_Title).ShowDialog();
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }


}
