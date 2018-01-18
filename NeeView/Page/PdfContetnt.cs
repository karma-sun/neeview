// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


namespace NeeView
{
    /// <summary>
    /// PDFページコンテンツ
    /// 今のところ画像コンテンツと同じ
    /// </summary>
    public class PdfContetnt : BitmapContent
    {
        public PdfContetnt(ArchiveEntry entry) : base(entry)
        {
        }
    }
}
