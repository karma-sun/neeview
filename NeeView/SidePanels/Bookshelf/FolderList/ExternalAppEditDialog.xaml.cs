using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// ExternalAppEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ExternalAppEditDialog : Window
    {
        private ExternalAppEditDialogViewModel _vm;

        public ExternalAppEditDialog()
        {
            InitializeComponent();
        }

        public ExternalAppEditDialog(ExternalApp model) : this()
        {
            _vm = new ExternalAppEditDialogViewModel(model);
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


    public class ArchivePolicyToSampleStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ArchivePolicy policy)
            {
                return "Sample: " + policy.ToSampleText();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ExternalAppEditDialogViewModel : BindableBase
    {
        private ExternalApp _model;

        public ExternalAppEditDialogViewModel(ExternalApp model)
        {
            _model = model;
        }

        public ExternalApp Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }


        public string Name
        {
            get => _model.Name ?? _model.DispName;
            set
            {
                value = string.IsNullOrWhiteSpace(value) ? null : value;
                if (_model.Name != value)
                {
                    _model.Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Command
        {
            get { return _model.Command; }
            set { if (_model.Command != value)
                {
                    _model.Command = value;
                    RaisePropertyChanged();

                    if (_model.Name == null)
                    {
                        _model.Name = _model.DispName;
                        RaisePropertyChanged(nameof(Name));
                    }
                }
            }
        }


        public Dictionary<ArchivePolicy, string> ArchivePolicyList => AliasNameExtensions.GetAliasNameDictionary<ArchivePolicy>();
    }
}
