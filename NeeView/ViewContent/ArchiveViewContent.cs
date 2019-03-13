using System.Windows;

namespace NeeView
{
    public class ArchiveViewContent : ViewContent
    {
        #region Constructors

        public ArchiveViewContent(ViewPage source, ViewContent old) : base(source, old)
        {
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = CreateView(this.Source, parameter);

            // content setting
            this.Size = new Size(512, 512);
        }

        private FrameworkElement CreateView(ViewPage source, ViewContentParameters parameter)
        {
            var control = new ArchivePageControl(source.Page.Content as ArchiveContent);
            control.SetBinding(ArchivePageControl.DefaultBrushProperty, parameter.ForegroundBrush);
            return control;
        }

        public override bool IsBitmapScalingModeSupported() => false;

        #endregion

        #region Utility

        public static ArchiveViewContent Create(ViewPage source, ViewContent oldViewContent)
        {
            var viewContent = new ArchiveViewContent(source, oldViewContent);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }

}
