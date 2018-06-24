using NeeLaboratory.ComponentModel;
using System.Linq;

namespace NeeView
{
    public class QuickAccessListBoxViewModel : BindableBase
    {
        public QuickAccessListBoxViewModel()
        {
            QuickAccessCollection.Current.CollectionChanged += Items_CollectionChanged;
        }


        public QuickAccessCollection Collection => QuickAccessCollection.Current;


        private QuickAccess _oldSelectedItem;

        private QuickAccess _selectedItem;
        public QuickAccess SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    if (_selectedItem != null && _selectedItem != _oldSelectedItem)
                    {
                        _oldSelectedItem = _selectedItem;
                    }
                }
            }
        }

        private void Items_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            if (e.Action == System.ComponentModel.CollectionChangeAction.Add)
            {
                SelectedItem = e.Element as QuickAccess;
            }
        }

        public void Decide(QuickAccess item)
        {
            // 非同期で更新
            var task = FolderList.Current.SetPlaceAsync(item.Path, null, FolderSetPlaceOption.IsUpdateHistory | FolderSetPlaceOption.ResetKeyword);
        }

        public bool Remove(QuickAccess item)
        {
            return Collection.Remove(item);
        }

        public void RecoverySelectedItem()
        {
            var item = _selectedItem ?? _oldSelectedItem;
            if (!Collection.Items.Contains(item))
            {
                item = Collection.Items.FirstOrDefault();
            }
            SelectedItem = item;
        }
    }

}
