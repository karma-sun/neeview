using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像ページ
    /// </summary>
    public class BitmapPage : Page
    {
        public BitmapPage(string bookPrefix, ArchiveEntry entry) : base(bookPrefix, entry)
        {
            Content = new BitmapContent(entry);
            Content.Loaded += (s, e) => Loaded?.Invoke(this, null);
        } 
    }

    /// <summary>
    /// アニメーション画像ページ
    /// </summary>
    public class AnimatedPage : Page
    {
        public AnimatedPage(string bookPrefix, ArchiveEntry entry) : base(bookPrefix, entry)
        {
            Content = new AnimatedContent(entry);
            Content.Loaded += (s, e) => Loaded?.Invoke(this, null);
        }
    }

    /// <summary>
    /// MediaPlayer ページ
    /// </summary>
    public class MediaPage : Page
    {
        public MediaPage(string bookPrefix, ArchiveEntry entry) : base(bookPrefix, entry)
        {
            Content = new MediaContent(entry);
            Content.Loaded += (s, e) => Loaded?.Invoke(this, null);
        }

        public bool IsLastStart { get; set; }
    }

    /// <summary>
    /// PDFページ
    /// </summary>
    public class PdfPage : Page
    {
        public PdfPage(string bookPrefix, ArchiveEntry entry) : base(bookPrefix, entry)
        {
            Content = new PdfContetnt(entry);
            Content.Loaded += (s, e) => Loaded?.Invoke(this, null);
        }
    }
    

}
