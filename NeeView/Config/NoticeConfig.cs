using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class NoticeConfig : BindableBase
    {
        private ShowMessageStyle _noticeShowMessageStyle = ShowMessageStyle.Normal;
        private ShowMessageStyle _bookNameShowMessageStyle = ShowMessageStyle.Normal;
        private ShowMessageStyle _commandShowMessageStyle = ShowMessageStyle.Normal;
        private ShowMessageStyle _gestureShowMessageStyle = ShowMessageStyle.Normal;
        private ShowMessageStyle _nowLoadingShowMessageStyle = ShowMessageStyle.Normal;
        private ShowMessageStyle _viewTransformShowMessageStyle = ShowMessageStyle.None;
        private bool _isOriginalScaleShowMessage;
        private bool _isBusyMarkEnabled = true;


        [PropertyMember("@ParamInfoMessageNoticeShowMessageStyle")]
        public ShowMessageStyle NoticeShowMessageStyle
        {
            get { return _noticeShowMessageStyle; }
            set { SetProperty(ref _noticeShowMessageStyle, value); }
        }

        [PropertyMember("@ParamInfoBookNameShowMessageStyle")]
        public ShowMessageStyle BookNameShowMessageStyle
        {
            get { return _bookNameShowMessageStyle; }
            set { SetProperty(ref _bookNameShowMessageStyle, value); }
        }

        [PropertyMember("@ParamInfoMessageCommandShowMessageStyle")]
        public ShowMessageStyle CommandShowMessageStyle
        {
            get { return _commandShowMessageStyle; }
            set { SetProperty(ref _commandShowMessageStyle, value); }
        }

        [PropertyMember("@ParamInfoMessageGestureShowMessageStyle")]
        public ShowMessageStyle GestureShowMessageStyle
        {
            get { return _gestureShowMessageStyle; }
            set { SetProperty(ref _gestureShowMessageStyle, value); }
        }

        [PropertyMember("@ParamInfoMessageNowLoadingShowMessageStyle")]
        public ShowMessageStyle NowLoadingShowMessageStyle
        {
            get { return _nowLoadingShowMessageStyle; }
            set { SetProperty(ref _nowLoadingShowMessageStyle, value); }
        }

        [PropertyMember("@ParamInfoMessageViewTransformShowMessageStyle")]
        public ShowMessageStyle ViewTransformShowMessageStyle
        {
            get { return _viewTransformShowMessageStyle; }
            set { SetProperty(ref _viewTransformShowMessageStyle, value); }
        }

        // View変換情報表示のスケール表示をオリジナルサイズ基準にする
        [PropertyMember("@ParamDragTransformIsOriginalScaleShowMessage", Tips = "@ParamDragTransformIsOriginalScaleShowMessageTips")]
        public bool IsOriginalScaleShowMessage
        {
            get { return _isOriginalScaleShowMessage; }
            set { SetProperty(ref _isOriginalScaleShowMessage, value); }
        }

        /// <summary>
        /// 非同期のページ読込処理中のマークを表示する
        /// </summary>
        [PropertyMember("@ParamIsVisibleBusy")]
        public bool IsBusyMarkEnabled
        {
            get { return _isBusyMarkEnabled; }
            set { SetProperty(ref _isBusyMarkEnabled, value); }
        }

    }
}


