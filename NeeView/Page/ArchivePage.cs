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
using System.Windows.Media;


namespace NeeView
{
    /// <summary>
    /// フォルダーサムネイル専用ページ.
    /// Pageの仕組みを使用してサムネイルを作成する
    /// </summary>
    public class ArchivePage : Page
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="place"></param>
        /// <param name="entryName"></param>
        public ArchivePage(string place, string entryName = null)
        {
            Entry = ArchiveEntry.Create(place);
            Content = new ArchiveContent(Entry, entryName);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="entryName"></param>
        public ArchivePage(ArchiveEntry entry, string entryName = null)
        {
            Entry = entry;
            Content = new ArchiveContent(Entry, entryName);
        }
    }

}
