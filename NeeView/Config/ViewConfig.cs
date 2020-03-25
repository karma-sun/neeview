using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ViewConfig : BindableBase
    {
        // 回転の中心
        [PropertyMember("@ParamDragTransformIsControRotatelCenter")]
        public DragControlCenter DragControlRotateCenter { get; set; }

        // 拡大の中心
        [PropertyMember("@ParamDragTransformIsControlScaleCenter")]
        public DragControlCenter DragControlScaleCenter { get; set; }

        // 反転の中心
        [PropertyMember("@ParamDragTransformIsControlFlipCenter")]
        public DragControlCenter DragControlFlipCenter { get; set; }

        // 拡大率キープ
        [PropertyMember("@ParamDragTransformIsKeepScale")]
        public bool IsKeepScale { get; set; }

        // 回転キープ
        [PropertyMember("@ParamDragTransformIsKeepAngle", Tips = "@ParamDragTransformIsKeepAngleTips")]
        public bool IsKeepAngle { get; set; }

        // 反転キープ
        [PropertyMember("@ParamDragTransformIsKeepFlip")]
        public bool IsKeepFlip { get; set; }

        // 表示開始時の基準
        [PropertyMember("@ParamDragTransformIsViewStartPositionCenter", Tips = "@ParamDragTransformIsViewStartPositionCenterTips")]
        public bool IsViewStartPositionCenter { get; set; }

        // 回転スナップ。0で無効
        [PropertyMember("@ParamDragTransformAngleFrequency")]
        public double AngleFrequency { get; set; } = 0;
    }

}