using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// QuickAccessPropertyDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class QuickAccessPropertyDialog : Window
    {
        private QuickAccessPropertyDialogViewModel _vm;


        public QuickAccessPropertyDialog()
        {
            InitializeComponent();
        }

        public QuickAccessPropertyDialog(QuickAccess quickAccess) : this()
        {
            _vm = new QuickAccessPropertyDialogViewModel(quickAccess);
            this.DataContext = _vm;
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }



    public class QuickAccessPropertyDialogViewModel : BindableBase
    {
        private QuickAccess _quickAccess;


        public QuickAccessPropertyDialogViewModel(QuickAccess quickAccess)
        {
            _quickAccess = quickAccess;
        }


        public string Name
        {
            get { return _quickAccess.Name; }
            set
            {
                if (_quickAccess.Name != value)
                {
                    _quickAccess.Name = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        public string Path
        {
            get { return _quickAccess.Path; }
            set
            {
                if (_quickAccess.Path != value)
                {
                    _quickAccess.Path = value;
                    RaisePropertyChanged(nameof(Path));
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

    }
}
