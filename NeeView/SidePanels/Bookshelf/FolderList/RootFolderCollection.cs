using System;
using System.Collections.ObjectModel;

namespace NeeView
{
    /// <summary>
    /// 最上位のフォルダーコレクション (Root)
    /// </summary>
    public class RootFolderCollection : FolderCollection, IDisposable
    {
        public RootFolderCollection(QueryPath path, bool isOverlayEnabled) : base(path, false, isOverlayEnabled)
        {
            var items = new ObservableCollection<FolderItem>();

            if (path.Path == null)
            {
                // NOTE: 操作に難があるため、クイックアクセス、ページマークは表示しない
                ////items.Add(CreateFolderItem(QueryScheme.QuickAccess));
                items.Add(CreateFolderItem(QueryScheme.File));
                items.Add(CreateFolderItem(QueryScheme.Bookmark));
                ////items.Add(CreateFolderItem(QueryScheme.Pagemark));
            }

            this.Items = items;
        }

        private FolderItem CreateFolderItem(QueryScheme scheme)
        {
            return new ConstFolderItem(new ConstThumbnail(() => scheme.ToImage()), _isOverlayEnabled)
            {
                Source = scheme,
                Type = FolderItemType.Directory,
                Place = Place,
                Name = scheme.ToAliasName(),
                TargetPath = new QueryPath(scheme, null),
                Length = -1,
                Attributes = FolderItemAttribute.Directory | FolderItemAttribute.System,
                IsReady = true
            };
        }
    }

}
