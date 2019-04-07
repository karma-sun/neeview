using System.Windows;

namespace NeeView
{
    public class ArchiveViewContent : ViewContent
    {
        #region Constructors

        public ArchiveViewContent(ViewContentSource source) : base(source)
        {
        }

        #endregion

        #region Methods

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

        public override bool IsBitmapScalingModeSupported() => false;

        #endregion

        #region Utility

        public static ArchiveViewContent Create(ViewContentSource source)
        {
            var viewContent = new ArchiveViewContent(source);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }

}
