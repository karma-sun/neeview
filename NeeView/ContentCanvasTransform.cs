using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class ContentCanvasTransform : BindableBase
    {
        // system object.
        public static ContentCanvasTransform Current { get; private set; }

        // 移動制限モード
        public bool IsLimitMove { get; set; } = true;

        // 回転単位
        public double AngleFrequency { get; set; }

        // 回転、拡縮をコンテンツの中心基準にする
        public bool IsControlCenterImage { get; set; }

        // 拡大率キープ
        public bool IsKeepScale { get; set; }

        // 回転キープ
        public bool IsKeepAngle { get; set; }

        // 反転キープ
        public bool IsKeepFlip { get; set; }

        // 表示開始時の基準
        public bool IsViewStartPositionCenter { get; set; }



        /// <summary>
        /// ContentAngle property.
        /// </summary>
        private double _contentAngle;
        public double ContentAngle
        {
            get { return _contentAngle; }
            set { if (_contentAngle != value) { _contentAngle = value; RaisePropertyChanged(); } }
        }


        public ContentCanvasTransform()
        {
            Current = this;
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsLimitMove { get; set; }
            [DataMember]
            public double AngleFrequency { get; set; }
            [DataMember]
            public bool IsControlCenterImage { get; set; }
            [DataMember]
            public bool IsKeepScale { get; set; }
            [DataMember]
            public bool IsKeepAngle { get; set; }
            [DataMember]
            public bool IsKeepFlip { get; set; }
            [DataMember]
            public bool IsViewStartPositionCenter { get; set; }
        }


        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsLimitMove = this.IsLimitMove;
            memento.AngleFrequency = this.AngleFrequency;
            memento.IsControlCenterImage = this.IsControlCenterImage;
            memento.IsKeepScale = this.IsKeepScale;
            memento.IsKeepAngle = this.IsKeepAngle;
            memento.IsKeepFlip = this.IsKeepFlip;
            memento.IsViewStartPositionCenter = this.IsViewStartPositionCenter;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsLimitMove = memento.IsLimitMove;
            this.AngleFrequency = memento.AngleFrequency;
            this.IsControlCenterImage = memento.IsControlCenterImage;
            this.IsKeepScale = memento.IsKeepScale;
            this.IsKeepAngle = memento.IsKeepAngle;
            this.IsKeepFlip = memento.IsKeepFlip;
            this.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
        }
 
        #endregion
    }
}