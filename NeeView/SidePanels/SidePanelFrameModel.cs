// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// SidePanelFrame Model
    /// 左右のパネルを管理
    /// </summary>
    public class SidePanelFrameModel : BindableBase
    {
        /// <summary>
        /// IsSideBarVisible property.
        /// </summary>
        public bool IsSideBarVisible
        {
            get { return _IsSideBarVisible; }
            set { if (_IsSideBarVisible != value) { _IsSideBarVisible = value; RaisePropertyChanged(); } }
        }

        //
        private bool _IsSideBarVisible;


        // スクロールビュータッチ操作の終端挙動
        [PropertyMember("パネルタッチスクロールの終端バウンド", Tips = "パネルのタッチスクロール操作での終端跳ね返り挙動の有効/無効を設定します")]
        public bool IsManipulationBoundaryFeedbackEnabled { get; set; }



        /// <summary>
        /// Left property.
        /// </summary>
        public SidePanelGroup Left
        {
            get { return _left; }
            set { if (_left != value) { _left = value; RaisePropertyChanged(); } }
        }

        //
        private SidePanelGroup _left;


        /// <summary>
        /// Right property.
        /// </summary>
        public SidePanelGroup Right
        {
            get { return _right; }
            set { if (_right != value) { _right = value; RaisePropertyChanged(); } }
        }

        //
        private SidePanelGroup _right;


        /// <summary>
        /// パネル選択変更イベント.
        /// 非表示状態のパネルを表示させるために使用される.
        /// </summary>
        public event EventHandler SelectedPanelChanged;


        /// <summary>
        /// パネル内容更新イベント.
        /// 自動非表示時間のリセットに使用される.
        /// </summary>
        public event EventHandler ContentChanged;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public SidePanelFrameModel()
        {
            _left = new SidePanelGroup();
            _left.PropertyChanged += Left_PropertyChanged;

            _right = new SidePanelGroup();
            _right.PropertyChanged += Right_PropertyChanged;
        }


        /// <summary>
        /// IsVisibleLocked property.
        /// </summary>
        public bool IsVisibleLocked
        {
            get { return _isVisibleLocked; }
            set { if (_isVisibleLocked != value) { _isVisibleLocked = value; RaisePropertyChanged(); } }
        }

        private bool _isVisibleLocked;


        /// <summary>
        /// パネル登録
        /// </summary>
        /// <param name="leftPanels"></param>
        /// <param name="rightPanels"></param>
        public void InitializePanels(List<IPanel> leftPanels, List<IPanel> rightPanels)
        {
            leftPanels.ForEach(e => _left.Panels.Add(e));
            rightPanels.ForEach(e => _right.Panels.Add(e));
        }

        /// <summary>
        /// コンテンツ変更通知
        /// </summary>
        public void RaiseContentChanged()
        {
            ContentChanged?.Invoke(this, null);
        }


        /// <summary>
        /// 右パネルのプロパティ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Right_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Right.SelectedPanel):
                    SelectedPanelChanged?.Invoke(Right, null);
                    break;
            }
        }

        /// <summary>
        /// 左パネルのプロパティ変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Left_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Left.SelectedPanel):
                    SelectedPanelChanged?.Invoke(Left, null);
                    break;
            }
        }

        /// <summary>
        ///  タッチスクロール終端挙動汎用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            if (!this.IsManipulationBoundaryFeedbackEnabled)
            {
                e.Handled = true;
            }
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsSideBarVisible { get; set; }

            [DataMember, DefaultValue(false)]
            [PropertyMember("パネルタッチスクロールの終端バウンド", Tips = "パネルのタッチスクロール操作での終端跳ね返り挙動の有効/無効を設定します")]
            public bool IsManipulationBoundaryFeedbackEnabled { get; set; }

            [DataMember]
            public SidePanelGroup.Memento Left { get; set; }

            [DataMember]
            public SidePanelGroup.Memento Right { get; set; }
        }

        /// <summary>
        /// Memento作成
        /// </summary>
        /// <returns></returns>
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsSideBarVisible = this.IsSideBarVisible;
            memento.Left = Left.CreateMemento();
            memento.Right = Right.CreateMemento();
            memento.IsManipulationBoundaryFeedbackEnabled = this.IsManipulationBoundaryFeedbackEnabled;

            return memento;
        }


        /// <summary>
        /// Memento適用
        /// </summary>
        /// <param name="memento"></param>
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            // パネル収集
            var panels = _left.Panels.Concat(_right.Panels).ToList();
            _left.Panels.Clear();
            _right.Panels.Clear();

            // memento反映
            this.IsSideBarVisible = memento.IsSideBarVisible;
            this.IsManipulationBoundaryFeedbackEnabled = memento.IsManipulationBoundaryFeedbackEnabled;
            _left.Restore(memento.Left, panels);
            _right.Restore(memento.Right, panels);

            // 未登録パネルを既定パネルに登録
            foreach (var panel in panels.Where(e => !_left.Panels.Contains(e) && !_right.Panels.Contains(e)))
            {
                (panel.DefaultPlace == PanelPlace.Right ? _right : _left).Panels.Add(panel);
            }

            // 情報更新
            SelectedPanelChanged?.Invoke(this, null);
        }

        #endregion
    }
}
