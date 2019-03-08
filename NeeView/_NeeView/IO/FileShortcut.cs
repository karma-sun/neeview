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
        #region Fields

        private FileInfo _source;
        private FileSystemInfo _target;

        #endregion

        #region Constructors

        public FileShortcut(string path)
        {
            Open(new FileInfo(path));
        }

        public FileShortcut(FileInfo source)
        {
            Open(source);
        }

        #endregion

        #region Properties

        // リンク元ファイル
        public FileInfo Source => _source;
        public string SourcePath => _source?.FullName;

        // リンク先ファイル
        public FileSystemInfo Target => _target;
        public string TargetPath => _target?.FullName;

        // 有効？
        public bool IsValid => _target != null && _target.Exists;

        #endregion

        #region Methods

        public static bool IsShortcut(string path)
        {
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

            IWshRuntimeLibrary.IWshShortcut shortcut = null;
            try
            {
                shortcut = (IWshRuntimeLibrary.IWshShortcut)WshShell.Current.Shell.CreateShortcut(source.FullName);
                if (string.IsNullOrWhiteSpace(shortcut?.TargetPath))
                {
                    Debug.WriteLine($"Cannot get shortcut target: {source.FullName}");
                    _target = null;
                }
                else
                {
                    var directoryInfo = new System.IO.DirectoryInfo(shortcut.TargetPath);
                    if (directoryInfo.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        _target = directoryInfo;
                    }
                    else
                    {
                        _target = new System.IO.FileInfo(shortcut.TargetPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShortcutFileName: {source.FullName}\nTargetPath: {shortcut.TargetPath}\n{ex.Message}");
                _target = null;
            }
            finally
            {
                if (shortcut != null)
                {
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
                }
            }
        }

        #endregion
    }
}
