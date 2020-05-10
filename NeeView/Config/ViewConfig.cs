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
        private DragControlCenter _rotateCenter;
        private DragControlCenter _scaleCenter;
        private DragControlCenter _flipCenter;
        private bool _isKeepScale;
        private bool _isKeepAngle;
        private bool _isKeepFlip;
        private bool _isViewStartPositionCenter;
        private double _angleFrequency = 0;
        private bool _forceAutoRotate;
        private bool _isBaseScaleEnabled;
        private double _baseScale = 1.0;

        // 回転の中心
        [PropertyMember("@ParamDragTransformIsControRotatelCenter")]
        public DragControlCenter RotateCenter
        {
            get { return _rotateCenter; }
            set { SetProperty(ref _rotateCenter, value); }
        }

        // 拡大の中心
        [PropertyMember("@ParamDragTransformIsControlScaleCenter")]
        public DragControlCenter ScaleCenter
        {
            get { return _scaleCenter; }
            set { SetProperty(ref _scaleCenter, value); }
        }

        // 反転の中心
        [PropertyMember("@ParamDragTransformIsControlFlipCenter")]
        public DragControlCenter FlipCenter
        {
            get { return _flipCenter; }
            set { SetProperty(ref _flipCenter, value); }
        }

        // 拡大率キープ
        [PropertyMember("@ParamDragTransformIsKeepScale")]
        public bool IsKeepScale
        {
            get { return _isKeepScale; }
            set { SetProperty(ref _isKeepScale, value); }
        }

        // 回転キープ
        [PropertyMember("@ParamDragTransformIsKeepAngle", Tips = "@ParamDragTransformIsKeepAngleTips")]
        public bool IsKeepAngle
        {
            get { return _isKeepAngle; }
            set { SetProperty(ref _isKeepAngle, value); }
        }

        // 反転キープ
        [PropertyMember("@ParamDragTransformIsKeepFlip")]
        public bool IsKeepFlip
        {
            get { return _isKeepFlip; }
            set { SetProperty(ref _isKeepFlip, value); }
        }

        // 表示開始時の基準
        [PropertyMember("@ParamDragTransformIsViewStartPositionCenter", Tips = "@ParamDragTransformIsViewStartPositionCenterTips")]
        public bool IsViewStartPositionCenter
        {
            get { return _isViewStartPositionCenter; }
            set { SetProperty(ref _isViewStartPositionCenter, value); }
        }

        // 回転スナップ。0で無効
        [PropertyMember("@ParamDragTransformAngleFrequency")]
        public double AngleFrequency
        {
            get { return _angleFrequency; }
            set { SetProperty(ref _angleFrequency, value); }
        }

        // ウィンドウ枠内の移動に制限する
        [PropertyMember("@ParamDragTransformIsLimitMove")]
        public bool IsLimitMove
        {
            get { return _isLimitMove; }
            set { SetProperty(ref _isLimitMove, value); }
        }

        // スケールモード
        [PropertyMember("@ParamViewStretchMode")]
        public PageStretchMode StretchMode
        {
            get { return _stretchMode; }
            set { SetProperty(ref _stretchMode, value); }
        }

        // スケールモード・拡大許可
        [PropertyMember("@ParamViewAllowEnlarge")]
        public bool AllowEnlarge
        {
            get { return _allowEnlarge; }
            set { SetProperty(ref _allowEnlarge, value); }
        }

        // スケールモード・縮小許可
        [PropertyMember("@ParamViewAllowReduce")]
        public bool AllowReduce
        {
            get { return _allowReduce; }
            set { SetProperty(ref _allowReduce, value); }
        }

        // 基底スケール有効
        [PropertyMember("@ParamViewIsBaseScaleEnabled", Tips = "@ParamViewIsBaseScaleEnabledTips")]
        public bool IsBaseScaleEnabled
        {
            get { return _isBaseScaleEnabled; }
            set { SetProperty(ref _isBaseScaleEnabled, value); }
        }

        // 基底スケール
        [PropertyPercent("@ParamViewBaseScale", 0.1, 2.0, TickFrequency = 0.01, Tips = "@ParamViewBaseScaleTips")]
        public double BaseScale
        {
            get { return _baseScale; }
            set { SetProperty(ref _baseScale, value); }
        }

        // 自動回転左/右
        [PropertyMember("@ParmViewAutoRotate")]
        public AutoRotateType AutoRotate
        {
            get { return _autoRotate; }
            set { SetProperty(ref _autoRotate, value); }
        }

        // 自動回転の強制
        [PropertyMember("@ParmViewForceAutoRotate")]
        public bool ForceAutoRotate
        {
            get { return _forceAutoRotate; }
            set { SetProperty(ref _forceAutoRotate, value); }
        }

    }

}