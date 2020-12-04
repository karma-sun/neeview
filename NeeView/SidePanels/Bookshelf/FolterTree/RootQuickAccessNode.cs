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
            // NOTE: need call Initialize()
        }


        public override string Name { get => QueryScheme.QuickAccess.ToSchemeString(); set { } }

        public override string DispName { get => Properties.Resources.WordQuickAccess; set { } }

        public override IImageSourceCollection Icon => new SingleImageSourceCollection(MainWindow.Current.Resources["ic_lightning"] as ImageSource);

        public override ObservableCollection<FolderTreeNodeBase> Children
        {
            get { return _children = _children ?? new ObservableCollection<FolderTreeNodeBase>(QuickAccessCollection.Current.Items.Select(e => new QuickAccessNode(e, this))); }
            set { SetProperty(ref _children, value); }
        }


        public void Initialize(FolderTreeNodeBase parent)
        {
            Parent = parent;

            RefreshChildren();
            QuickAccessCollection.Current.CollectionChanged += QuickAccessCollection_CollectionChanged;
        }

        private void QuickAccessCollection_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            var item = e.Element as QuickAccess;

            switch (e.Action)
            {
                case CollectionChangeAction.Refresh:
                    RefreshChildren(isExpanded: true);
                    break;

                case CollectionChangeAction.Add:
                    var index = QuickAccessCollection.Current.Items.IndexOf(item);
                    var node = new QuickAccessNode(item, null) { IsSelected = true }; // NOTE: 選択項目として追加
                    Insert(index, node);
                    break;

                case CollectionChangeAction.Remove:
                    Remove(item);
                    break;
            }
        }

    }
}
