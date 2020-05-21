using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace NeeView
{
    public class DestinationFolderDialogViewModel : BindableBase
    {
        public ObservableCollection<DestinationFolder> _items;
        private int _selectedIndex = -1;


        public DestinationFolderDialogViewModel()
        {
            _items = new ObservableCollection<DestinationFolder>(Config.Current.System.DestinationFodlerCollection);

            AddCommand = new RelayCommand(AddCommand_Execute);
            EditCommand = new RelayCommand(EditCommand_Execute, SelectedItemCommand_CanExecute);
            DeleteCommand = new RelayCommand(DeleteCommand_Execute, SelectedItemCommand_CanExecute);
            MoveUpCommand = new RelayCommand(MoveUpCommand_Execute, MoveUpCommand_CanExecute);
            MoveDownCommand = new RelayCommand(MoveDownCommand_Execute, MoveDownCommand_CanExecute);
        }


        public Window Owner { get; set; }


        public ObservableCollection<DestinationFolder> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (SetProperty(ref _selectedIndex, value))
                {
                    EditCommand.RaiseCanExecuteChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
                    MoveUpCommand.RaiseCanExecuteChanged();
                    MoveDownCommand.RaiseCanExecuteChanged();
                }
            }
        }



        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand MoveUpCommand { get; }
        public RelayCommand MoveDownCommand { get; }



        private void AddCommand_Execute()
        {
            CallEditDialog(-1, new DestinationFolder());
        }

        private bool SelectedItemCommand_CanExecute()
        {
            return Items.Any() && _selectedIndex >= 0;
        }

        private void EditCommand_Execute()
        {
            if (_selectedIndex < 0) return;

            var index = _selectedIndex;
            var item = (DestinationFolder)Items[_selectedIndex].Clone();

            CallEditDialog(index, item);
        }

        private void CallEditDialog(int index, DestinationFolder item)
        {
            var dialog = new DestinationFolderEditDialog(item);
            dialog.Owner = Owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true && item.IsValid())
            {
                if (index >= 0)
                {
                    Items[index] = item;
                }
                else
                {
                    Items.Add(item);
                }
            }
        }

        public void Add(string path)
        {
            Items.Add(new DestinationFolder("", path));
        }

        private void DeleteCommand_Execute()
        {
            if (_selectedIndex < 0) return;

            var index = _selectedIndex;
            Items.RemoveAt(_selectedIndex);
            SelectedIndex = MathUtility.Clamp(index, -1, Items.Count - 1);
        }

        private bool MoveUpCommand_CanExecute()
        {
            return _selectedIndex > 0;
        }

        private void MoveUpCommand_Execute()
        {
            if (!MoveUpCommand_CanExecute()) return;

            Items.Move(_selectedIndex, _selectedIndex - 1);
        }

        private bool MoveDownCommand_CanExecute()
        {
            return _selectedIndex >= 0 && _selectedIndex < Items.Count - 1;
        }

        private void MoveDownCommand_Execute()
        {
            if (!MoveDownCommand_CanExecute()) return;

            Items.Move(_selectedIndex, _selectedIndex + 1);
        }


        public void Decide()
        {
            Config.Current.System.DestinationFodlerCollection = new DestinationFolderCollection(_items);
        }
    }
}
