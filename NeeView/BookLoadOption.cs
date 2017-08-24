// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
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
        Recursive = (1 << 0), // 再帰
        SupportAllFile = (1 << 1), // すべてのファイルをページとみなす
        FirstPage = (1 << 2), // 初期ページを先頭ページにする
        LastPage = (1 << 3), // 初期ページを最終ページにする
        ReLoad = (1 << 4), // 再読み込みフラグ(BookHubで使用)
        KeepHistoryOrder = (1 << 5), // 履歴の順番を変更しない
        SelectFoderListMaybe = (1 << 6), // 可能ならばフォルダーリストで選択する
        SelectHistoryMaybe = (1 << 7), // 可能ならば履歴リストで選択する
        SkipSamePlace = (1 << 8), // 同じフォルダーならば読み込まない
        AutoRecursive = (1 << 9), // 自動再帰
        Resume = (1 << 10), // 履歴情報から全て復元
        DefaultRecursive = (1 << 11), // 再帰、ただし履歴が優先
    };

}
