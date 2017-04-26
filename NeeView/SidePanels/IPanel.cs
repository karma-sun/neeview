// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// パネル定義
    /// </summary>
    public interface IPanel
    {
        /// <summary>
        /// 識別名
        /// </summary>
        string TypeCode { get; }

        /// <summary>
        /// アイコン
        /// </summary>
        ImageSource Icon { get; }

        /// <summary>
        /// アイコンレイアウト
        /// </summary>
        Thickness IconMargin { get; }

        /// <summary>
        /// アイコン説明
        /// </summary>
        string IconTips { get; }

        /// <summary>
        /// パネル実体
        /// </summary>
        FrameworkElement View { get; }

        /// <summary>
        /// 表示固定フラグ
        /// </summary>
        bool IsVisibleLock { get; }
    }
}
