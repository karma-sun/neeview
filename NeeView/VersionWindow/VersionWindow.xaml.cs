using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Markup;
using NeeView.Native;

namespace NeeView
{
    /// <summary>
    /// VersionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionWindow : Window
    {
        public readonly static RoutedCommand CopyCommand = new RoutedCommand("CopyCommand", typeof(VersionWindowViewModel), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.C, ModifierKeys.Control) }));

        private VersionWindowViewModel _vm;


        public VersionWindow()
        {
            Interop.NVFpReset();

            InitializeComponent();

            _vm = new VersionWindowViewModel();
            this.DataContext = _vm;

            this.CommandBindings.Add(new CommandBinding(CopyCommand, (s, e) => _vm.CopyVersionToClipboard(), (s, e) => e.CanExecute = true));
            this.CopyContextMenu.CommandBindings.Add(new CommandBinding(CopyCommand, (s, e) => _vm.CopyVersionToClipboard(), (s, e) => e.CanExecute = true));
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
