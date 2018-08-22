using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Susie;
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

namespace NeeView.Setting
{
    /// <summary>
    /// SusiePluginSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SusiePluginSettingWindow : Window
    {
        private SusiePluginSettingWindowViewModel _vm;

        public SusiePluginSettingWindow()
        {
            InitializeComponent();
        }

        public SusiePluginSettingWindow(SusiePlugin spi) : this()
        {
            _vm = new SusiePluginSettingWindowViewModel(spi);
            this.DataContext = _vm;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.OpenConfigDialog(this);
        }
    }

    /// <summary>
    /// SusiePluginSettingWindow ViewModel
    /// </summary>
    public class SusiePluginSettingWindowViewModel : BindableBase
    {
        private SusiePlugin _spi;

        public SusiePluginSettingWindowViewModel(SusiePlugin spi)
        {
            _spi = spi;
        }

        public string Name => _spi.Name;
        public string Extensions => string.Join(" ", _spi.Extensions);
        public string Version => _spi.PluginVersion;

        public bool IsArchiver => _spi.PluginType == SusiePluginType.Archive;

        public bool IsEnabled
        {
            get { return _spi.IsEnabled; }
            set { _spi.IsEnabled = value; }
        }

        public bool IsPreExtract
        {
            get { return _spi.IsPreExtract; }
            set { _spi.IsPreExtract = value; }
        }

        public bool CanOpenConfigDialog => _spi.HasConfigurationDlg;


        public void OpenConfigDialog(Window owner)
        {
            _spi.OpenConfigurationDlg_Executed(owner);
        }
    }

}
