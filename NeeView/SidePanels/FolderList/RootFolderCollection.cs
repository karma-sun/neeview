using System;
using System.Collections.ObjectModel;

namespace NeeView
{
    public class RootFolderCollection : FolderCollection, IDisposable
    {
        public RootFolderCollection() : base(new QueryPath(QueryScheme.Root, null, null), false)
        {
            var items = new ObservableCollection<FolderItem>();

            items.Add(CreateFolderItem(QueryScheme.File));
            items.Add(CreateFolderItem(QueryScheme.Bookmark));

            this.Items = items;
        }

        private FolderItem CreateFolderItem(QueryScheme scheme)
        {
            return new FolderItem()
            {
                Source = scheme,
                Type = FolderItemType.Directory,
                Place = new QueryPath(scheme, null, null),
                Name = scheme.ToAliasName(),
                Length = -1,
                Attributes = FolderItemAttribute.Directory | FolderItemAttribute.System,
                IsReady = true
            };
        }
    }
}
