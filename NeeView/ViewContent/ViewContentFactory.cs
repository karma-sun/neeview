// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


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
                case ViewContentType.Thumbnail:
                    viewContent =  ThumbnailViewContent.Create(source, oldViewContent);
                    break;
                case ViewContentType.Bitmap:
                    viewContent = BitmapViewContent.Create(source, oldViewContent);
                    // ここでBitmapのないviewContentになってしまう場合、ここまでの処理中に非同期でPageがUnloadされた可能性がある(高速ページ送り等)。
                    // TODO: わたされたsourceはPageUnloadにかかわらず不変であるものが望ましい
                    if (viewContent.GetViewBrush() == null)
                    {
                        throw new ViewContentFactoryException("このページはすでにアンロードされています。たぶん。");
                    }
                    break;
                case ViewContentType.Anime:
                    viewContent = AnimatedViewContent.Create(source, oldViewContent);
                    break;
                case ViewContentType.Pdf:
                    viewContent = PdfViewContent.Create(source, oldViewContent);
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
