// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps;

namespace NeeView
{
    public class PrintContext
    {
        public ViewContent MainContent { get; set; }
        public BitmapSource RawImage => (MainContent.Source.Page.Content as BitmapContent).BitmapSource;

        public IEnumerable<ViewContent> Contents { get; set; }
        public Thickness ContentMargin { get; set; }

        public FrameworkElement View { get; set; }

        public Transform ViewTransform { get; set; }
        public double ViewWidth { get; set; }
        public double ViewHeight { get; set; }
        public Effect ViewEffect { get; set; }

        public Brush Background { get; set; }
    }

    public enum PrintMode
    {
        RawImage,
        View,
        ViewFill,
        ViewStretch,
    }

    public class PrintModel : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //
        private PrintDialog _printDialog;

        private PageImageableArea _area;


        /// <summary>
        /// PageOrientation property.
        /// </summary>
        private PageOrientation _PageOrientation;
        public PageOrientation PageOrientation
        {
            get { return _PageOrientation; }
            set
            {
                if (_PageOrientation != value)
                {
                    switch (value)
                    {
                        default:
                            _PageOrientation = PageOrientation.Portrait;
                            break;
                        case PageOrientation.Landscape:
                        case PageOrientation.ReverseLandscape:
                            _PageOrientation = PageOrientation.Landscape;
                            break;
                    }
                    UpdatePrintOrientation();
                    RaisePropertyChanged();
                }
            }
        }

        //
        public static Dictionary<PageOrientation, string> PageOrientationList { get; } = new Dictionary<PageOrientation, string>()
        {
            [PageOrientation.Portrait] = "縦",
            [PageOrientation.Landscape] = "横"
        };

        private void UpdatePrintOrientation()
        {
            PrintCapabilities printCapabilites = _printDialog.PrintQueue.GetPrintCapabilities();
            if (printCapabilites.PageOrientationCapability.Contains(PageOrientation))
            {
                _printDialog.PrintTicket.PageOrientation = PageOrientation;
            }
        }


        /// <summary>
        /// PrintMode property.
        /// </summary>
        private PrintMode _PrintMode = PrintMode.View;
        public PrintMode PrintMode
        {
            get { return _PrintMode; }
            set { if (_PrintMode != value) { _PrintMode = value; RaisePropertyChanged(); } }
        }

        //
        public Dictionary<PrintMode, string> PrintModeList { get; } = new Dictionary<PrintMode, string>()
        {
            [PrintMode.RawImage] = "画像を印刷", // 元画像、メインページのみ
            [PrintMode.View] = "表示を印刷",
            [PrintMode.ViewFill] = "用紙サイズで表示を印刷",
            [PrintMode.ViewStretch] = "全体の表示を印刷",
        };



