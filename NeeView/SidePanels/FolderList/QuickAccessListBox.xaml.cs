using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    /// QuickAccessListBox.xaml の相互作用ロジック
    /// </summary>
    public partial class QuickAccessListBox : UserControl
    {
        public static string DragDropFormat = $"{Config.Current.ProcessId}.DragDropFormat";

        private QuickAccessListBoxViewModel _vm;

        static QuickAccessListBox()
        {
            InitializeCommandStatic();
        }

        public QuickAccessListBox()
        {
            InitializeComponent();
            InitializeCommand();

            _vm = new QuickAccessListBoxViewModel();
            this.Root.DataContext = _vm;
        }


        #region Commands

        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(QuickAccessListBox));

        private static void InitializeCommandStatic()
        {
            RemoveCommand.InputGestures.Add(new KeyGesture(Key.Delete));
        }

        public void InitializeCommand()
        {
            this.ListBox.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec));
        }

        public void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var item = (sender as ListBox)?.SelectedItem as QuickAccess;
            if (item != null)
            {
                _vm.Remove(item);
            }
        }

        #endregion


        private void ListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e, DragDropFormat);
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            var list = (sender as ListBox).Tag as ObservableCollection<QuickAccess>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop(sender, e, DragDropFormat, list);
                e.Handled = true;
            }
        }

        private void ListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((sender as ListBoxItem)?.Content as QuickAccess);

            _vm.Decide(item);
            e.Handled = true;
        }

        private void ListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var item = ((sender as ListBoxItem)?.Content as QuickAccess);

            if (e.Key == Key.Return)
            {
                _vm.Decide(item);
                e.Handled = true;
            }
        }

        private void ListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.ListBox.SelectedItem = null;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ListBox.SelectedItem != null)
            {
                this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
            }
        }

        public bool FocusSelectedItem()
        {
            _vm.RecoverySelectedItem();

            if (this.ListBox.SelectedIndex >= 0)
            {
                this.ListBox.ScrollIntoView(this.ListBox.SelectedItem);
                ListBoxItem lbi = (ListBoxItem)(this.ListBox.ItemContainerGenerator.ContainerFromIndex(this.ListBox.SelectedIndex));
                if (lbi != null)
                {
                    return lbi.Focus();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

}
