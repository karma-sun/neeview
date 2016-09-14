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

    //
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
            path.Data = Geometry.Parse("M0,0 L0,10 5,8 10,10 10,0 z");
            path.Fill = Brushes.Orange;
            path.Width = 10;
            path.Height = 18;
            path.Stretch = Stretch.Fill;
            this.Control = path;
        }

        //
        public void Update()
        {
            int max = _Book.Pages.Count - 1;
            if (max < 1) max = 1;
            _Position = (double)_Book.GetIndex(Page) / (double)max;
        }

        //
        public void UpdateControl(double width, bool isReverse)
        {
            var x = (width - Control.Width) * (isReverse ? 1.0 - _Position : _Position);
            Canvas.SetLeft(Control, x);
        }
    }

    /// <summary>
    /// 
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

        //
        public PageMarkersVM(Canvas canvas)
        {
            Markers = new List<PageMarker>();

            _Canvas = canvas;
            _Canvas.SizeChanged += Canvas_SizeChanged;
        }

        //
        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                UpdateControl();
            }
        }

        private void BookHubChanged()
        {
            _BookHub.BookChanged += (s, e) => BookChanged();
            _BookHub.MarkerChanged += (s, e) => UpdateInvoke();
            _BookHub.PagesSorted += (s, e) => UpdateInvoke();
            _BookHub.PageRemoved += (s, e) => UpdateInvoke();
        }

        private void UpdateInvoke()
        {
            App.Current.Dispatcher.Invoke(() => Update());
        }

        private void BookChanged()
        {
            // clear
            _Canvas.Children.Clear();
            Markers.Clear();

            if (_BookHub.CurrentBook == null) return;

            // update first
            Update();
        }

        //
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

        //
        private void UpdateControl()
        {
            Markers.ForEach(e => e.UpdateControl(_Canvas.ActualWidth, true));
        }

    }
}
