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
        private List<IDisposable> _trashes = new List<IDisposable>();

        public void Add(IDisposable trash)
        {
            if (trash == null) return;
            _trashes.Add(trash);
        }

        // ゴミ箱を空にする
        public void Clear()
        {
            _trashes.Reverse();
            _trashes.ForEach(e => e.Dispose());
            _trashes.Clear();
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
        private string _fileName;

        public TrashFile(string fileName)
        {
            _fileName = fileName;
        }

        public void Dispose()
        {
            if (_fileName != null)
            {
                try
                {
                    File.Delete(_fileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                _fileName = null;
            }
        }
    }
}
