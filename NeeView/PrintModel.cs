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
        public BitmapSource RawImage { get; set; }

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
        private PrintMode _PrintMode;
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
            [PrintMode.ViewFill] = "表示を印刷(用紙サイズに広げる)",
            [PrintMode.ViewStretch] = "表示を印刷(全体を印刷)",
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

        //
        PrintContext _context;


        //
        public PrintModel(PrintContext context)
        {
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


        private void UpdatePrintDialog()
        {
            // プリンター
            PrintQueue = _printDialog.PrintQueue;
            Debug.WriteLine($"Printer: {PrintQueue.FullName}");

            // 用紙の方向 (縦/横限定)
            PageOrientation = _printDialog.PrintTicket.PageOrientation ?? PageOrientation.Unknown;

            // 用紙の印刷可能領域
            _area = _printDialog.PrintQueue.GetPrintCapabilities().PageImageableArea;

            Debug.WriteLine($"Origin: {_area.OriginWidth}x{_area.OriginHeight}");
            Debug.WriteLine($"Extent: {_area.ExtentWidth}x{_area.ExtentHeight}");
            Debug.WriteLine($"PrintableArea: {_printDialog.PrintableAreaWidth}x{_printDialog.PrintableAreaHeight}");
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

        //
        public FrameworkElement CreateVisualElement()
        {
            UpdatePrintDialog();

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
            bool isLandspace = _printDialog.PrintTicket.PageOrientation == PageOrientation.Landscape;
            double originWidth = isLandspace ? _area.OriginHeight : _area.OriginWidth;
            double originHeight = isLandspace ? _area.OriginWidth : _area.OriginHeight;
            double extentWidth = isLandspace ? _area.ExtentHeight : _area.ExtentWidth;
            double extentHeight = isLandspace ? _area.ExtentWidth : _area.ExtentHeight;
#endif

            var target = isView ? CreateViewContent() : CreateRawImageContente();

            var canvas = new Canvas();
            canvas.Width = target.Width;
            canvas.Height = target.Height;
            canvas.HorizontalAlignment = HorizontalAlignment.Center;
            canvas.VerticalAlignment = VerticalAlignment.Center;
            canvas.Children.Add(target);
            
            var gridClip = new Grid();
            //gridClip.Name = "GridClip";
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

                /*
                var paperAspectRatio = _area.ExtentWidth / _area.ExtentHeight;
                var viewAspectRatio = gridClip.Width / gridClip.Height;
                if (viewAspectRatio > paperAspectRatio)
                {
                    gridClip.Height = gridClip.Width / paperAspectRatio;
                }
                else
                {
                    gridClip.Width = gridClip.Height * paperAspectRatio;
                }
                */

                ////gridClip.VerticalAlignment = VerticalAlignment.Bottom;

            }
            else if (isViewPaperArea)
            {
                var paperAspectRatio = extentWidth / extentHeight;
                var viewAspectRatio = _context.ViewWidth / _context.ViewHeight;
                if (viewAspectRatio > paperAspectRatio)
                {
                    gridClip.Height = _context.ViewWidth / paperAspectRatio;
                }
                else
                {
                    gridClip.Width = _context.ViewHeight * paperAspectRatio;
                }

                ////canvas.VerticalAlignment = VerticalAlignment.Bottom;
            }

            var gridEffect = new Grid();
            //gridEffect.Name = "GridEffect";
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

            var gridArea = new Grid();
            gridArea.Margin = new Thickness(originWidth, originHeight, originWidth, originHeight);
            gridArea.Width = extentWidth;
            gridArea.Height = extentHeight;
            gridArea.Background = isBackground ? _context.Background : null;
            gridArea.Children.Add(viewbox);

            var gridRoot = new Grid();
            gridRoot.Width = extentWidth + originWidth * 2;
            gridRoot.Height = extentHeight + originHeight * 2;
            gridRoot.Children.Add(gridArea);

            return gridRoot;
        }


        //
        public FixedPage CreatePage()
        {
            var visual = CreateVisual();

            var page = new FixedPage();
            page.Width = visual.Width; // 既定値上書きのため必須
            page.Height = visual.Height;
            page.Children.Add(visual);

            return page;
        }

        //
        public FixedDocument CreateDocument()
        {
            // FixedPageを作って印刷対象を設定する。
            var page = CreatePage();

            // PageContentを作ってFixedPageを設定する。
            var cont = new System.Windows.Documents.PageContent();
            cont.Child = page;

            // FixedDocumentを作ってPageContentを設定する。
            var doc = new FixedDocument();
            doc.Pages.Add(cont);

            return doc;
        }

        //
        public void Print()
        {
            GC.Collect();

            Debug.WriteLine($"ThreadId: {Thread.CurrentThread.ManagedThreadId}");

            /*
            // 印刷ダイアログ
            var resultPrinterDialog = ShowPrintDialog();
            if (resultPrinterDialog != true) return;

            return;
            */

            // 印刷する。
            _printDialog.PrintDocument(CreateDocument().DocumentPaginator, "Print1");

            return;

            /*
            var doc = CreateDocument();
            XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(_printDialog.PrintQueue);
            writer.WriteAsync(doc, _printDialog.PrintTicket);
            */
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
}
