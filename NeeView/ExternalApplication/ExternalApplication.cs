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
            return source.Contains(OpenExternalAppCommandParameter.KeyFile) ? source : (source + $" \"{OpenExternalAppCommandParameter.KeyFile}\"");
        }


        // 外部アプリの実行
        public void Call(List<Page> pages, OpenExternalAppCommandParameter options)
        {
            this.LastCall = null;

            foreach (var page in pages)
            {
                // file
                if (page.Entry.Archiver is FolderArchive)
                {
                    CallProcess(page.GetFilePlace(), options);
                }
                // in archive
                else
                {
                    switch (options.ArchivePolicy)
                    {
                        case ArchivePolicy.None:
                            break;
                        case ArchivePolicy.SendArchiveFile:
                            CallProcess(page.GetFolderOpenPlace(), options);
                            break;
                        case ArchivePolicy.SendExtractFile:
                            if (page.Entry.IsDirectory)
                            {
                                throw new ApplicationException(Properties.Resources.ExceptionNotSupportArchiveFolder);
                            }
                            else
                            {
                                CallProcess(page.ContentAccessor.CreateTempFile(true).Path, options);
                            }
                            break;
                        case ArchivePolicy.SendArchivePath:
                            CallProcess(page.Entry.CreateArchivePath(options.ArchiveSeparater), options);
                            break;
                    }
                }
                if (options.MultiPagePolicy == MultiPagePolicy.Once || options.ArchivePolicy == ArchivePolicy.SendArchiveFile) break;
            }
        }

        // 外部アプリの実行(コア)
        private void CallProcess(string fileName, OpenExternalAppCommandParameter options)
        {
            string param = ReplaceKeyword(ValidateApplicationParam(options.Parameter), fileName);

            if (string.IsNullOrWhiteSpace(options.Command))
            {
                this.LastCall = $"\"{param}\"";
                Debug.WriteLine($"CallProcess: {LastCall}");
                System.Diagnostics.Process.Start(param);
            }
            else
            {
                var command = options.Command.Replace("$NeeView", Environment.AssemblyLocation);
                this.LastCall = $"\"{command}\" {param}";
                Debug.WriteLine($"CallProcess: {LastCall}");
                System.Diagnostics.Process.Start(command, param);
            }
            return;
        }

        private static string ReplaceKeyword(string s, string filenName)
        {
            var uriData = Uri.EscapeDataString(filenName);

            s = s.Replace(OpenExternalAppCommandParameter.KeyUri, uriData);
            s = s.Replace(OpenExternalAppCommandParameter.KeyFile, filenName);
            return s;
        }
    }
}
