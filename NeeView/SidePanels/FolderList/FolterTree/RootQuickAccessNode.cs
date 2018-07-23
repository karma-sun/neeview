using NeeLaboratory.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace NeeView
{

    public class RootQuickAccessNode : FolderTreeNodeBase
    {
        public RootQuickAccessNode()
        {
            RefreshChildren();
            QuickAccessCollection.Current.CollectionChanged += QuickAccessCollection_CollectionChanged;
        }


        public override string Name { get => QueryScheme.QuickAccess.ToSchemeString(); set { } }

        public override string DispName { get => Properties.Resources.WordQuickAccess; set { } }

        public override ImageSource Icon => MainWindow.Current.Resources["ic_pushpin"] as ImageSource;

        public override ObservableCollection<FolderTreeNodeBase> Children
        {
            get { return _children = _children ?? new ObservableCollection<FolderTreeNodeBase>(QuickAccessCollection.Current.Items.Select(e => new QuickAccessNode(e, this))); }
            set { SetProperty(ref _children, value); }
        }


        private void QuickAccessCollection_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            var item = e.Element as QuickAccess;

            switch (e.Action)
            {
                case CollectionChangeAction.Refresh:
                    RefreshChildren();
                    break;

                case CollectionChangeAction.Add:
                    var index = QuickAccessCollection.Current.Items.IndexOf(item);
                    Insert(index, new QuickAccessNode(item, null));
                    break;

                case CollectionChangeAction.Remove:
                    Remove(item);
                    break;
            }
        }

    }
}
