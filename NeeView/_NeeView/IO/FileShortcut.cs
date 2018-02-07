// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
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

        #endregion
        
        #region Methods

        public static bool IsShortcut(string path)
        {
            return Path.GetExtension(path).ToLower() == ".lnk";
        }

        public void Open(FileInfo source)
        {
            if (!IsShortcut(source.FullName)) throw new NotSupportedException($"{source.FullName} is not link file.");

            _source = source;

            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)WshShell.Current.Shell.CreateShortcut(source.FullName);
            try
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
            finally
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            }
        }

        #endregion
    }
}
