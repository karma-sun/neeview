// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // ファイルページ用コンテンツのアイコン
    public enum FilePageIcon
    {
        File,
        Archive,
        Folder,
        Alart,
    }

    /// <summary>
    /// ファイルページ用コンテンツ
    /// FilePageControl のパラメータとして使用される
    /// </summary>
    public class FilePageContent
    {
        public FilePageIcon Icon { get; set; }
        public string FileName { get; set; }
        public string Message { get; set; }

        public FileBasicInfo Info { get; set; }
    }
}
