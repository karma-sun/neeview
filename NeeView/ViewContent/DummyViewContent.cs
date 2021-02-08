using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class DummyViewContent : ViewContent
    {
        public DummyViewContent(MainViewComponent viewComponent, ViewContentSource source) : base(viewComponent, source)
        {
        }


        public override bool IsBitmapScalingModeSupported => false;

        public override bool IsInformationValid => false;

        private void Initialize()
        {
            this.View = new ViewContentControl(CreateView());
        }

        private FrameworkElement CreateView()
        {
            return new Grid() { Background = new SolidColorBrush(Color.FromArgb(0x20, 0x80, 0x80, 0x80)) };
        }

        public static DummyViewContent Create(MainViewComponent viewComponent, ViewContentSource source)
        {
            var viewContent = new DummyViewContent(viewComponent, source);
            viewContent.Initialize();
            return viewContent;
        }
    }
}
