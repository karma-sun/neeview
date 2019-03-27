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
        public static ViewContent Create(ViewPage source, ViewContent oldViewContent)
        {
            ViewContent viewContent = null;

            switch (source.GetContentType())
            {
                case ViewContentType.Message:
                    viewContent = MessageViewContent.Create(source, oldViewContent);
                    break;
                case ViewContentType.Reserve:
                    viewContent =  ReserveViewContent.Create(source, oldViewContent);
                    break;
                case ViewContentType.Bitmap:
                    viewContent = BitmapViewContent.Create(source, oldViewContent);
                    // ここでBitmapのないviewContentになってしまう場合、ここまでの処理中に非同期でPageがUnloadされた可能性がある(高速ページ送り等)。
                    // TODO: わたされたsourceはPageUnloadにかかわらず不変であるものが望ましい
                    if (viewContent.GetViewBrush() == null)
                    {
                        throw new ViewContentFactoryException("This page has already been unloaded. maybe.");
                    }
                    break;
                case ViewContentType.Anime:
                    viewContent = AnimatedViewContent.Create(source, oldViewContent);
                    break;
                case ViewContentType.Media:
                    viewContent = MediaViewContent.Create(source, oldViewContent);
                    break;
                case ViewContentType.Pdf:
                    viewContent = PdfViewContent.Create(source, oldViewContent);
                    break;
                case ViewContentType.Archive:
                    viewContent = ArchiveViewContent.Create(source, oldViewContent);
                    break;
                default:
                    viewContent = new ViewContent();
                    break;
            }

            viewContent.Reserver = viewContent.CreateReserver();
            return viewContent;
        }
    }

    /// <summary>
    /// ViewContentが生成できなかったときの例外
    /// </summary>
    [Serializable]
    public class ViewContentFactoryException : Exception
    {
        public ViewContentFactoryException()
        {
        }

        public ViewContentFactoryException(string message) : base(message)
        {
        }

        public ViewContentFactoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ViewContentFactoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
