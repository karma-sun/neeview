// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// ViewContent Factory
    /// </summary>
    public class ViewContentFactory
    {
        public static ViewContent Create(ViewContentSource source, ViewContent oldViewContent)
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
}
