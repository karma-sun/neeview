using System.Windows;

namespace NeeView
{
    public class ArchivePageContent
    {
        public ArchiveEntry Entry { get; set; }
        public Thumbnail Thumbnail { get; set; }
    }

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
            var content = new ArchivePageContent()
            {
                Entry = source.Page.Entry,
                Thumbnail = source.Page.Thumbnail,
            };

            var control = new ArchivePageControl(content);
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
