using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// Pdf ViewContent
    /// </summary>
    public class PdfViewContent : BitmapViewContent
    {
        public PdfViewContent(ViewContentSource source) : base(source)
        {
        }


        public new void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = new ViewContentControl(CreateView(this.Source, parameter));

            // content setting
            var bitmapContent = this.Content as BitmapContent;
            this.Color = bitmapContent.Color;
        }

        public override bool Rebuild(double scale)
        {
            var size = GetScaledSize(scale);
            return Rebuild(size);
        }


        public new static PdfViewContent Create(ViewContentSource source)
        {
            var viewContent = new PdfViewContent(source);
            viewContent.Initialize();
            return viewContent;
        }
    }
}
