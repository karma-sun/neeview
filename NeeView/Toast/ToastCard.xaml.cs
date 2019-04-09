using System;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Toast.xaml の相互作用ロジック
    /// </summary>
    public partial class ToastCard : UserControl
    {
        Toast _toast;

        public ToastCard()
        {
            InitializeComponent();
        }

        public Toast Toast
        {
            get { return _toast; }
            set
            {
                if (_toast != value)
                {
                    _toast = value;
                    Refresh();
                }
            }
        }

        public bool IsCanceled { get; set; }

        private void Refresh()
        {
            this.Caption.Text = _toast.Caption;
            this.Caption.Visibility = _toast.Caption is null ? Visibility.Collapsed : Visibility.Visible;
            this.Message.Source = _toast.Message;
            this.ConfirmButton.Content = _toast.ButtonContent;
            this.ConfirmButton.Visibility = _toast.ButtonContent is null ? Visibility.Collapsed : Visibility.Visible;
           
            switch(_toast.Icon)
            {
                default:
                case ToastIcon.Information:
                    this.Icon.Source = (DrawingImage)this.Resources["tic_info"];
                    break;
                case ToastIcon.Warning:
                    this.Icon.Source = (DrawingImage)this.Resources["tic_warning"];
                    break;
                case ToastIcon.Error:
                    this.Icon.Source = (DrawingImage)this.Resources["tic_error"];
                    break;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            ToastService.Current.Update();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _toast.RaiseConfirmedEvent();
            IsCanceled = true;
            ToastService.Current.Update();
        }

        // from http://gushwell.ldblog.jp/archives/52279481.html
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.DialogHyperLinkFailedTitle).ShowDialog();
            }
        }
    }
}
