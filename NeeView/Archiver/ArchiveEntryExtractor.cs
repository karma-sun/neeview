﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
    /// ArchiveEntryExtractor イベント引数
    /// </summary>
    public class ArchiveEntryExtractorEventArgs : EventArgs
    {
        public CancellationToken CancellationToken { get; set; }
    }

    /// <summary>
    /// ArchiveEntryからファイルに展開する。キャンセル可。
    /// </summary>
    public class ArchiveEntryExtractor
    {
        /// <summary>
        /// 展開完了イベント
        /// </summary>
        public event EventHandler<ArchiveEntryExtractorEventArgs> Completed;

        /// <summary>
        /// 非同期アクション
        /// </summary>
        private Utility.AsynchronousAction _action;

        /// <summary>
        /// 元になるArchiveEntry
        /// </summary>
        public ArchiveEntry Entry { get; private set; }

        /// <summary>
        /// 展開ファイルパス
        /// </summary>
        public string ExtractFileName { get; private set; }

        /// <summary>
        /// 処理開始済？
        /// </summary>
        public bool IsActive => _action != null;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="entry"></param>
        public ArchiveEntryExtractor(ArchiveEntry entry)
        {
            Entry = entry;
        }

        /// <summary>
        /// extractor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> ExtractAsync(string path, CancellationToken token)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            ExtractFileName = path;

            _action = new Utility.AsynchronousAction();
            await _action.ExecuteAsync((t) =>
            {
                Entry.ExtractToFile(ExtractFileName, false);
                //Debug.WriteLine("EXT: Extract done.");
                Completed?.Invoke(this, new ArchiveEntryExtractorEventArgs() { CancellationToken = token });
            },
            token);

            return ExtractFileName;
        }

        /// <summary>
        /// wait
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> WaitAsync(CancellationToken token)
        {
            Debug.Assert(_action != null);

            await _action.WaitAsync(token);
            return ExtractFileName;
        }
    }
}