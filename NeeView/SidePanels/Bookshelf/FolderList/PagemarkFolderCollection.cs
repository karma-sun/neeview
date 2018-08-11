using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{
    public class PagemarkFolderCollection : FolderCollection, IDisposable
    {
        // Fields

        private TreeListNode<IPagemarkEntry> _pagemarkPlace;

        // Constructors

        public PagemarkFolderCollection(QueryPath path) : base(path, false)
        {
            _pagemarkPlace = PagemarkCollection.Current.FindNode(path) ?? new TreeListNode<IPagemarkEntry>();

            var specials = _pagemarkPlace.Children
                .Where(e => e.Value is DefaultPagemarkFolder)
                .Select(e => CreateFolderItem(e))
                .Where(e => e != null);
            //.ToList();

            specials = Sort(specials);

            var items = _pagemarkPlace.Children
                .Where(e => e.Value is PagemarkFolder && !(e.Value is DefaultPagemarkFolder))
                .Select(e => CreateFolderItem(e))
                .Where(e => e != null);
            //.ToList();

            items = Sort(items);

            //var list = Sort(items).ToList();
            var list = specials.Concat(items).ToList();

            if (!list.Any())
            {
                list.Add(CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            // 変更監視
            PagemarkCollection.Current.PagemarkChanged += PagemarkCollection_PagemarkChanged;
        }


        // Properties

        public TreeListNode<IPagemarkEntry> PagemarkPlace => _pagemarkPlace;


        // Methods

        private void PagemarkCollection_PagemarkChanged(object sender, PagemarkCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case EntryCollectionChangedAction.Add:
                    if (e.Parent == _pagemarkPlace)
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item == null)
                        {
                            item = CreateFolderItem(e.Item);
                            CreateItem(item);
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Remove:
                    if (e.Parent == _pagemarkPlace)
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item != null)
                        {
                            DeleteItem(item);
                        }
                    }
                    break;

                case EntryCollectionChangedAction.Rename:
                    if (e.Parent == _pagemarkPlace)
                    {
                        var item = Items.FirstOrDefault(i => e.Item == i.Source);
                        if (item != null)
                        {
                            RenameItem(item, e.Item.Value.Name);
                        }
                    }
                    break;


                case EntryCollectionChangedAction.Move:
                    // nop.
                    break;

                case EntryCollectionChangedAction.Replace:
                case EntryCollectionChangedAction.Reset:
                    FolderList.Current.RequestPlace(new QueryPath(QueryScheme.Pagemark, null, null), null, FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.Refresh);
                    break;
            }
        }


        public FolderItem CreateFolderItem(TreeListNode<IPagemarkEntry> node)
        {
            var scheme = QueryScheme.Pagemark.ToSchemeString();

            switch (node.Value)
            {
                case DefaultPagemarkFolder folder:
                    return new ConstFolderItem(new FolderThumbnail())
                    {
                        Source = node,
                        Type = FolderItemType.Directory,
                        Place = Place,
                        Name = folder.Name,
                        DispName = Properties.Resources.WordDefaultPagemark,
                        Length = -1,
                        Attributes = FolderItemAttribute.Directory | FolderItemAttribute.Pagemark | FolderItemAttribute.ReadOnly,
                        IsReady = true
                    };

                case PagemarkFolder folder:
                    return new ConstFolderItem(new FolderThumbnail())
                    {
                        Source = node,
                        Type = FolderItemType.Directory,
                        Place = Place,
                        Name = folder.Name,
                        Length = -1,
                        Attributes = FolderItemAttribute.Directory | FolderItemAttribute.Pagemark,
                        IsReady = true
                    };

                default:
                    return null;
            }
        }


        #region IDisposable Support

        private bool _disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    PagemarkCollection.Current.PagemarkChanged -= PagemarkCollection_PagemarkChanged;
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
