using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class MouseInputContext : BindableBase
    {
        public MouseInputContext(FrameworkElement sender, MouseGestureCommandCollection gestureCommandCollection, DragTransformControl dragTransformControl, DragTransform dragTransform, LoupeTransform loupeTransform)
        {
            this.Sender = sender;
            this.GestureCommandCollection = gestureCommandCollection;
            this.DragTransformControl = dragTransformControl;
            this.DragTransform = dragTransform;
            this.LoupeTransform = loupeTransform;
        }


        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }
        
        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection GestureCommandCollection { get; set; }

        public DragTransformControl DragTransformControl { get; set; }
            
        public DragTransform DragTransform { get; set; }
            
        public LoupeTransform LoupeTransform { get; set; }

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        public Point StartPoint { get; set; }
    }

}