        /// <summary>
        /// IsBackground property.
        /// </summary>
        private bool _IsBackground;
        public bool IsBackground
        {
            get { return _IsBackground; }
            set { if (_IsBackground != value) { _IsBackground = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsDotScale property.
        /// </summary>
        private bool _IsDotScale;
        public bool IsDotScale
        {
            get { return _IsDotScale; }
            set { if (_IsDotScale != value) { _IsDotScale = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// Preview property.
        /// </summary>
        private FrameworkElement _Preview;
        public FrameworkElement Preview
        {
            get { return _Preview; }
            set { if (_Preview != value) { _Preview = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// PrintQueue property.
        /// </summary>
        private PrintQueue _PrintQueue;
        public PrintQueue PrintQueue
        {
            get { return _PrintQueue; }
            set { if (_PrintQueue != value) { _PrintQueue = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Columns property.
        /// </summary>
        private int _Columns = 1;
        public int Columns
        {
            get { return _Columns; }
            set { if (_Columns != value) { _Columns = NVUtility.Clamp(value, 1, 4); RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Rows property.
        /// </summary>
        private int _Rows = 1;
        public int Rows
        {
            get { return _Rows; }
            set { if (_Rows != value) { _Rows = NVUtility.Clamp(value, 1, 4); ; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// HorizontalAlignment property.
        /// </summary>
        private HorizontalAlignment _HorizontalAlignment = HorizontalAlignment.Center;
        public HorizontalAlignment HorizontalAlignment
        {
            get { return _HorizontalAlignment; }
            set { if (_HorizontalAlignment != value) { _HorizontalAlignment = value; RaisePropertyChanged(); } }
        }

        public Dictionary<HorizontalAlignment, string> HorizontalAlignmentList { get; } = new Dictionary<HorizontalAlignment, string>()
        {
            [HorizontalAlignment.Left] = "左詰め",
            [HorizontalAlignment.Center] = "中央",
            [HorizontalAlignment.Right] = "右詰め",
        };

        /// <summary>
        /// VerticalAlignment property.
        /// </summary>
        private VerticalAlignment _VerticalAlignment = VerticalAlignment.Center;
        public VerticalAlignment VerticalAlignment
        {
            get { return _VerticalAlignment; }
            set { if (_VerticalAlignment != value) { _VerticalAlignment = value; RaisePropertyChanged(); } }
        }

        public Dictionary<VerticalAlignment, string> VerticalAlignmentList { get; } = new Dictionary<VerticalAlignment, string>()
        {
            [VerticalAlignment.Top] = "上詰め",
            [VerticalAlignment.Center] = "中央",
            [VerticalAlignment.Bottom] = "下詰め",
        };

        /// <summary>
        /// Margin property.
        /// </summary>
        private Margin _Margin = new Margin();
        public Margin Margin
        {
            get { return _Margin; }
            set { if (_Margin != value) { _Margin = value; RaisePropertyChanged(); } }
        }

        //
        private double MillimeterToPixel(double mm)
        {
            // ミリメートルをインチに変換
            var inch = mm * 0.039370;

            // インチをピクセルに変換
            var pixel = inch * 96.0;

            return pixel;
        }


        //
        PrintContext _context;


        //
        public PrintModel(PrintContext context)
        {
            //_Margin.PropertyChanged += (s, e) => RaisePropertyChanged(nameof(Margin));

            _context = context;
            _printDialog = new PrintDialog();

            UpdatePrintDialog();
        }


        /// <summary>
        /// 印刷ダイアログを表示して、プリンタ選択と印刷設定を行う。
        /// </summary>
        /// <returns></returns>
        public bool? ShowPrintDialog()
        {
            var result = _printDialog.ShowDialog();
            UpdatePrintDialog();
            return result;
        }


        private double _printableAreaWidth;
        private double _printableAreaHeight;

        private void UpdatePrintDialog()
        {
            // プリンター
            PrintQueue = _printDialog.PrintQueue;
            Debug.WriteLine($"Printer: {PrintQueue.FullName}");

            // 用紙の方向 (縦/横限定)
            PageOrientation = _printDialog.PrintTicket.PageOrientation ?? PageOrientation.Unknown;

            //bool isLandspace = PageOrientation == PageOrientation.Landscape;

            // 用紙の印刷可能領域
            _area = _printDialog.PrintQueue.GetPrintCapabilities().PageImageableArea;

            Debug.WriteLine($"Origin: {_area.OriginWidth}x{_area.OriginHeight}");
            Debug.WriteLine($"Extent: {_area.ExtentWidth}x{_area.ExtentHeight}");
            Debug.WriteLine($"PrintableArea: {_printDialog.PrintableAreaWidth}x{_printDialog.PrintableAreaHeight}");

            _printableAreaWidth = _printDialog.PrintableAreaWidth;
            _printableAreaHeight = _printDialog.PrintableAreaHeight;
        }



        //
        private ContentControl CreateContentControl(ViewContent content)
        {
            var control = new ContentControl();
            control.Content = content.View;
            control.Width = content.Width;
            control.Height = content.Height;
            return control;
        }

        public FrameworkElement CreateVisual()
        {
            return CreateVisualElement();
        }


        //
        public FrameworkElement CreateVisuaContextl()
        {
            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;

            foreach (var element in _context.Contents.Select(e => CreateContentControl(e)).Reverse())
            {
                element.Margin = (stackPanel.Children.Count == 0) ? new Thickness() : _context.ContentMargin;
                stackPanel.Children.Add(element);
            }

            return stackPanel;
        }

        //
        public FrameworkElement CreateRawImageContente()
        {
            //
            if (_context.RawImage == null)
            {
                return new Rectangle();
            }

            //
            var brush = new ImageBrush(_context.RawImage);
            brush.Stretch = Stretch.Fill;
            brush.TileMode = TileMode.None;

            var rectangle = new Rectangle();
            rectangle.Width = _context.RawImage.PixelWidth;
            rectangle.Height = _context.RawImage.PixelHeight;
            rectangle.Fill = brush;
            RenderOptions.SetBitmapScalingMode(rectangle, IsDotScale ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality);

            return rectangle;
        }

        //
        public FrameworkElement CreateViewContent()
        {
            // スケールモード設定
            foreach (var viewContent in _context.Contents)
            {
                viewContent.BitmapScalingMode = IsDotScale ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality;
            }

            var rectangle = new Rectangle();
            rectangle.Width = _context.View.ActualWidth;
            rectangle.Height = _context.View.ActualHeight;
            var brush = new VisualBrush(_context.View);
            brush.Stretch = Stretch.None;
            rectangle.Fill = brush;
            rectangle.RenderTransformOrigin = new Point(0.5, 0.5);
            rectangle.RenderTransform = _context.ViewTransform; // isViewTransform ? _context.ViewTransform : new TranslateTransform(0, 0);

            return rectangle;
        }



        private double _originWidth;
        private double _originHeight;

        private double _extentWidth;
        private double _extentHeight;

        private void UpdateImageableArea()
        {
            bool isLandspace = PageOrientation == PageOrientation.Landscape;
            double originWidth = isLandspace ? _area.OriginHeight : _area.OriginWidth;
            double originHeight = isLandspace ? _area.OriginWidth : _area.OriginHeight;
            double extentWidth = isLandspace ? _area.ExtentHeight : _area.ExtentWidth;
            double extentHeight = isLandspace ? _area.ExtentWidth : _area.ExtentHeight;

            double printWidth = extentWidth * Columns;
            double printHeight = extentHeight * Rows;

            // 既定の余白
            var margin = new Margin();
            margin.Left = originWidth;
            margin.Right = _printableAreaWidth - extentWidth - originWidth;
            margin.Top = originHeight;
            margin.Bottom = _printableAreaHeight - extentHeight - originHeight;

            // 余白補正
            margin.Left = Math.Max(0, margin.Left + MillimeterToPixel(Margin.Left));
            margin.Right = Math.Max(0, margin.Right + MillimeterToPixel(Margin.Right));
            margin.Top = Math.Max(0, margin.Top + MillimeterToPixel(Margin.Top));
            margin.Bottom = Math.Max(0, margin.Bottom + MillimeterToPixel(Margin.Bottom));

            // 領域補正
            _originWidth = margin.Left;
            _originHeight = margin.Top;

            _extentWidth = Math.Max(1, _printableAreaWidth - margin.Left - margin.Right);
            _extentHeight = Math.Max(1, _printableAreaHeight - margin.Top - margin.Bottom);
        }

        //
        public FrameworkElement CreateVisualElement()
        {
            //UpdatePrintDialog();

            bool isView = PrintMode != PrintMode.RawImage;

            bool isViewTransform = isView;
            bool isViewAll = PrintMode == PrintMode.ViewStretch || !isView;
            bool isViewPaperArea = isView && PrintMode == PrintMode.ViewFill;
            bool isEffect = isView;
            bool isBackground = IsBackground;

#if false
            double originWidth = 0;
            double originHeight = 0;
            double extentWidth = _printDialog.PrintableAreaWidth;
            double extentHeight = _printDialog.PrintableAreaHeight;
#else
            /*
            bool isLandspace = PageOrientation == PageOrientation.Landscape;
            double originWidth = isLandspace ? _area.OriginHeight : _area.OriginWidth;
            double originHeight = isLandspace ? _area.OriginWidth : _area.OriginHeight;
            double extentWidth = isLandspace ? _area.ExtentHeight : _area.ExtentWidth;
            double extentHeight = isLandspace ? _area.ExtentWidth : _area.ExtentHeight;

            double printWidth = extentWidth * Columns;
            double printHeight = extentHeight * Rows;
            */

            double printWidth = _extentWidth * Columns;
            double printHeight = _extentHeight * Rows;

            /*
            // 既定の余白
            var margin = new Margin();
            margin.Left = originWidth;
            margin.Right = _printableAreaWidth - extentWidth - originWidth;
            margin.Top = originHeight;
            margin.Bottom = _printableAreaHeight - extentHeight - originHeight;

            // 余白補正
            margin.Left = Math.Max(0, margin.Left + MillimeterToPixel(Margin.Left));
            margin.Right = Math.Max(0, margin.Right + MillimeterToPixel(Margin.Right));
            margin.Top = Math.Max(0, margin.Top + MillimeterToPixel(Margin.Top));
            margin.Bottom = Math.Max(0, margin.Bottom + MillimeterToPixel(Margin.Bottom));

            // 領域補正
            originWidth = margin.Left;
            originHeight = margin.Top;

            extentWidth = Math.Max(1, _printableAreaWidth - margin.Left - margin.Right);
            extentHeight = Math.Max(1, _printableAreaHeight - margin.Top - margin.Bottom);
            */
#endif

            var target = isView ? CreateViewContent() : CreateRawImageContente();

            var canvas = new Canvas();
            canvas.Width = target.Width;
            canvas.Height = target.Height;
            canvas.HorizontalAlignment = HorizontalAlignment.Center;
            canvas.VerticalAlignment = VerticalAlignment.Center;
            canvas.Children.Add(target);




            var gridClip = new Grid();
            gridClip.Name = "GridClip";
            //gridClip.Background = Brushes.LightGreen;
            gridClip.Width = _context.ViewWidth;
            gridClip.Height = _context.ViewHeight;
            gridClip.ClipToBounds = true;
            gridClip.Children.Add(canvas);
            gridClip.Background = Brushes.Transparent;

            if (isViewAll)
            {
                // 描画矩形を拡大
                var rect = new Rect(0, 0, target.Width, target.Height);
                rect.Offset(rect.Width * -0.5, rect.Height * -0.5);
                rect = target.RenderTransform.TransformBounds(rect);
                gridClip.Width = rect.Width;
                gridClip.Height = rect.Height;

                // 原点を基準値に戻す補正
                var offset = target.RenderTransform.Transform(new Point(0, 0));
                canvas.RenderTransform = new TranslateTransform(-offset.X, -offset.Y);
            }
            else if (isViewPaperArea)
            {
                //var paperAspectRatio = extentWidth / extentHeight;
                var paperAspectRatio = printWidth / printHeight;
                var viewAspectRatio = _context.ViewWidth / _context.ViewHeight;
                if (viewAspectRatio > paperAspectRatio)
                {
                    gridClip.Height = _context.ViewWidth / paperAspectRatio;

                    double offset = (gridClip.Height - _context.ViewHeight) * VerticalAlignment.Direction() * 0.5;
                    canvas.RenderTransform = new TranslateTransform(0, offset);
                }
                else
                {
                    gridClip.Width = _context.ViewHeight * paperAspectRatio;

                    double offset = (gridClip.Width - _context.ViewWidth) * HorizontalAlignment.Direction() * 0.5;
                    canvas.RenderTransform = new TranslateTransform(offset, 0);
                }

                ////canvas.VerticalAlignment = VerticalAlignment.Bottom;
            }

            var gridEffect = new Grid();
            gridEffect.Name = "GridEffect";
            gridEffect.Effect = isEffect ? _context.ViewEffect : null;
            //gridEffect.Width = gridClip.Width;
            //gridEffect.Height = gridClip.Height;
            gridEffect.Children.Add(gridClip);

            //gridEffect.Background = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x80));
            //gridEffect.VerticalAlignment = VerticalAlignment.Bottom;
            //canvas.VerticalAlignment = VerticalAlignment.Bottom;

            /*
            var grid = new Grid();
            //grid.Background = isBackground ? _context.Background : null;
            //grid.Width = _area.ExtentWidth;
            //grid.Height = _area.ExtentHeight;
            grid.Children.Add(gridEffect);
            */

            var viewbox = new Viewbox();
            //viewbox.Margin = new Thickness(_area.OriginWidth, _area.OriginHeight, _area.OriginWidth, _area.OriginHeight);
            //viewbox.Width = _area.ExtentWidth;
            //viewbox.Height = _area.ExtentHeight;
            viewbox.Child = gridEffect;

            //viewbox.VerticalAlignment = VerticalAlignment.Bottom;

            //return viewbox;

            viewbox.HorizontalAlignment = HorizontalAlignment;
            viewbox.VerticalAlignment = VerticalAlignment;


            var gridArea = new Grid();
            //gridArea.Margin = new Thickness(originWidth, originHeight, originWidth, originHeight);
            gridArea.Width = printWidth;
            gridArea.Height = printHeight;
            gridArea.Background = isBackground ? _context.Background : null;
            gridArea.Children.Add(viewbox);

            return gridArea;

            /*
            var gridRoot = new Grid();
            gridRoot.Width = extentWidth + originWidth * 2;
            gridRoot.Height = extentHeight + originHeight * 2;
            gridRoot.Children.Add(gridArea);

            return gridRoot;
            */
        }

        //
        public FrameworkElement CreateVisual(FrameworkElement visual, int x, int y)
        {
            /*
            bool isLandspace = PageOrientation == PageOrientation.Landscape;
            double extentWidth = isLandspace ? _area.ExtentHeight : _area.ExtentWidth;
            double extentHeight = isLandspace ? _area.ExtentWidth : _area.ExtentHeight;
            */

            var ox = _extentWidth * x;
            var oy = _extentHeight * y;

#if false
            var brush = new VisualBrush(visual);
            brush.Stretch = Stretch.None;
            brush.TileMode = TileMode.None;
            brush.Viewbox = new Rect(ox, oy, extentWidth, extentHeight);

            var rectangle = new Rectangle();
            rectangle.Width = extentWidth;
            rectangle.Height = extentHeight;
            rectangle.Fill = brush;

            var canvas = new Canvas();
            canvas.ClipToBounds = true;
            canvas.Width = extentWidth;
            canvas.Height = extentHeight;
            Canvas.SetLeft(rectangle, ox);
            Canvas.SetTop(rectangle, oy);
            canvas.Children.Add(rectangle);

#else
            var canvas = new Canvas();
            canvas.ClipToBounds = true;
            canvas.Width = _extentWidth;
            canvas.Height = _extentHeight;
            Canvas.SetLeft(visual, -ox);
            Canvas.SetTop(visual, -oy);
            canvas.Children.Add(visual);
#endif

            return canvas;
        }

        //
        public List<FixedPage> CreatePageCollection()
        {
            UpdatePrintDialog();

            UpdateImageableArea();

            var collection = new List<FixedPage>();


            for (int y = 0; y < Rows; ++y)
            {
                for (int x = 0; x < Columns; ++x)
                {
                    var fullVisual = CreateVisual();

                    var visual = CreateVisual(fullVisual, x, y);

                    var page = new FixedPage();
                    page.Width = _printableAreaWidth; // visual.Width; // 既定値上書きのため必須
                    page.Height = _printableAreaHeight; // visual.Height;
                    //page.Width = _originWidth + _extentWidth;
                    //page.Height = _originHeight + _extentHeight;

                    FixedPage.SetLeft(visual, _originWidth);
                    FixedPage.SetTop(visual, _originHeight);

                    page.Children.Add(visual);

                    collection.Add(page);
                }
            }

            return collection;
        }

#if false
        //
        public FixedPage CreatePage()
        {
            var fullVisual = CreateVisual();
            var visual = CreateVisual(fullVisual, 0, 0);

            var page = new FixedPage();
            page.Width = _printableAreaWidth; // visual.Width; // 既定値上書きのため必須
            page.Height = _printableAreaHeight; // visual.Height;

            FixedPage.SetLeft(visual, _area.OriginWidth);
            FixedPage.SetTop(visual, _area.OriginHeight);

            page.Children.Add(visual);

            return page;
        }
#endif

        //
        public FixedDocument CreateDocument()
        {
            var document = new FixedDocument();
            foreach (var page in CreatePageCollection().Select(e => new System.Windows.Documents.PageContent() { Child = e }))
            {
                document.Pages.Add(page);
            }
            return document;

#if false
            // FixedPageを作って印刷対象を設定する。
            var page = CreatePage();

            // PageContentを作ってFixedPageを設定する。
            var cont = new System.Windows.Documents.PageContent();
            cont.Child = page;

            // FixedDocumentを作ってPageContentを設定する。
            var doc = new FixedDocument();
            doc.Pages.Add(cont);
            return doc;
#endif
        }

        //
        public void Print()
        {
            GC.Collect();

            // 印刷する。

            //
            var name = GetPrintName();
            _printDialog.PrintDocument(CreateDocument().DocumentPaginator, name);

            return;
        }

        private string GetPrintName()
        {
            if (PrintMode == PrintMode.RawImage)
            {
                return LoosePath.GetFileName(_context.MainContent.FullPath);
            }
            else
            {
                return string.Join(" | ", _context.Contents.Reverse().Select(e => LoosePath.GetFileName(e.FullPath)));
            }
        }

#if false
        //
        public void Print()
        {
            // 印刷可能領域を取得する。
            var area = _printDialog.PrintQueue.GetPrintCapabilities().PageImageableArea;

            // 上と左の余白を含めた印刷可能領域の大きさのCanvasを作る。
            var canvas = new Canvas();
            canvas.Width = area.OriginWidth + area.ExtentWidth;
            canvas.Height = area.OriginHeight + area.ExtentHeight;


            // FixedPageを作って印刷対象（ここではCanvas）を設定する。
            var page = new FixedPage();
            page.Children.Add(_element);

            // PageContentを作ってFixedPageを設定する。
            var cont = new System.Windows.Documents.PageContent();
            cont.Child = page;

            // FixedDocumentを作ってPageContentを設定する。
            var doc = new FixedDocument();
            doc.Pages.Add(cont);


            // 印刷する。
            //printer.PrintDocument(doc.DocumentPaginator, "Print1");
        }
#endif
    }

    public class Margin : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Top property.
        /// </summary>
        private double _Top;
        public double Top
        {
            get { return _Top; }
            set { if (_Top != value) { _Top = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Bottom property.
        /// </summary>
        private double _Bottom;
        public double Bottom
        {
            get { return _Bottom; }
            set { if (_Bottom != value) { _Bottom = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Left property.
        /// </summary>
        private double _Left;
        public double Left
        {
            get { return _Left; }
            set { if (_Left != value) { _Left = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Right property.
        /// </summary>
        private double _Right;
        public double Right
        {
            get { return _Right; }
            set { if (_Right != value) { _Right = value; RaisePropertyChanged(); } }
        }
    }

    internal static class HorizontalAlignmentExtensions
    {
        public static double Direction(this HorizontalAlignment self)
        {
            switch (self)
            {
                case HorizontalAlignment.Left:
                    return -1.0;
                case HorizontalAlignment.Right:
                    return 1.0;
                default:
                    return 0.0;
            }
        }
    }

    internal static class VerticalAlignmentExtensions
    {
        public static double Direction(this VerticalAlignment self)
        {
            switch (self)
            {
                case VerticalAlignment.Top:
                    return -1.0;
                case VerticalAlignment.Bottom:
                    return 1.0;
                default:
                    return 0.0;
            }
        }
    }
}
