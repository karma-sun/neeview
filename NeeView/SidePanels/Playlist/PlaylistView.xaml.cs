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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using NeeView.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// HistoryListView.xaml の相互作用ロジック
    /// </summary>
    public partial class PlaylistView : UserControl
    {
        private PlaylistViewModel _vm;


        public PlaylistView()
        {
            InitializeComponent();
            InitializeCommand();
        }

        public PlaylistView(PlaylistModel model) : this()
        {
            _vm = new PlaylistViewModel(model);
            this.DockPanel.DataContext = _vm;

            _vm.RenameRequest +=
                (s, e) => Rename();
       }


        public readonly static RoutedCommand RenameCommand = new RoutedCommand(nameof(RenameCommand), typeof(PlaylistView), new InputGestureCollection() { new KeyGesture(Key.F2) });

        private void InitializeCommand()
        {
            this.PlaylistComboBox.CommandBindings.Add(new CommandBinding(RenameCommand, RenameCommand_Execute, RenameCommand_CanExecute));
        }

        private void RenameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _vm.RenameCommand.CanExecute(e.Parameter);
        }

        private void RenameCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            _vm.RenameCommand.Execute(e.Parameter);
        }


        private void Rename()
        {
            var comboBox = this.PlaylistComboBox;
            comboBox.UpdateLayout();

            var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(comboBox, "NameTextBlock");
            if (textBlock is null) return;

            var rename = new RenameControl() { Target = textBlock };
            rename.IsInvalidFileNameChars = true;

            rename.Closing += (s, ev) =>
            {
                if (ev.OldValue != ev.NewValue)
                {
                    bool isRenamed = _vm.Rename(ev.NewValue);
                    ev.Cancel = !isRenamed;
                }
            };
            rename.Closed += (s, ev) =>
            {
                comboBox.Focus();
            };

            MainWindow.Current.RenameManager.Open(rename);
        }
    }
}
