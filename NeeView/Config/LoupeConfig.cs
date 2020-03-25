using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    public class LoupeConfig : BindableBase
    {
        private double _defaultScale = 2.0;
        private bool _isLoupeCenter;
        private double _minimumScale = 2.0;
        private double _maximumScale = 10.0;
        private double _scaleStep = 1.0;
        private bool _isResetByRestart = false;
        private bool _isResetByPageChanged = true;
        private bool _isVisibleLoupeInfo = true;


        [PropertyMember("@ParamLoupeIsLoupeCenter")]
        public bool IsLoupeCenter
        {
            get { return _isLoupeCenter; }
            set { SetProperty(ref _isLoupeCenter, value); }
        }

        [PropertyRange("@ParamLoupeMinimumScale", 1, 20, TickFrequency = 1.0, IsEditable = true)]
        public double MinimumScale
        {
            get { return _minimumScale; }
            set { SetProperty(ref _minimumScale, value); }
        }

        [PropertyRange("@ParamLoupeMaximumScale", 1, 20, TickFrequency = 1.0, IsEditable = true)]
        public double MaximumScale
        {
            get { return _maximumScale; }
            set { SetProperty(ref _maximumScale, value); }
        }

        [PropertyRange("@ParamLoupeDefaultScale", 1, 20, TickFrequency = 1.0, IsEditable = true)]
        public double DefaultScale
        {
            get { return _defaultScale; }
            set { SetProperty(ref _defaultScale, value); }
        }

        [PropertyRange("@ParamLoupeScaleStep", 0.1, 5.0, TickFrequency = 0.1, IsEditable = true)]
        public double ScaleStep
        {
            get { return _scaleStep; }
            set { SetProperty(ref _scaleStep, Math.Max(value, 0.0)); }
        }

        [PropertyMember("@ParamLoupeIsResetByRestart", Tips = "@ParamLoupeIsResetByRestartTips")]
        public bool IsResetByRestart
        {
            get { return _isResetByRestart; }
            set { SetProperty(ref _isResetByRestart, value); }
        }

        [PropertyMember("@ParamLoupeIsResetByPageChanged")]
        public bool IsResetByPageChanged
        {
            get { return _isResetByPageChanged; }
            set { SetProperty(ref _isResetByPageChanged, value); }
        }

        [PropertyMember("@ParamLoupeIsWheelScalingEnabled", Tips = "@ParamLoupeIsWheelScalingEnabledTips")]
        public bool IsWheelScalingEnabled { get; set; } = true;

        [PropertyRange("@ParamLoupeSpeed", 0.0, 10.0, TickFrequency = 0.1, Format = "×{0:0.0}")]
        public double Speed { get; set; } = 1.0;

        [PropertyMember("@ParamLoupeIsEscapeKeyEnabled")]
        public bool IsEscapeKeyEnabled { get; set; } = true;


        [PropertyMember("@ParamLoupeIsVisibleLoupeInfo", Tips = "@ParamLoupeIsVisibleLoupeInfoTips")]
        public bool IsVisibleLoupeInfo
        {
            get { return _isVisibleLoupeInfo; }
            set { SetProperty(ref _isVisibleLoupeInfo, value); }
        }
    }

}