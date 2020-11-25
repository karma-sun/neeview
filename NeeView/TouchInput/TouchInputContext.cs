using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    //
    public class TouchInputContext
    {
        public TouchInputContext(FrameworkElement sender, FrameworkElement target, MouseGestureCommandCollection gestureCommandCollection, DragTransform dragTransform, DragTransformControl dragTransformControl)
        {
            this.Sender = sender;
            this.Target = target;
            this.GestureCommandCollection = gestureCommandCollection;
            this.DragTransform = dragTransform;
            this.DragTransformControl = dragTransformControl;
        }

        /// <summary>
        /// イベント受取エレメント
        /// </summary>
        public FrameworkElement Sender { get; set; }

        /// <summary>
        /// 操作対象エレメント計算用
        /// アニメーション非対応。非表示の矩形のみ。
        /// 表示領域計算にはこちらを利用する
        /// </summary>
        public FrameworkElement Target { get; set; }

        /// <summary>
        /// ジェスチャーコマンドテーブル
        /// </summary>
        public MouseGestureCommandCollection GestureCommandCollection { get; set; }

        /// <summary>
        /// 操作する座標系
        /// </summary>
        public DragTransform DragTransform { get; set; }

        public DragTransformControl DragTransformControl { get; set; }

        /// <summary>
        /// 有効なタッチデバイス情報
        /// </summary>
        public Dictionary<StylusDevice, TouchContext> TouchMap { get; set; } = new Dictionary<StylusDevice, TouchContext>();


        /// <summary>
        /// 表示コンテンツのエリア情報取得
        /// </summary>
        /// <returns></returns>
        public DragArea GetArea()
        {
            return new DragArea(this.Sender, this.Target);
        }
    }
}
