using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        [AliasName("@EnumMultiPageOptionTypeAll")]
        All,

        [Obsolete] // ver.37
        [AliasName("@EnumMultiPageOptionTypeTwice", IsVisibled = false)]
        Twice = All,
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
    
    public static class ArchivePolicyExtensions
    {
        public static string ToSampleText(this ArchivePolicy self)
        {
            switch(self)
            {
                case ArchivePolicy.None:
                    return @"not run.";
                case ArchivePolicy.SendArchiveFile:
                    return @"C:\Archive.zip";
                case ArchivePolicy.SendArchivePath:
                    return @"C:\Archive.zip\File.jpg";
                case ArchivePolicy.SendExtractFile:
                    return @"ExtractToTempFolder\File.jpg";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }


    // 外部アプリ起動
    public class ExternalApplicationUtility
    {
        // コマンドパラメータ文字列のバリデート
        public static string ValidateApplicationParam(string source)
        {
            if (source == null) source = "";
            source = source.Trim();
            return source.Contains(OpenExternalAppCommandParameter.KeyFile) ? source : (source + $" \"{OpenExternalAppCommandParameter.KeyFile}\"");
        }

        /// <summary>
        /// 外部アプリ実行
        /// </summary>
        /// <param name="pages">実行するページ群</param>
        /// <param name="options">実行オプション</param>
        /// <param name="token">キャンセルトークン</param>
        public void Call(IEnumerable<Page> pages, OpenExternalAppCommandParameter options, CancellationToken token)
        {
            var files = new List<string>();

            foreach (var page in pages)
            {
                token.ThrowIfCancellationRequested();

                // file
                if (page.Entry.IsFileSystem)
                {
                    files.Add(page.GetFilePlace());
                }
                else if (page.Entry.Instance is ArchiveEntry archiveEntry && archiveEntry.IsFileSystem)
                {
                    files.Add(archiveEntry.SystemPath);
                }
                // in archive
                else
                {
                    switch (options.ArchivePolicy)
                    {
                        case ArchivePolicy.None:
                            break;
                        case ArchivePolicy.SendArchiveFile:
                            files.Add(page.GetFolderOpenPlace());
                            break;
                        case ArchivePolicy.SendExtractFile:
                            if (page.Entry.IsDirectory)
                            {
                                throw new ApplicationException(Properties.Resources.ExceptionNotSupportArchiveFolder);
                            }
                            else
                            {
                                files.Add(page.ContentAccessor.CreateTempFile(true).Path);
                            }
                            break;
                        case ArchivePolicy.SendArchivePath:
                            files.Add(page.Entry.SystemPath);
                            break;
                    }
                }
                if (options.MultiPagePolicy == MultiPagePolicy.Once) break;
            }

            Call(files, options);
        }

        /// <summary>
        /// 外部アプリ実行
        /// </summary>
        /// <param name="paths">実行するファイルパス群</param>
        /// <param name="options">実行オプション</param>
        /// <param name="token">キャンセルトークン</param>
        public void Call(IEnumerable<string> paths, OpenExternalAppCommandParameter options)
        {
            foreach (var path in paths.Distinct())
            {
                CallProcess(path, options);

                if (options.MultiPagePolicy == MultiPagePolicy.Once) break;
            }
        }


        // 外部アプリの実行(コア)
        private void CallProcess(string fileName, OpenExternalAppCommandParameter options)
        {
            string param = ReplaceKeyword(ValidateApplicationParam(options.Parameter), fileName);

            if (string.IsNullOrWhiteSpace(options.Command))
            {
                var sentence = $"\"{param}\"";
                Debug.WriteLine($"CallProcess: {sentence}");
                try
                {
                    System.Diagnostics.Process.Start(param);
                }
                catch (Exception ex)
                {
                    var message = $"{ex.Message}: {sentence}";
                    throw new InvalidOperationException(message, ex);
                }
            }
            else
            {
                var command = options.Command.Replace("$NeeView", Environment.AssemblyLocation);
                var sentence = $"\"{command}\" {param}";
                Debug.WriteLine($"CallProcess: {sentence}");
                try
                {
                    System.Diagnostics.Process.Start(command, param);
                }
                catch (Exception ex)
                {
                    var message = $"{ex.Message}: {sentence}";
                    throw new InvalidOperationException(message, ex);
                }
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
