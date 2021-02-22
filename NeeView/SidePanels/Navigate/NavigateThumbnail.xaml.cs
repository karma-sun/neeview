using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// NavigateThumbnail.xaml の相互作用ロジック
    /// </summary>
    public partial class NavigateThumbnail : UserControl
    {
        private NavigateThumbnailViewModel _vm;

        public NavigateThumbnail()
        {
            InitializeComponent();

            _vm = new NavigateThumbnailViewModel(MainViewComponent.Current);
            this.Root.DataContext = _vm;

            this.IsVisibleChanged += NavigateThumbnail_IsVisibleChanged;

            this.ThumbnailGrid.MouseLeftButtonDown += ThumbnailGrid_MouseLeftButtonDown;
            this.ThumbnailGrid.PreviewMouseLeftButtonUp += ThumbnailGrid_PreviewMouseLeftButtonUp;
            this.ThumbnailGrid.MouseMove += ThumbnailGrid_MouseMove;
        }


        private void NavigateThumbnail_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.IsEnabled = this.IsVisible;
        }

        private void ThumbnailGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ThumbnailGrid.CaptureMouse();
            this.ThumbnailGrid.Cursor = Cursors.Hand;

            _vm.LookAt(e.GetPosition(this.ThumbnailGrid));
        }

        private void ThumbnailGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ThumbnailGrid.ReleaseMouseCapture();
            this.ThumbnailGrid.Cursor = null;
        }

        private void ThumbnailGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            _vm.LookAt(e.GetPosition(this.ThumbnailGrid));
        }
    }
}
