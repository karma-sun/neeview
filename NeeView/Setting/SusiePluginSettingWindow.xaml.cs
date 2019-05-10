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

            this.Closed += SusiePluginSettingWindow_Closed;
        }

        private void SusiePluginSettingWindow_Closed(object sender, EventArgs e)
        {
            _vm.Flush();
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
            DefaultExtensions = new FileTypeCollection(_spi.DefaultExtensions);
            Extensions = new FileTypeCollection(_spi.Extensions);
        }

        public string Name => _spi.Name;

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

        public FileTypeCollection DefaultExtensions { get; set; }

        public FileTypeCollection Extensions { get; set; }

        public bool CanOpenConfigDialog => _spi.HasConfigurationDlg;


        public void OpenConfigDialog(Window owner)
        {
            _spi.OpenConfigulationDialog(owner);
        }

        public void Flush()
        {
            _spi.UserExtensions = new FileExtensionCollection(Extensions.OneLine);
        }
    }

}
