// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ゴミ箱(簡易)
    /// </summary>
    public class TrashBox : IDisposable
    {
        List<IDisposable> _Trashes = new List<IDisposable>();

        public void Add(IDisposable trash)
        {
            _Trashes.Add(trash);
        }

        // ゴミ箱を空にする
        public void Clear()
        {
            _Trashes.ForEach(e => e.Dispose());
            _Trashes.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }

    /// <summary>
    /// 廃棄ファイル
    /// </summary>
    public class TrashFile : IDisposable
    {
        string _FileName;

        public TrashFile(string fileName)
        {
            _FileName = fileName;
        }

        public void Dispose()
        {
            if (_FileName != null)
            {
                try
                {
                    File.Delete(_FileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                _FileName = null;
            }
        }
    }
}
