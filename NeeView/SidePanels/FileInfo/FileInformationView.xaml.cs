using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
    /// FileInformationView.xaml の相互作用ロジック
    /// </summary>
    public partial class FileInformationView : UserControl
    {
        private FileInformationViewModel _vm;
        private bool _isFocusRequest;


        public FileInformationView()
        {
            InitializeComponent();
        }

        public FileInformationView(FileInformation model) : this()
        {
            _vm = new FileInformationViewModel(model);
            this.DataContext = _vm;

            this.IsVisibleChanged += FileInformationView_IsVisibleChanged;

            // タッチスクロール操作の終端挙動抑制
            this.ScrollView.ManipulationBoundaryFeedback += SidePanelFrame.Current.ScrollViewer_ManipulationBoundaryFeedback;
        }


        private void FileInformationView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_isFocusRequest && this.IsVisible)
            {
                this.Focus();
                _isFocusRequest = false;
            }
        }

        public void FocusAtOnce()
        {
            var focused = this.Focus();
            if (!focused)
            {
                _isFocusRequest = true;
            }
        }
    }
}
