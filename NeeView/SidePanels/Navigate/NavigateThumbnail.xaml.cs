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
        }

        private void NavigateThumbnail_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.IsEnabled = this.IsVisible;
        }

        private void ThumbnailGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _vm.LookAt(e.GetPosition(this.ThumbnailGrid));
                this.ThumbnailGrid.Cursor = Cursors.Hand;
            }
            else
            {
                this.ThumbnailGrid.Cursor = null;
            }
        }
    }
}
