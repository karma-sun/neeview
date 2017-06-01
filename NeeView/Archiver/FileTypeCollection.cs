// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// ファイル拡張子コレクション
    /// </summary>
    public class FileTypeCollection : StringCollection
    {
        public FileTypeCollection()
        {
        }

        public FileTypeCollection(string exts) : base(exts)
        {
        }

        // 文字列から設定
        public override void FromString(string exts)
        {
            if (exts == null) return;

            var list = new List<string>();
            foreach (var token in exts.Split(';'))
            {
                var ext = token.Trim().TrimStart('.').ToLower();
                if (!string.IsNullOrWhiteSpace(ext)) list.Add("." + ext);
            }

            _items = list;
        }
    }
}
