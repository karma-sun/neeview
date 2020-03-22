using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class AutoHideConfig : BindableBase
    {
        private double _autoHideDelayTime = 1.0;
        private double _autoHideDelayVisibleTime = 0.0;
        private AutoHideFocusLockMode _autoHideFocusLockMode = AutoHideFocusLockMode.LogicalTextBoxFocusLock;
        private bool _isAutoHideKeyDownDelay = true;
        private double _autoHideHitTestMargin = 32.0;

        // パネルやメニューが自動的に消えるまでの時間(秒)
        [PropertyMember("@ParamAutoHideDelayTime")]
        public double AutoHideDelayTime
        {
            get { return _autoHideDelayTime; }
            set { if (SetProperty(ref _autoHideDelayTime, value)) { RaisePropertyChanged(nameof(AutoHideDelayTimeMillisecond)); } }
        }

        // パネルやメニューが自動的に消えるまでの時間(ミリ秒)
        public double AutoHideDelayTimeMillisecond
        {
            get { return _autoHideDelayTime * 1000.0; }
        }


        // パネルやメニューが自動的に消えるまでの時間(秒)
        [PropertyMember("@ParamAutoHideDelayVisibleTime")]
        public double AutoHideDelayVisibleTime
        {
            get { return _autoHideDelayVisibleTime; }
            set { if (SetProperty(ref _autoHideDelayVisibleTime, value)) { RaisePropertyChanged(nameof(AutoHideDelayVisibleTimeMillisecond)); } }
        }

        // パネルやメニューが自動的に消えるまでの時間(ミリ秒)
        public double AutoHideDelayVisibleTimeMillisecond
        {
            get { return _autoHideDelayVisibleTime * 1000.0; }
        }


        // パネル自動非表示のフォーカス挙動モード
        [PropertyMember("@AutoHideFocusLockMode", Tips = "@AutoHideFocusLockModeTips")]
        public AutoHideFocusLockMode AutoHideFocusLockMode
        {
            get { return _autoHideFocusLockMode; }
            set { SetProperty(ref _autoHideFocusLockMode, value); }
        }

        // パネル自動非表示のキー入力遅延
        [PropertyMember("@IsAutoHideKeyDownDelay", Tips = "@IsAutoHideKeyDownDelayTips")]
        public bool IsAutoHideKeyDownDelay
        {
            get { return _isAutoHideKeyDownDelay; }
            set { SetProperty(ref _isAutoHideKeyDownDelay, value); }
        }

        // パネル自動非表示の表示判定マージン
        [PropertyMember("@ParamSidePanelHitTestMargin")]
        public double AutoHideHitTestMargin
        {
            get { return _autoHideHitTestMargin; }
            set { SetProperty(ref _autoHideHitTestMargin, value); }
        }
    }
}