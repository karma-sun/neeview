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
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// 印刷モード
    /// </summary>
    public enum PrintMode
    {
        RawImage,
        View,
        ViewFill,
        ViewStretch,
    }

    /// <summary>
    /// Print Model
    /// </summary>
    [DataContract]
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



        /// <summary>
        /// PageOrientation property.
        /// </summary>
        [DataMember(Name = nameof(PageOrientation))]
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

        //
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
        [DataMember(Name = nameof(PrintMode))]
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
        [DataMember(Name = nameof(IsBackground))]
        private bool _IsBackground;
        public bool IsBackground
        {
            get { return _IsBackground; }
            set { if (_IsBackground != value) { _IsBackground = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsDotScale property.
        /// </summary>
        [DataMember(Name = nameof(IsDotScale))]
        private bool _IsDotScale;
        public bool IsDotScale
        {
            get { return _IsDotScale; }
            set { if (_IsDotScale != value) { _IsDotScale = value; RaisePropertyChanged(); } }
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
        [DataMember(Name = nameof(Columns))]
        private int _Columns = 1;
        public int Columns
        {
            get { return _Columns; }
            set { if (_Columns != value) { _Columns = NVUtility.Clamp(value, 1, 4); RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Rows property.
        /// </summary>
        [DataMember(Name = nameof(Rows))]
        private int _Rows = 1;
        public int Rows
        {
            get { return _Rows; }
            set { if (_Rows != value) { _Rows = NVUtility.Clamp(value, 1, 4); ; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// HorizontalAlignment property.
        /// </summary>
        [DataMember(Name = nameof(HorizontalAlignment))]
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
        [DataMember(Name = nameof(VerticalAlignment))]
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
        [DataMember(Name = nameof(Margin))]
        private Margin _Margin = new Margin();
        public Margin Margin
        {
            get { return _Margin; }
            set { if (_Margin != value) { _Margin = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// ミリメートルをピクセル数に変換(96DPI)
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        private double MillimeterToPixel(double mm)
        {
            return mm * 0.039370 * 96.0; // mm -> inch -> 96dpi
        }


        /// <summary>
        /// 印刷コンテキスト
        /// </summary>
        PrintContext _context;

        /// <summary>
        /// Print Dialog
        /// </summary>
        private PrintDialog _printDialog;
        
        /// <summary>
        /// 印刷領域サイズ
        /// </summary>
        private double _printableAreaWidth;
        private double _printableAreaHeight;

        /// <summary>
        /// 印刷エリア
        /// </summary>
        private PageImageableArea _area;

        /// <summary>
        /// 印刷開始位置
        /// </summary>
        private double _originWidth;
        private double _originHeight;

        /// <summary>
        /// 印刷コンテンツサイズ
        /// </summary>
        private double _extentWidth;
        private double _extentHeight;


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
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


        /// <summary>
        /// ダイアログ情報からパラメータ更新
        /// </summary>
        private void UpdatePrintDialog()
        {
            // プリンター
            PrintQueue = _printDialog.PrintQueue;
            ////Debug.WriteLine($"Printer: {PrintQueue.FullName}");

            // 用紙の方向 (縦/横限定)
            PageOrientation = _printDialog.PrintTicket.PageOrientation ?? PageOrientation.Unknown;

            // 用紙の印刷可能領域
            _area = _printDialog.PrintQueue.GetPrintCapabilities().PageImageableArea;
            ////Debug.WriteLine($"Origin: {_area.OriginWidth}x{_area.OriginHeight}");
            ////Debug.WriteLine($"Extent: {_area.ExtentWidth}x{_area.ExtentHeight}");
            ////Debug.WriteLine($"PrintableArea: {_printDialog.PrintableAreaWidth}x{_printDialog.PrintableAreaHeight}");

            _printableAreaWidth = _printDialog.PrintableAreaWidth;
            _printableAreaHeight = _printDialog.PrintableAreaHeight;
        }

        /// <summary>
        /// 画像コンテンツ生成
        /// </summary>
        /// <returns></returns>
        private FrameworkElement CreateRawImageContente()
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

        /// <summary>
        /// 表示コンテンツ生成
        /// </summary>
        /// <returns></returns>
        private FrameworkElement CreateViewContent()
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
            rectangle.RenderTransform = _context.ViewTransform;

            return rectangle;
        }


        /// <summary>
        /// 印刷領域パラメータ更新
        /// </summary>
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

        /// <summary>
        /// 印刷ビュー生成(全体)
        /// </summary>
        /// <returns></returns>
        private FrameworkElement CreateVisualElement()
        {
            bool isView = PrintMode != PrintMode.RawImage;

            bool isViewTransform = isView;
            bool isViewAll = PrintMode == PrintMode.ViewStretch || !isView;
            bool isViewPaperArea = isView && PrintMode == PrintMode.ViewFill;
            bool isEffect = isView;
            bool isBackground = IsBackground;

            double printWidth = _extentWidth * Columns;
            double printHeight = _extentHeight * Rows;

            var target = isView ? CreateViewContent() : CreateRawImageContente();

            var canvas = new Canvas();
            canvas.Width = target.Width;
            canvas.Height = target.Height;
            canvas.HorizontalAlignment = HorizontalAlignment.Center;
            canvas.VerticalAlignment = VerticalAlignment.Center;
            canvas.Children.Add(target);

            var gridClip = new Grid();
            gridClip.Name = "GridClip";
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
            }

            var gridEffect = new Grid();
            gridEffect.Name = "GridEffect";
            gridEffect.Effect = isEffect ? _context.ViewEffect : null;
            gridEffect.Children.Add(gridClip);

            var viewbox = new Viewbox();
            viewbox.Child = gridEffect;
            viewbox.HorizontalAlignment = HorizontalAlignment;
            viewbox.VerticalAlignment = VerticalAlignment;

            var gridArea = new Grid();
            gridArea.Width = printWidth;
            gridArea.Height = printHeight;
            gridArea.Background = isBackground ? _context.Background : null;
            gridArea.Children.Add(viewbox);

            return gridArea;
        }

        /// <summary>
        /// 印刷ビュー生成(分割)
        /// </summary>
        /// <param name="visual">印刷全体ビュー</param>
        /// <param name="column">列</param>
        /// <param name="row">行</param>
        /// <returns></returns>
        private FrameworkElement CreateVisual(FrameworkElement visual, int column, int row)
        {
            var ox = _extentWidth * column;
            var oy = _extentHeight * row;

            var canvas = new Canvas();
            canvas.ClipToBounds = true;
            canvas.Width = _extentWidth;
            canvas.Height = _extentHeight;
            Canvas.SetLeft(visual, -ox);
            Canvas.SetTop(visual, -oy);
            canvas.Children.Add(visual);

            return canvas;
        }

        /// <summary>
        /// 印刷ページ群生成
        /// </summary>
        /// <returns></returns>
        public List<FixedPage> CreatePageCollection()
        {
            UpdatePrintDialog();

            UpdateImageableArea();

            var collection = new List<FixedPage>();

            for (int row = 0; row < Rows; ++row)
            {
                for (int column = 0; column < Columns; ++column)
                {
                    var fullVisual = CreateVisualElement();

                    var visual = CreateVisual(fullVisual, column, row);

                    var page = new FixedPage();
                    page.Width = _printableAreaWidth; // 既定値上書きのため必須
                    page.Height = _printableAreaHeight;

                    FixedPage.SetLeft(visual, _originWidth);
                    FixedPage.SetTop(visual, _originHeight);

                    page.Children.Add(visual);

                    collection.Add(page);
                }
            }

            return collection;
        }

        /// <summary>
        /// 印刷ドキュメント生成
        /// </summary>
        /// <returns></returns>
        public FixedDocument CreateDocument()
        {
            var document = new FixedDocument();
            foreach (var page in CreatePageCollection().Select(e => new System.Windows.Documents.PageContent() { Child = e }))
            {
                document.Pages.Add(page);
            }
            return document;
        }

        /// <summary>
        /// 印刷実行
        /// </summary>
        public void Print()
        {
            GC.Collect();

            var name = "NeeView - " + GetPrintName();
            Debug.WriteLine($"Print {name}...");
            _printDialog.PrintDocument(CreateDocument().DocumentPaginator, name);
        }

        /// <summary>
        /// 印刷JOB名
        /// </summary>
        /// <returns></returns>
        private string GetPrintName()
        {
            if (PrintMode == PrintMode.RawImage)
            {
                return LoosePath.GetFileName(_context.MainContent.FullPath);
            }
            else
            {
                return string.Join(" | ", _context.Contents.Where(e => e.IsValid).Reverse().Select(e => LoosePath.GetFileName(e.FullPath)));
            }
        }


        #region Memento

        /// <summary>
        /// Memento
        /// </summary>
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PageOrientation PageOrientation { get; set; }

            [DataMember]
            public PrintMode PrintMode { get; set; }

            [DataMember]
            public bool IsBackground { get; set; }

            [DataMember]
            public bool IsDotScale { get; set; }

            [DataMember]
            public int Columns { get; set; }

            [DataMember]
            public int Rows { get; set; }

            [DataMember]
            public HorizontalAlignment HorizontalAlignment { get; set; }

            [DataMember]
            public VerticalAlignment VerticalAlignment { get; set; }

            [DataMember]
            public Margin Margin { get; set; }

            /// <summary>
            /// 初期化
            /// </summary>
            private void Constructor()
            {
                PageOrientation = PageOrientation.Portrait;
                PrintMode = PrintMode.View;
                HorizontalAlignment = HorizontalAlignment.Center;
                VerticalAlignment = VerticalAlignment.Center;
                Margin = new Margin();
            }

            /// <summary>
            /// コンストラクター
            /// </summary>
            public Memento()
            {
                Constructor();
            }

            /// <summary>
            /// デシリアイズ前処理
            /// </summary>
            /// <param name="c"></param>
            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            /// <summary>
            /// デシリアイズ後処理
            /// </summary>
            /// <param name="c"></param>
            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
            }
        }

        /// <summary>
        /// memento作成
        /// </summary>
        /// <returns></returns>
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.PageOrientation = PageOrientation;
            memento.PrintMode = PrintMode;
            memento.IsBackground = IsBackground;
            memento.IsDotScale = IsDotScale;
            memento.Columns = Columns;
            memento.Rows = Rows;
            memento.HorizontalAlignment = HorizontalAlignment;
            memento.VerticalAlignment = VerticalAlignment;
            memento.Margin = Margin;

            return memento;
        }

        /// <summary>
        /// memento反映
        /// </summary>
        /// <param name="memento"></param>
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            PageOrientation = memento.PageOrientation;
            PrintMode = memento.PrintMode;
            IsBackground = memento.IsBackground;
            IsDotScale = memento.IsDotScale;
            Columns = memento.Columns;
            Rows = memento.Rows;
            HorizontalAlignment = memento.HorizontalAlignment;
            VerticalAlignment = memento.VerticalAlignment;
            Margin = memento.Margin;
        }

        #endregion
    }


    /// <summary>
    /// 余白
    /// </summary>
    [DataContract]
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
        [DataMember]
        public double Top
        {
            get { return _Top; }
            set { if (_Top != value) { _Top = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Bottom property.
        /// </summary>
        private double _Bottom;
        [DataMember]
        public double Bottom
        {
            get { return _Bottom; }
            set { if (_Bottom != value) { _Bottom = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Left property.
        /// </summary>
        private double _Left;
        [DataMember]
        public double Left
        {
            get { return _Left; }
            set { if (_Left != value) { _Left = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Right property.
        /// </summary>
        private double _Right;
        [DataMember]
        public double Right
        {
            get { return _Right; }
            set { if (_Right != value) { _Right = value; RaisePropertyChanged(); } }
        }
    }

    /// <summary>
    /// HorizontalAlignment 拡張
    /// </summary>
    internal static class HorizontalAlignmentExtensions
    {
        /// <summary>
        /// 方向
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
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

    /// <summary>
    /// VerticalAlignment 拡張
    /// </summary>
    internal static class VerticalAlignmentExtensions
    {
        /// <summary>
        /// 方向
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
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
