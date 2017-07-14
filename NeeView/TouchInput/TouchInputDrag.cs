﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{

    /// <summary>
    /// タッチ通常ドラッグ状態
    /// </summary>
    public class TouchInputDrag : TouchInputBase
    {
        private TouchDragManipulation _manipulation;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public TouchInputDrag(TouchInputContext context) : base(context)
        {
            _manipulation = new TouchDragManipulation(context);
        }

        /// <summary>
        /// 状態開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            _manipulation.Start();
        }


        /// <summary>
        /// 状態終了
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            _manipulation.Stop();
        }


        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusDown(object sender, StylusDownEventArgs e)
        {
            _manipulation.Start();
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusUp(object sender, StylusEventArgs e)
        {
            // タッチされなくなったら解除
            if (_context.TouchMap.Count < 1)
            {
                ResetState();
            }
            else
            {
                _manipulation.Start();
            }
        }


        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnStylusMove(object sender, StylusEventArgs e)
        {
            _manipulation.Update();
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public TouchDragManipulation.Memento Manipulation { get; set; }

        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.Manipulation = _manipulation.CreateMemento();
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            _manipulation.Restore(memento.Manipulation);
        }
        #endregion

    }
}