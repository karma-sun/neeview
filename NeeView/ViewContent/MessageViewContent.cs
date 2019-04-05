using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// Message ViewContent
    /// </summary>
    public class MessageViewContent : ViewContent
    {
        #region Constructors

        public MessageViewContent(ViewPage source) : base(source)
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
            this.Size = new Size(480, 480);
        }

        //
        private FrameworkElement CreateView(ViewPage source, ViewContentParameters parameter)
        {
            if (Content.PageMessage == null)
            {
                Debug.WriteLine("Warning: Content.PageMessage is null");
                return null;
            }

            var filepage = new FilePageContent()
            {
                Icon = Content.PageMessage.Icon,
                FileName = Content.Entry.EntryName,
                Message = Content.PageMessage.Message,
            };

            var control = new FilePageControl(filepage);
            control.SetBinding(FilePageControl.DefaultBrushProperty, parameter.ForegroundBrush);
            return control;
        }

        //
        public override bool IsBitmapScalingModeSupported() => false;

        #endregion
    
        #region Static Methods

        public static MessageViewContent Create(ViewPage source)
        {
            var viewContent = new MessageViewContent(source);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }
}
