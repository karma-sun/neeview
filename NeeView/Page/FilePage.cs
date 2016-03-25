// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ファイルページ
    /// </summary>
    public class FilePage : Page
    {
        // ファイルページアイコン
        FilePageIcon _Icon;

        // 追加テキスト
        public string Text { get; set; }

        // コンストラクタ
        public FilePage(Archiver archiver, ArchiveEntry entry, string place, FilePageIcon icon)
        {
            Place = place;
            Entry = entry;
            _Icon = icon;
        }

        // コンテンツロード
        // FilePageContext を返す
        protected override object LoadContent()
        {
            Width = 320;
            Height = 320 * 1.25;
            Color = Colors.Black;

            var info = new FileBasicInfo();
            info.Archiver = Entry.Archiver.ToString();
            info.FileSize = Entry.FileSize;
            info.LastWriteTime = Entry.LastWriteTime;

            return new FilePageContent()
            {
                Icon = _Icon,
                FileName = FileName,
                Message = Text,
                Info = info,
            };
        }
    }
}
