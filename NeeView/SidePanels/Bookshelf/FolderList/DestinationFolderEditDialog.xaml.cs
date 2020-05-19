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
    /// DestinationFolderEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class DestinationFolderEditDialog : Window
    {
        private DestinationFolderEditDialogViewMdoel _vm;

        public DestinationFolderEditDialog()
        {
            InitializeComponent();
        }

        public DestinationFolderEditDialog(DestinationFolder model) : this()
        {
            _vm = new DestinationFolderEditDialogViewMdoel(model);
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


    public class DestinationFolderEditDialogViewMdoel : BindableBase
    {
        private DestinationFolder _model;


        public DestinationFolderEditDialogViewMdoel(DestinationFolder model)
        {
            _model = model;
        }


        public string Name
        {
            get => _model.Name;
            set => _model.Name = value;
        }

        public string Path
        {
            get => _model.Path;
            set
            {
                _model.Path = value;
                RaisePropertyChanged(nameof(Name));
            }
        }
    }
}
