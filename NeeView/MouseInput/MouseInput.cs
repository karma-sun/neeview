// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// MouseInput: Model
    /// </summary>
    public class MouseInput : BindableBase
    {
        public static MouseInput Current { get; private set; }
        
        /// <summary>
        /// constructor
        /// </summary>
        public MouseInput()
        {
            Current = this;
        }

        /// <summary>
        /// コントロール初期化
        /// </summary>
        /// <param name="window"></param>
        /// <param name="sender"></param>
        /// <param name="targetView"></param>
        /// <param name="targetShadow"></param>
        public void Initialize(Window window, FrameworkElement sender, FrameworkElement targetView, FrameworkElement targetShadow)
        {
            this.Window = window;
            this.Sender = sender;
            this.TargetView = targetView;
            this.TargetShadow = targetShadow;
        }

        /// <summary>
        /// 所属ウィンドウ
        /// </summary>
        public Window Window { get; set; }

        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }

        /// <summary>
        /// 操作対象エレメント
        /// アニメーション対応
        /// </summary>
        public FrameworkElement TargetView { get; set; }

        /// <summary>
        /// 操作対象エレメント計算用
        /// アニメーション非対応。非表示の矩形のみ。
        /// 表示領域計算にはこちらを利用する
        /// </summary>
        public FrameworkElement TargetShadow { get; set; }

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public Point StartPoint { get; set; }



        // 左クリック長押しモード
        private LongButtonDownMode _longLeftButtonDownMode = LongButtonDownMode.Loupe;
        public LongButtonDownMode LongLeftButtonDownMode
        {
            get { return _longLeftButtonDownMode; }
            set { _longLeftButtonDownMode = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// IsVisibleLoupeInfo property.
        /// </summary>
        private bool _IsVisibleLoupeInfo = true;
        public bool IsVisibleLoupeInfo
        {
            get { return _IsVisibleLoupeInfo; }
            set { if (_IsVisibleLoupeInfo != value) { _IsVisibleLoupeInfo = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsLoupeCenter property.
        /// </summary>
        private bool _IsLoupeCenter;
        public bool IsLoupeCenter
        {
            get { return _IsLoupeCenter; }
            set { if (_IsLoupeCenter != value) { _IsLoupeCenter = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// 一定距離カーソルが移動したイベント
        /// </summary>
        public event EventHandler MouseMoved;

        /// <summary>
        /// 一定距離カーソルが移動したイベント発行
        /// TODO: 判定処理もここで
        /// </summary>
        public void RaiseMouseMoved()
        {
            MouseMoved?.Invoke(this, null);
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public LongButtonDownMode LongLeftButtonDownMode { get; set; }
            [DataMember]
            public bool IsLoupeCenter { get; set; }
            [DataMember]
            public bool IsVisibleLoupeInfo { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.LongLeftButtonDownMode = this.LongLeftButtonDownMode;
            memento.IsLoupeCenter = this.IsLoupeCenter;
            memento.IsVisibleLoupeInfo = this.IsVisibleLoupeInfo;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.LongLeftButtonDownMode = memento.LongLeftButtonDownMode;
            this.IsLoupeCenter = memento.IsLoupeCenter;
            this.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
        }
        #endregion


    }

}
