using NeeLaboratory.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PageMakers : ViewModel
    /// </summary>
    public class PageMarkersViewModel : BindableBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="canvas"></param>
        public PageMarkersViewModel(PageMarkers model, Canvas canvas)
        {
            _model = model;

            _canvas = canvas;
            _canvas.SizeChanged += Canvas_SizeChanged;

            _model.AddPropertyChanged(nameof(_model.MarkerCollection),
                (s, e) => UpdateInvoke());
            _model.AddPropertyChanged(nameof(_model.IsSliderDirectionReversed),
                (s, e) => UpdateInvoke());
        }


        /// <summary>
        /// Model property.
        /// </summary>
        public PageMarkers Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private PageMarkers _model;


        // マーカーコントロールを登録するキャンバス
        private Canvas _canvas;


        /// <summary>
        /// キャンバスサイズが変更されたらマーカー表示座標を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                UpdateControl();
            }
        }


        /// <summary>
        /// マーカー表示更新 (表示スレッド)
        /// </summary>
        private void UpdateInvoke()
        {
            App.Current?.Dispatcher.Invoke(() => UpdateControl());
        }


        /// <summary>
        /// マーカー表示高悪心
        /// </summary>
        private void UpdateControl()
        {
            if (_model.MarkerCollection == null)
            {
                _canvas.Children.Clear();
                return;
            }

            var canvasWidth = _canvas.ActualWidth;
            bool isReverse = _model.IsSliderDirectionReversed;

            // Pathの作成
            Path path = new Path();
            path.Fill = App.Current.Resources["NVStarMarkBrush"] as Brush;

            StreamGeometry geometry = new StreamGeometry();
            geometry.FillRule = FillRule.Nonzero;
            using (StreamGeometryContext context = geometry.Open())
            {
                foreach (var index in _model.MarkerCollection.Indexes)
                {
                    const double tumbWidth = 25.0;

                    double min = tumbWidth * 0.5;
                    double max = canvasWidth - tumbWidth * 0.5;
                    double valueMin = 0;
                    double valueMax = _model.MarkerCollection.Maximum;
                    double value = isReverse ? valueMax - index + valueMin : index;

                    double x = value * (max - min) / (valueMax - valueMin) + min;
                    double y = 15.0; // キャンバスの高さの半分。中央座標のY。

                    context.BeginFigure(new Point(x + 0, y - 5), true, true);
                    context.LineTo(new Point(x + 5, y + 0), true, true);
                    context.LineTo(new Point(x + 0, y + 5), true, true);
                    context.LineTo(new Point(x - 5, y + 0), true, true);
                }
            }
            geometry.Freeze();

            path.Data = geometry;


            // Pathをキャンバスに登録
            _canvas.Children.Clear();
            _canvas.Children.Add(path);
        }

    }
}
