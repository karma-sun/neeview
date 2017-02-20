// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Utility
{
    public class Shell
    {
        private static Shell _current;
        public static Shell Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new Shell();
                }
                return _current;
            }
        }

        private IWshRuntimeLibrary.WshShell _shell;
        public IWshRuntimeLibrary.WshShell WshShell => _shell;

        public Shell()
        {
            //WshShellを作成
            _shell = new IWshRuntimeLibrary.WshShell();
        }

        ~Shell()
        {
            if (_shell != null)
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(_shell);
            }
        }
    }


    public class FileShortcut
    {
        public string Path { get; private set; }

        public string TargetPath { get; private set; }

        public System.IO.DirectoryInfo DirectoryInfo { get; private set; }
        public System.IO.FileInfo FileInfo { get; private set; }

        public static bool IsShortcut(string path)
        {
            return System.IO.Path.GetExtension(path).ToLower() == ".lnk";
        }

        public FileShortcut(string path)
        {
            Open(path);
        }

        public void Open(string path)
        {
            if (!IsShortcut(path)) throw new NotSupportedException($"{path} is not link file.");

            this.Path = path;

            //
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)Shell.Current.WshShell.CreateShortcut(path);

            //
            this.TargetPath = shortcut.TargetPath;

            //
            this.DirectoryInfo = new System.IO.DirectoryInfo(this.TargetPath);
            this.FileInfo = new System.IO.FileInfo(this.TargetPath);

            //
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
        }
    }
}
