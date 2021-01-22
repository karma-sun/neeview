using System.Windows;

namespace NeeView
{
    public class ArchiveViewContent : ViewContent
    {
        public ArchiveViewContent(MainViewComponent viewComponent, ViewContentSource source) : base(viewComponent, source)
        {
        }


        public override bool IsBitmapScalingModeSupported => false;


        private void Initialize()
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


        public static ArchiveViewContent Create(MainViewComponent viewComponent, ViewContentSource source)
        {
            var viewContent = new ArchiveViewContent(viewComponent, source);
            viewContent.Initialize();
            return viewContent;
        }
    }

}
