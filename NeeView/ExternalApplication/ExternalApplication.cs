using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // プログラムの種類
    [Obsolete]
    public enum ExternalProgramType
    {
        [AliasName("@EnumExternalProgramTypeNormal")]
        Normal,

        [AliasName("@EnumExternalProgramTypeProtocol")]
        Protocol,
    }

    // 複数ページのときの動作
    public enum MultiPagePolicy
    {
        [AliasName("@EnumMultiPageOptionTypeOnce")]
        Once,

        [AliasName("@EnumMultiPageOptionTypeTwice")]
        Twice,
    };

    // 圧縮ファイルの時の動作
    public enum ArchivePolicy
    {
        [AliasName("@EnumArchiveOptionTypeNone")]
        None,

        [AliasName("@EnumArchiveOptionTypeSendArchiveFile")]
        SendArchiveFile,

        [AliasName("@EnumArchiveOptionTypeSendArchivePath")]
        SendArchivePath, // ver 33.0

        [AliasName("@EnumArchiveOptionTypeSendExtractFile")]
        SendExtractFile,
    }

    public class ExternalApplicationParameters
    {
        public string Command { get; set; }

        public string Parameter { get; set; }

        public ArchivePolicy ArchivePolicy { get; set; }
        
        public string ArchiveSeparater { get; set; }

        public MultiPagePolicy MutiPagePolicy { get; set; }


        public static ExternalApplicationParameters CreateDefaultParameters()
        {
            var parameters = new ExternalApplicationParameters();
            parameters.Command = Config.Current.External.Command;
            parameters.Parameter = Config.Current.External.Parameter;
            parameters.ArchivePolicy = Config.Current.External.ArchivePolicy;
            parameters.ArchiveSeparater = Config.Current.External.ArchiveSeparater;
            parameters.MutiPagePolicy = Config.Current.External.MultiPagePolicy;
            return parameters;
        }
    }

    // 外部アプリ起動
    public class ExternalApplicationUtility
    {
        // 最後に実行したコマンド
        public string LastCall { get; set; }

        // コマンドパラメータ文字列のバリデート
        public static string ValidateApplicationParam(string source)
        {
            if (source == null) source = "";
            source = source.Trim();
            return source.Contains(ExternalConfig.KeyFile) ? source : (source + $" \"{ExternalConfig.KeyFile}\"").Trim();
        }


        // 外部アプリの実行
        public void Call(List<Page> pages)
        {
            this.LastCall = null;

            foreach (var page in pages)
            {
                // file
                if (page.Entry.Archiver is FolderArchive)
                {
                    CallProcess(page.GetFilePlace());
                }
                // in archive
                else
                {
                    switch (Config.Current.External.ArchivePolicy)
                    {
                        case ArchivePolicy.None:
                            break;
                        case ArchivePolicy.SendArchiveFile:
                            CallProcess(page.GetFolderOpenPlace());
                            break;
                        case ArchivePolicy.SendExtractFile:
                            if (page.Entry.IsDirectory)
                            {
                                throw new ApplicationException(Properties.Resources.ExceptionNotSupportArchiveFolder);
                            }
                            else
                            {
                                CallProcess(page.ContentAccessor.CreateTempFile(true).Path);
                            }
                            break;
                        case ArchivePolicy.SendArchivePath:
                            CallProcess(page.Entry.CreateArchivePath(Config.Current.External.ArchiveSeparater));
                            break;
                    }
                }
                if (Config.Current.External.MultiPagePolicy == MultiPagePolicy.Once || Config.Current.External.ArchivePolicy == ArchivePolicy.SendArchiveFile) break;
            }
        }

        // 外部アプリの実行(コア)
        private void CallProcess(string fileName)
        {
            string param = ReplaceKeyword(Config.Current.External.Parameter, fileName);

            if (string.IsNullOrWhiteSpace(Config.Current.External.Command))
            {
                this.LastCall = $"\"{param}\"";
                Debug.WriteLine($"CallProcess: {LastCall}");
                System.Diagnostics.Process.Start(param);
            }
            else
            {
                this.LastCall = $"\"{Config.Current.External.Command}\" {param}";
                Debug.WriteLine($"CallProcess: {LastCall}");
                System.Diagnostics.Process.Start(Config.Current.External.Command, param);
            }
            return;
        }

        private static string ReplaceKeyword(string s, string filenName)
        {
            var uriData = Uri.EscapeDataString(filenName);

            s = s.Replace(ExternalConfig.KeyUri, uriData);
            s = s.Replace(ExternalConfig.KeyFile, filenName);
            return s;
        }
    }
}
