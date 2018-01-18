// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
    /// <summary>
    /// エントリ群のソート
    /// TODO: bookと共通化
    /// </summary>
    public static class EntrySort
    {
        // TODO: 入力されたentriesを変更しないようにする
        /// <summary>
        /// ソート実行
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="sortMode"></param>
        /// <returns></returns>
        public static List<ArchiveEntry> SortEntries(List<ArchiveEntry> entries, PageSortMode sortMode)
        {
            if (entries == null || entries.Count <= 0) return entries;

            switch (sortMode)
            {
                case PageSortMode.FileName:
                    entries.Sort((a, b) => CompareFileNameOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.FileNameDescending:
                    entries.Sort((a, b) => CompareFileNameOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStamp:
                    entries.Sort((a, b) => CompareDateTimeOrder(a, b, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.TimeStampDescending:
                    entries.Sort((a, b) => CompareDateTimeOrder(b, a, Win32Api.StrCmpLogicalW));
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    entries = entries.OrderBy(e => random.Next()).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return entries;
        }

        // ファイル名, 日付, ID の順で比較
        private static int CompareFileNameOrder(ArchiveEntry e1, ArchiveEntry e2, Func<string, string, int> compare)
        {
            if (e1.EntryName != e2.EntryName)
                return CompareFileName(e1.EntryName, e2.EntryName, compare);
            else if (e1.LastWriteTime != e2.LastWriteTime)
                return CompareDateTime(e1.LastWriteTime, e2.LastWriteTime);
            else
                return e1.Id - e2.Id;
        }

        // 日付, ファイル名, ID の順で比較
        private static int CompareDateTimeOrder(ArchiveEntry e1, ArchiveEntry e2, Func<string, string, int> compare)
        {
            if (e1.LastWriteTime != e2.LastWriteTime)
                return CompareDateTime(e1.LastWriteTime, e2.LastWriteTime);
            else if (e1.EntryName != e2.EntryName)
                return CompareFileName(e1.EntryName, e2.EntryName, compare);
            else
                return e1.Id - e2.Id;
        }

        // ファイル名比較. ディレクトリを優先する
        private static int CompareFileName(string s1, string s2, Func<string, string, int> compare)
        {
            string d1 = LoosePath.GetDirectoryName(s1);
            string d2 = LoosePath.GetDirectoryName(s2);

            if (d1 == d2)
                return compare(s1, s2);
            else
                return compare(d1, d2);
        }

        // 日付比較。null対応
        private static int CompareDateTime(DateTime? _t1, DateTime? _t2)
        {
            DateTime t1 = _t1 ?? DateTime.MinValue;
            DateTime t2 = _t2 ?? DateTime.MinValue;
            return (t1.Ticks - t2.Ticks < 0) ? -1 : 1;
        }
    }

}
