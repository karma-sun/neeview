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
    public enum ExternalProgramType
    {
        [AliasName("@EnumExternalProgramTypeNormal")]
        Normal,

        [AliasName("@EnumExternalProgramTypeProtocol")]
        Protocol,
    }

    // 複数ページのときの動作
    public enum MultiPageOptionType
    {
        [AliasName("@EnumMultiPageOptionTypeOnce")]
        Once,

        [AliasName("@EnumMultiPageOptionTypeTwice")]
        Twice,
    };

    // 圧縮ファイルの時の動作
    public enum ArchiveOptionType
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
                    switch (Config.Current.External.ArchiveOption)
                    {
                        case ArchiveOptionType.None:
                            break;
                        case ArchiveOptionType.SendArchiveFile:
                            CallProcess(page.GetFolderOpenPlace());
                            break;
                        case ArchiveOptionType.SendExtractFile:
                            if (page.Entry.IsDirectory)
                            {
                                throw new ApplicationException(Properties.Resources.ExceptionNotSupportArchiveFolder);
                            }
                            else
                            {
                                CallProcess(page.ContentAccessor.CreateTempFile(true).Path);
                            }
                            break;
                        case ArchiveOptionType.SendArchivePath:
                            CallProcess(page.Entry.CreateArchivePath(Config.Current.External.ArchiveSeparater));
                            break;
                    }
                }
                if (Config.Current.External.MultiPageOption == MultiPageOptionType.Once || Config.Current.External.ArchiveOption == ArchiveOptionType.SendArchiveFile) break;
            }
        }

        // 外部アプリの実行(コア)
        private void CallProcess(string fileName)
        {
            switch (Config.Current.External.ProgramType)
            {
                case ExternalProgramType.Normal:
                    if (string.IsNullOrWhiteSpace(Config.Current.External.Command))
                    {
                        this.LastCall = $"\"{fileName}\"";
                        Debug.WriteLine($"CallProcess: {LastCall}");
                        System.Diagnostics.Process.Start(fileName);
                    }
                    else
                    {
                        string param = ReplaceKeyword(Config.Current.External.Parameter, fileName);
                        this.LastCall = $"\"{Config.Current.External.Command}\" {param}";
                        Debug.WriteLine($"CallProcess: {LastCall}");
                        System.Diagnostics.Process.Start(Config.Current.External.Command, param);
                    }
                    return;

                case ExternalProgramType.Protocol:
                    if (!string.IsNullOrWhiteSpace(Config.Current.External.Protocol))
                    {
                        string protocol = ReplaceKeyword(Config.Current.External.Protocol, fileName);
                        this.LastCall = protocol;
                        Debug.WriteLine($"CallProcess: {LastCall}");
                        System.Diagnostics.Process.Start(protocol);
                    }
                    return;
            }
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
