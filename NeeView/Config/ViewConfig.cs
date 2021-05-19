using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    public class ViewConfig : BindableBaseFull
    {
        private PageStretchMode _stretchMode = PageStretchMode.Uniform;
        private bool _allowStretchScaleUp = true;
        private bool _allowStretchScaleDown = true;
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
        private bool _isBaseScaleEnabled;
        private double _baseScale = 1.0;
        private bool _isRotateStretchEnabled = true;
        private double _mainViewMergin;
        private bool _isKeepScakeBooks;
        private bool _isKeepAngleBooks;
        private bool _isKeepFlipBooks;

        // 回転の中心
        [PropertyMember]
        public DragControlCenter RotateCenter
        {
            get { return _rotateCenter; }
            set { SetProperty(ref _rotateCenter, value); }
        }

        // 拡大の中心
        [PropertyMember]
        public DragControlCenter ScaleCenter
        {
            get { return _scaleCenter; }
            set { SetProperty(ref _scaleCenter, value); }
        }

        // 反転の中心
        [PropertyMember]
        public DragControlCenter FlipCenter
        {
            get { return _flipCenter; }
            set { SetProperty(ref _flipCenter, value); }
        }

        // 拡大率キープ
        [PropertyMember]
        public bool IsKeepScale
        {
            get { return _isKeepScale; }
            set { SetProperty(ref _isKeepScale, value); }
        }

        // ブック間の拡大率キープ
        [PropertyMember]
        public bool IsKeepScaleBooks
        {
            get { return _isKeepScakeBooks; }
            set { SetProperty(ref _isKeepScakeBooks, value); }
        }

        // 回転キープ
        [PropertyMember]
        public bool IsKeepAngle
        {
            get { return _isKeepAngle; }
            set { SetProperty(ref _isKeepAngle, value); }
        }

        // ブック間の回転キープ
        [PropertyMember]
        public bool IsKeepAngleBooks
        {
            get { return _isKeepAngleBooks; }
            set { SetProperty(ref _isKeepAngleBooks, value); }
        }

        // 反転キープ
        [PropertyMember]
        public bool IsKeepFlip
        {
            get { return _isKeepFlip; }
            set { SetProperty(ref _isKeepFlip, value); }
        }

        // ブック間の反転キープ
        [PropertyMember]
        public bool IsKeepFlipBooks
        {
            get { return _isKeepFlipBooks; }
            set { SetProperty(ref _isKeepFlipBooks, value); }
        }

        // 表示開始時の基準
        [PropertyMember]
        public bool IsViewStartPositionCenter
        {
            get { return _isViewStartPositionCenter; }
            set { SetProperty(ref _isViewStartPositionCenter, value); }
        }

        // 回転スナップ。0で無効
        [PropertyMember]
        public double AngleFrequency
        {
            get { return _angleFrequency; }
            set { SetProperty(ref _angleFrequency, value); }
        }

        // ウィンドウ枠内の移動に制限する
        [PropertyMember]
        public bool IsLimitMove
        {
            get { return _isLimitMove; }
            set { SetProperty(ref _isLimitMove, value); }
        }

        // スケールモード
        [PropertyMember]
        public PageStretchMode StretchMode
        {
            get { return _stretchMode; }
            set { SetProperty(ref _stretchMode, value); }
        }

        // スケールモード・拡大許可
        [PropertyMember]
        public bool AllowStretchScaleUp
        {
            get { return _allowStretchScaleUp; }
            set { SetProperty(ref _allowStretchScaleUp, value); }
        }

        // スケールモード・縮小許可
        [PropertyMember]
        public bool AllowStretchScaleDown
        {
            get { return _allowStretchScaleDown; }
            set { SetProperty(ref _allowStretchScaleDown, value); }
        }

        // 基底スケール有効
        [PropertyMember]
        public bool IsBaseScaleEnabled
        {
            get { return _isBaseScaleEnabled; }
            set { SetProperty(ref _isBaseScaleEnabled, value); }
        }

        // 基底スケール
        [PropertyPercent(0.1, 2.0, TickFrequency = 0.01)]
        public double BaseScale
        {
            get { return _baseScale; }
            set { SetProperty(ref _baseScale, Math.Max(value, 0.0)); }
        }

        // 自動回転左/右
        [PropertyMember]
        public AutoRotateType AutoRotate
        {
            get { return _autoRotate; }
            set { SetProperty(ref _autoRotate, value); }
        }

        // ナビゲーターボタンによる回転にストレッチを適用
        [PropertyMember]
        public bool IsRotateStretchEnabled
        {
            get { return _isRotateStretchEnabled; }
            set { SetProperty(ref _isRotateStretchEnabled, value); }
        }

        // ビューエリアの余白
        [PropertyRange(0.0, 100.0)]
        public double MainViewMergin
        {
            get { return _mainViewMergin; }
            set { SetProperty(ref _mainViewMergin, value); }
        }
    }

}