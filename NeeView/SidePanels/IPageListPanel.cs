// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ページのListBoxを持つ、サムネイル更新が必要なコントロールのインターフェイス
    /// </summary>
    public interface IPageListPanel
    {
        /// <summary>
        /// IHasPage 要素を持つ ListBox
        /// </summary>
        ListBox PageListBox { get; }

        /// <summary>
        /// サムネイルが表示されているか
        /// </summary>
        bool IsThumbnailVisibled { get; }
    }
}
