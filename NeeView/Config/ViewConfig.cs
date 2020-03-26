using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ViewConfig : BindableBaseFull
    {
        private PageStretchMode _stretchMode = PageStretchMode.Uniform;
        private bool _allowEnlarge = true;
        private bool _allowReduce = true;
        private AutoRotateType _autoRotate;
        private bool _isLimitMove = true;


        // 回転の中心
        [PropertyMember("@ParamDragTransformIsControRotatelCenter")]
        public DragControlCenter RotateCenter { get; set; }

        // 拡大の中心
        [PropertyMember("@ParamDragTransformIsControlScaleCenter")]
        public DragControlCenter ScaleCenter { get; set; }

        // 反転の中心
        [PropertyMember("@ParamDragTransformIsControlFlipCenter")]
        public DragControlCenter FlipCenter { get; set; }

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

        // ウィンドウ枠内の移動に制限する
        [PropertyMember("@ParamDragTransformIsLimitMove")]
        public bool IsLimitMove
        {
            get { return _isLimitMove; }
            set { SetProperty(ref _isLimitMove, value); }
        }


        // スケールモード
        public PageStretchMode StretchMode
        {
            get { return _stretchMode; }
            set { SetProperty(ref _stretchMode, value); }
        }

        // スケールモード・拡大許可
        public bool AllowEnlarge
        {
            get { return _allowEnlarge; }
            set { SetProperty(ref _allowEnlarge, value); }
        }

        // スケールモード・縮小許可
        public bool AllowReduce
        {
            get { return _allowReduce; }
            set { SetProperty(ref _allowReduce, value); }
        }

        // 自動回転左/右
        public AutoRotateType AutoRotate
        {
            get { return _autoRotate; }
            set { SetProperty(ref _autoRotate, value); }
        }
    }

}