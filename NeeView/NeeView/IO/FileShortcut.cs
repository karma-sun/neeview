using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.IO
{
    /// <summary>
    /// ファイルショートカット
    /// </summary>
    public class FileShortcut
    {
        private FileInfo _source;
        private FileSystemInfo _target;


        public FileShortcut(string path)
        {
            Open(new FileInfo(path));
        }

        public FileShortcut(FileInfo source)
        {
            Open(source);
        }


        // リンク元ファイル
        public FileInfo Source => _source;
        public string SourcePath => _source?.FullName;

        // リンク先ファイル
        public FileSystemInfo Target => _target;
        public string TargetPath => _target?.FullName;

        // 有効？
        public bool IsValid => _target != null && _target.Exists;


        public static bool IsShortcut(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            if (QuerySchemeExtensions.GetScheme(path) != QueryScheme.File)
            {
                return false;
            }

            try
            {
                return Path.GetExtension(path).ToLower() == ".lnk";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public void Open(FileInfo source)
        {
            if (!IsShortcut(source.FullName)) throw new NotSupportedException($"{source.FullName} is not link file.");

            _source = source;

            try
            {
                var targetPath = GetLinkTargetPath(source);
                ////var targetPath = AppDispatcher.InvokeSTA(() => GetLinkTargetPathSTA(source));

                var directoryInfo = new DirectoryInfo(targetPath);
                if (directoryInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    _target = directoryInfo;
                }
                else
                {
                    _target = new FileInfo(targetPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShortcutFileName: {source.FullName}\n{ex.Message}");
                _target = null;
            }
        }

        /// <summary>
        /// ショートカットのターゲットパスを取得
        /// </summary>
        /// <param name="linkFile">ショートカットファイル</param>
        /// <returns>ターゲットパス</returns>
        private static string GetLinkTargetPath(FileInfo linkFile)
        {
            if (linkFile is null)
            {
                throw new ArgumentNullException(nameof(linkFile));
            }
            if (!linkFile.Exists)
            {
                throw new FileNotFoundException();
            }

            // NOTE: MTAでも動作するようにdynamic型を使用
            var shellAppType = Type.GetTypeFromProgID("Shell.Application");
            dynamic shell = Activator.CreateInstance(shellAppType);
            Shell32.Folder dir = shell.NameSpace(linkFile.DirectoryName);
            Shell32.FolderItem item = dir.Items().Item(linkFile.Name);
            if (!item.IsLink)
            {
                throw new NotSupportedException($"{linkFile.FullName} is not link file.");
            }

            Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)item.GetLink;
            Shell32.FolderItem target = link.Target;

            return target.Path;
        }

        /// <summary>
        /// ショートカットのターゲットパスを取得(STA)
        /// </summary>
        /// <param name="linkFile">ショートカットファイル</param>
        /// <returns>ターゲットパス</returns>
        private static string GetLinkTargetPathSTA(FileInfo linkFile)
        {
            AssertSTA();
            
            if (linkFile is null)
            {
                throw new ArgumentNullException(nameof(linkFile));
            }
            if (!linkFile.Exists)
            {
                throw new FileNotFoundException();
            }

            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder dir = shell.NameSpace(linkFile.DirectoryName);
            Shell32.FolderItem item = dir.Items().Item(linkFile.Name);
            if (!item.IsLink)
            {
                throw new NotSupportedException($"{linkFile.FullName} is not link file.");
            }

            Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)item.GetLink;
            Shell32.FolderItem target = link.Target;

            return target.Path;
        }

        /// <summary>
        /// check STA
        /// </summary>
        [Conditional("DEBUG")]
        private static void AssertSTA()
        {
            if (System.Threading.Thread.CurrentThread.GetApartmentState() != System.Threading.ApartmentState.STA)
            {
                Debug.Fail("Must be a STA thread.");
                throw new System.Threading.ThreadStateException("Must be a STA thread.");
            }
        }
    }
}
