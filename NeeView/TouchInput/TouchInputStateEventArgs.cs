// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;

namespace NeeView
{
    /// <summary>
    /// タッチ入力状態遷移イベントデータ
    /// </summary>
    public class TouchInputStateEventArgs : EventArgs
    {
        /// <summary>
        /// 遷移先状態
        /// </summary>
        public TouchInputState State { get; set; }

        /// <summary>
        /// 遷移パラメータ。
        /// 遷移状態により要求される内容は異なります。
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="state"></param>
        public TouchInputStateEventArgs(TouchInputState state)
        {
            this.State = state;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="state"></param>
        public TouchInputStateEventArgs(TouchInputState state, object parameter)
        {
            this.State = state;
            this.Parameter = parameter;
        }
    }
}
