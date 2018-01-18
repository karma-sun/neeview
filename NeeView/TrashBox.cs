// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    /// <summary>
    /// ゴミ
    /// </summary>
    public interface ITrash : IDisposable
    {
        bool IsDisposed { get; }
    }


    /// <summary>
    /// ゴミ箱(簡易)
    /// Disposableなオブジェクトをまとめて廃棄
    /// </summary>
    public class TrashBox : ITrash
    {
        /// <summary>
        /// 現在のシステムオブジェクト
        /// (燃えないゴミの処理)
        /// </summary>
        static private TrashBox _current;
        static public TrashBox Current
        {
            get
            {
                _current = _current ?? new TrashBox();
                return _current;
            }
        }

        /// <summary>
        /// ゴミたち
        /// </summary>
        private List<ITrash> _trashes = new List<ITrash>();

        /// <summary>
        /// lock
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Any()
        {
            return _trashes.Any();
        }

        /// <summary>
        /// ごみ箱に登録
        /// </summary>
        /// <param name="trash"></param>
        public void Add(ITrash trash)
        {
            lock (_lock)
            {
                if (trash == null) return;
                _trashes.Add(trash);
            }
        }

        /// <summary>
        /// まとめてごみ箱に登録
        /// </summary>
        /// <param name="trashes"></param>
        public void Add(IEnumerable<ITrash> trashes)
        {
            lock (_lock)
            {
                foreach (var trash in trashes)
                {
                    _trashes.Add(trash);
                }
            }
        }

        /// <summary>
        /// ごみ箱を空にする
        /// </summary>
        public void CleanUp()
        {
            lock (_lock)
            {
                _trashes.Reverse();
                _trashes.ForEach(e => e.Dispose());
                _trashes.RemoveAll(e => e.IsDisposed);

                // 不燃物処理
                if (TrashBox.Current != this)
                {
                    TrashBox.Current.CleanUp();

                    // 新しい不燃物の登録
                    TrashBox.Current.Add(_trashes);
                    _trashes.Clear();
                }
            }
        }

        /// <summary>
        /// is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose()
        {
            CleanUp();
            IsDisposed = true;
        }
    }
}
