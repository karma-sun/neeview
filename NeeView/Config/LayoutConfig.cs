using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;

namespace NeeView
{
    public class LayoutConfig : BindableBase
    {
        public ThemeConfig Theme { get; set; } = new ThemeConfig();

        public BackgroundConfig Background { get; set; } = new BackgroundConfig();

        public AutoHideConfig AutoHide { get; set; } = new AutoHideConfig();

        public NoticeConfig Notice { get; set; } = new NoticeConfig();

        public SidePanelsConfig SidePanels { get; set; } = new SidePanelsConfig();

        public MenuBarConfig MenuBar { get; set; } = new MenuBarConfig();

        public SliderConfig Slider { get; set; } = new SliderConfig();
    }


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


