using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class NoticeConfig : BindableBase
    {
        [PropertyMember("@ParamInfoMessageNoticeShowMessageStyle")]
        public ShowMessageStyle NoticeShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoBookNameShowMessageStyle")]
        public ShowMessageStyle BookNameShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoMessageCommandShowMessageStyle")]
        public ShowMessageStyle CommandShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoMessageGestureShowMessageStyle")]
        public ShowMessageStyle GestureShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoMessageNowLoadingShowMessageStyle")]
        public ShowMessageStyle NowLoadingShowMessageStyle { get; set; } = ShowMessageStyle.Normal;

        [PropertyMember("@ParamInfoMessageViewTransformShowMessageStyle")]
        public ShowMessageStyle ViewTransformShowMessageStyle { get; set; } = ShowMessageStyle.None;

        // View変換情報表示のスケール表示をオリジナルサイズ基準にする
        [PropertyMember("@ParamDragTransformIsOriginalScaleShowMessage", Tips = "@ParamDragTransformIsOriginalScaleShowMessageTips")]
        public bool IsOriginalScaleShowMessage { get; set; }
    }
}


