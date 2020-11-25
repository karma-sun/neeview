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
        public static ViewContent Create(ViewComponent viewComponent, ViewContentSource source, ViewContent oldViewContent)
        {
            ViewContent viewContent;

            switch (source.GetContentType())
            {
                case ViewContentType.Dummy:
                    viewContent = DummyViewContent.Create(viewComponent, source);
                    break;
                case ViewContentType.Message:
                    viewContent = MessageViewContent.Create(viewComponent, source);
                    break;
                case ViewContentType.Reserve:
                    viewContent =  ReserveViewContent.Create(viewComponent, source, oldViewContent);
                    break;
                case ViewContentType.Bitmap:
                    viewContent = BitmapViewContent.Create(viewComponent, source);
                    break;
                case ViewContentType.Anime:
                    viewContent = AnimatedViewContent.Create(viewComponent, source);
                    break;
                case ViewContentType.Media:
                    viewContent = MediaViewContent.Create(viewComponent, source);
                    break;
                case ViewContentType.Pdf:
                    viewContent = PdfViewContent.Create(viewComponent, source);
                    break;
                case ViewContentType.Archive:
                    viewContent = ArchiveViewContent.Create(viewComponent, source);
                    break;
                default:
                    viewContent = new ViewContent();
                    break;
            }

            return viewContent;
        }
    }

}
