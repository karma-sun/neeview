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
    /// Disposableなオブジェクトをまとめて廃棄
    /// </summary>
    public class TrashBox : IDisposable
    {
        private List<IDisposable> _trashes = new List<IDisposable>();

        /// <summary>
        /// ごみ箱に登録
        /// </summary>
        /// <param name="trash"></param>
        public void Add(IDisposable trash)
        {
            if (trash == null) return;
            _trashes.Add(trash);
        }

        /// <summary>
        /// ごみ箱を空にする
        /// </summary>
        public void Clear()
        {
            _trashes.Reverse();
            _trashes.ForEach(e => e.Dispose());
            _trashes.Clear();
        }

        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose()
        {
            Clear();
        }
    }
}
