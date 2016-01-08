using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    // 表示コンテンツ
    public class ViewContent
    {
        public object Content { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Color Color { get; set; }

        public ViewContent(Page page)
        {
            Content = page.Content;
            Width = page.Width;
            Height = page.Height;
            Color = page.Color;
        }
    }

}
