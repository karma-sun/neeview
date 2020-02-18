using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    public class ViewImageExporter : IImageExporter
    {
        private ExportImageSource _source;

        public bool HasBackground { get; set; }

        public ViewImageExporter(ExportImageSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public ImageExporterContent CreateView()
        {
            if (_source == null) return null;

            var grid = new Grid();

            if (HasBackground)
            {
                grid.Background = _source.Background;

                var backgroundFront = new Rectangle();
                backgroundFront.HorizontalAlignment = HorizontalAlignment.Stretch;
                backgroundFront.VerticalAlignment = VerticalAlignment.Stretch;
                backgroundFront.Fill = _source.BackgroundFront;
                RenderOptions.SetBitmapScalingMode(backgroundFront, BitmapScalingMode.HighQuality);
                grid.Children.Add(backgroundFront);
            }

            var rectangle = new Rectangle();
            rectangle.Width = _source.View.ActualWidth;
            rectangle.Height = _source.View.ActualHeight;
            var brush = new VisualBrush(_source.View);
            brush.Stretch = Stretch.None;
            rectangle.Fill = brush;
            rectangle.LayoutTransform = _source.ViewTransform;
            rectangle.Effect = _source.ViewEffect;
            grid.Children.Add(rectangle);

            // 描画サイズ取得
            var rect = new Rect(0, 0, rectangle.Width, rectangle.Height);
            rect = _source.ViewTransform.TransformBounds(rect);

            return new ImageExporterContent(grid, rect.Size);
        }

        public void Export(string path, bool isOverwrite, int qualityLevel)
        {
            var bitmapSource = CreateBitmapSource();

            var fileMode = isOverwrite ? FileMode.Create : FileMode.CreateNew;

            using (FileStream stream = new FileStream(path, fileMode))
            {
                // 出力ファイル名からフォーマットを決定する
                if (System.IO.Path.GetExtension(path).ToLower() == ".png")
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(stream);
                }
                else
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.QualityLevel = qualityLevel;
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(stream);
                }
            }
        }

        private BitmapSource CreateBitmapSource()
        {
            if (_source.View == null) throw new InvalidOperationException();

            var canvas = new Canvas();

            var content = CreateView();
            canvas.Children.Add(content.View);

            // calc content size
            UpdateElementLayout(canvas, new Size(256, 256));
            var rect = new Rect(0, 0, content.View.ActualWidth, content.View.ActualHeight);
            canvas.Width = rect.Width;
            canvas.Height = rect.Height;

            UpdateElementLayout(canvas, rect.Size);

            double dpi = 96.0;
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(canvas);

            canvas.Children.Clear(); // コンテンツ開放

            return bmp;
        }

        private void UpdateElementLayout(FrameworkElement element, Size size)
        {
            element.Measure(size);
            element.Arrange(new Rect(size));
            element.UpdateLayout();
        }

        public string CreateFileName()
        {
            var bookName = LoosePath.ValidFileName(LoosePath.GetFileNameWithoutExtension(_source.BookAddress));
            var indexLabel = (_source.Pages.Count > 1) ? $"{_source.Pages[0].Index:000}-{_source.Pages[1].Index:000}" : $"{_source.Pages[0].Index:000}";
            return $"{bookName}_{indexLabel}.png";
        }
    }
}
