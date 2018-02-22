// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;

namespace NeeView
{
    /// <summary>
    /// ロードオプションフラグ
    /// </summary>
    [Flags]
    public enum BookLoadOption
    {
        None = 0,

        /// <summary>
        /// 再帰
        /// </summary>
        Recursive = (1 << 0),

        /// <summary>
        /// すべてのファイルをページとみなす
        /// </summary>
        SupportAllFile = (1 << 1),

        /// <summary>
        /// 初期ページを先頭ページにする
        /// </summary>
        FirstPage = (1 << 2),

        /// <summary>
        /// 初期ページを最終ページにする
        /// </summary>
        LastPage = (1 << 3),

        /// <summary>
        /// 再読み込みフラグ(BookHubで使用)
        /// </summary>
        ReLoad = (1 << 4),

        /// <summary>
        /// 履歴の順番を変更しない
        /// </summary>
        KeepHistoryOrder = (1 << 5),

        /// <summary>
        /// 可能ならばフォルダーリストで選択する
        /// </summary>
        SelectFoderListMaybe = (1 << 6),

        /// <summary>
        /// 可能ならば履歴リストで選択する
        /// </summary>
        SelectHistoryMaybe = (1 << 7),

        /// <summary>
        /// 同じフォルダーならば読み込まない
        /// </summary>
        SkipSamePlace = (1 << 8),

        /// <summary>
        /// 自動再帰
        /// </summary>
        AutoRecursive = (1 << 9),

        /// <summary>
        /// 履歴情報から全て復元
        /// </summary>
        Resume = (1 << 10),

        /// <summary>
        /// 再帰、ただし履歴が優先
        /// </summary>
        DefaultRecursive = (1 << 11),

        /// <summary>
        /// 圧縮ファイル内の圧縮ファイルを再帰
        /// </summary>
        ArchiveRecursive = (1 << 12),

        /// <summary>
        /// 再帰しない
        /// </summary>
        NotRecursive = (1 << 13),

        /// <summary>
        /// このアドレスはブックです
        /// </summary>
        IsBook = (1 << 14),

        /// <summary>
        /// このアドレスはページです
        /// </summary>
        IsPage = (1 << 15),
    };

}
