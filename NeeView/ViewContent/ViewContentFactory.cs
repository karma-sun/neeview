using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// ViewContent Factory
    /// </summary>
    public class ViewContentFactory
    {
        public static ViewContent Create(ViewContentSource source, ViewContent oldViewContent)
        {
            ViewContent viewContent;

            switch (source.GetContentType())
            {
                case ViewContentType.Dummy:
                    viewContent = DummyViewContent.Create(source);
                    break;
                case ViewContentType.Message:
                    viewContent = MessageViewContent.Create(source);
                    break;
                case ViewContentType.Reserve:
                    viewContent =  ReserveViewContent.Create(source, oldViewContent);
                    break;
                case ViewContentType.Bitmap:
                    viewContent = BitmapViewContent.Create(source);
                    break;
                case ViewContentType.Anime:
                    viewContent = AnimatedViewContent.Create(source);
                    break;
                case ViewContentType.Media:
                    viewContent = MediaViewContent.Create(source);
                    break;
                case ViewContentType.Pdf:
                    viewContent = PdfViewContent.Create(source);
                    break;
                case ViewContentType.Archive:
                    viewContent = ArchiveViewContent.Create(source);
                    break;
                default:
                    viewContent = new ViewContent();
                    break;
            }

            return viewContent;
        }
    }

}
