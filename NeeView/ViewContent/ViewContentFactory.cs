// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


namespace NeeView
{
    /// <summary>
    /// ViewContent Factory
    /// </summary>
    public class ViewContentFactory
    {
        public static ViewContent Create(ViewContentSource source, ViewContent oldViewContent)
        {
            switch (source.GetContentType())
            {
                case ViewContentType.Message:
                    return MessageViewContent.Create(source, oldViewContent);
                case ViewContentType.Thumbnail:
                    return ThumbnailViewContent.Create(source, oldViewContent);
                case ViewContentType.Bitmap:
                    return BitmapViewContent.Create(source, oldViewContent);
                case ViewContentType.Anime:
                    return AnimatedViewContent.Create(source, oldViewContent);
                case ViewContentType.Pdf:
                    return PdfViewContent.Create(source, oldViewContent);
                default:
                    return new ViewContent();
            }
        }
    }
}
