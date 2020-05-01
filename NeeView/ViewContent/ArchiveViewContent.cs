using System.Windows;

namespace NeeView
{
    public class ArchiveViewContent : ViewContent
    {
        public ArchiveViewContent(ViewContentSource source) : base(source)
        {
        }


        public override bool IsBitmapScalingModeSupported => false;


        public void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = new ViewContentControl(CreateView(this.Source, parameter));

            // content setting
            this.Size = new Size(512, 512);
        }

        private FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
        {
            var control = new ArchivePageControl(source.Content as ArchiveContent);
            control.SetBinding(ArchivePageControl.DefaultBrushProperty, parameter.ForegroundBrush);
            return control;
        }


        public static ArchiveViewContent Create(ViewContentSource source)
        {
            var viewContent = new ArchiveViewContent(source);
            viewContent.Initialize();
            return viewContent;
        }
    }

}
