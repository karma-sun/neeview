// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PageMarkers.xaml の相互作用ロジック
    /// </summary>
    public partial class PageMarkers : UserControl
    {
        private PageMarkersVM _VM;

        public PageMarkers()
        {
            InitializeComponent();

            _VM = new PageMarkersVM(this.RootCanvas);
            this.DataContext = _VM;
        }

        public void Initialize(BookHub bookHub)
        {
            _VM.BookHub = bookHub;
        }
    }

    /// <summary>
    /// ページマーク単体の表示情報
    /// </summary>
    public class PageMarker
    {
        public FrameworkElement Control { get; set; }

        private Book _Book;
        public Page Page { get; set; }

        private double _Position;

        public PageMarker(Book book, Page page)
        {
            _Book = book;
            Page = page;

            var path = new Path();
            path.Data = Geometry.Parse("M0,0 L0,10 5,9 10,10 10,0 z");
            path.Fill = App.Current.Resources["NVStarMarkBrush"] as Brush;
            //path.Stroke = Brushes.DarkOrange;
            //path.StrokeThickness = 1.0;
            path.Width = 10;
            path.Height = 18;
            path.Stretch = Stretch.Fill;
            this.Control = path;
        }

        /// <summary>
        /// 座標更新
        /// </summary>
        public void Update()
        {
            int max = _Book.Pages.Count - 1;
            if (max < 1) max = 1;
            _Position = (double)Page.Index / (double)max;
        }

        /// <summary>
        /// 表示更新
        /// </summary>
        /// <param name="width"></param>
        /// <param name="isReverse"></param>
        public void UpdateControl(double width, bool isReverse)
        {
            const double tumbWidth = 12;

            var x = (width - tumbWidth) * (isReverse ? 1.0 - _Position : _Position) + (tumbWidth - Control.Width) * 0.5;
            Canvas.SetLeft(Control, x);
        }
    }

    /// <summary>
    /// PageMakers ViewModel
    /// </summary>
    public class PageMarkersVM : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private Canvas _Canvas;

        #region Property: BookHub
        private BookHub _BookHub;
        public BookHub BookHub
        {
            get { return _BookHub; }
            set
            {
                if (_BookHub != value)
                {
                    _BookHub = value;
                    BookHubChanged();
                }
            }
        }
        #endregion

        //
        private List<PageMarker> Markers;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="canvas"></param>
        public PageMarkersVM(Canvas canvas)
        {
            Markers = new List<PageMarker>();

            _Canvas = canvas;
            _Canvas.SizeChanged += Canvas_SizeChanged;
        }

        /// <summary>
        /// サイズ変更されたらマーカー表示座標を更新する
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
        /// BookHub Changed
        /// </summary>
        private void BookHubChanged()
        {
            _BookHub.BookChanged += (s, e) => BookChanged();
            _BookHub.PagemarkChanged += (s, e) => UpdateInvoke();
            _BookHub.PagesSorted += (s, e) => UpdateInvoke();
            _BookHub.PageRemoved += (s, e) => UpdateInvoke();
        }

        /// <summary>
        /// マーカー表示更新 (表示スレッド)
        /// </summary>
        private void UpdateInvoke()
        {
            App.Current.Dispatcher.Invoke(() => Update());
        }

        /// <summary>
        /// 本が変更された場合、全てを更新
        /// </summary>
        private void BookChanged()
        {
            // clear
            _Canvas.Children.Clear();
            Markers.Clear();

            if (_BookHub.CurrentBook == null) return;

            // update first
            Update();
        }

        /// <summary>
        /// マーカー更新
        /// </summary>
        private void Update()
        {
            // remove markers
            foreach (var marker in Markers.Where(e => !_BookHub.CurrentBook.Markers.Contains(e.Page)).ToList())
            {
                _Canvas.Children.Remove(marker.Control);
                Markers.Remove(marker);
            }

            // add markers
            foreach (var key in _BookHub.CurrentBook.Markers.Where(e => Markers.All(m => m.Page != e)).ToList())
            {
                var marker = new PageMarker(_BookHub.CurrentBook, key);
                _Canvas.Children.Add(marker.Control);
                Markers.Add(marker);
            }

            // update
            Markers.ForEach(e => e.Update());

            // update control
            UpdateControl();
        }

        /// <summary>
        /// マーカー更新(表示)
        /// </summary>
        private void UpdateControl()
        {
            Markers.ForEach(e => e.UpdateControl(_Canvas.ActualWidth, true));
        }

    }
}
