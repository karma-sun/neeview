﻿using NeeView.IO;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeeView
{

    /// <summary>
    /// 検索コレクション
    /// </summary>
    public class FolderSearchCollection : FolderCollection, IDisposable
    {
        // Fields

        /// <summary>
        /// 検索結果
        /// </summary>
        private NeeLaboratory.IO.Search.SearchResultWatcher _searchResult;


        // Constructors
        public FolderSearchCollection(QueryPath path, NeeLaboratory.IO.Search.SearchResultWatcher searchResult, bool isActive, bool isOverlayEnabled) : base(path, isActive, isOverlayEnabled)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));
            Debug.Assert(path.Search == searchResult.Keyword);

            _searchResult = searchResult;
        }

        public override async Task InitializeItemsAsync(CancellationToken token)
        {
            await Task.Yield();

            var items = _searchResult.Items
                .Select(e => CreateFolderItem(Place, e))
                .Where(e => e != null)
                .ToList();

            var list = Sort(items).ToList();

            if (!list.Any())
            {
                list.Add(CreateFolderItemEmpty(Place));
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            _searchResult.SearchResultChanged += SearchResult_NodeChanged;
        }


        public override FolderOrderClass FolderOrderClass => FolderOrderClass.WithPath;


        #region Methods

        /// <summary>
        /// 検索結果変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchResult_NodeChanged(object sender, NeeLaboratory.IO.Search.SearchResultChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NeeLaboratory.IO.Search.NodeChangedAction.Add:
                    RequestCreate(new QueryPath(e.Content.Path));
                    break;
                case NeeLaboratory.IO.Search.NodeChangedAction.Remove:
                    RequestDelete(new QueryPath(e.Content.Path));
                    break;
                case NeeLaboratory.IO.Search.NodeChangedAction.Rename:
                    RequestRename(new QueryPath(e.OldPath), new QueryPath(e.Content.Path));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 検索結果からFolderItem作成
        /// </summary>
        /// <param name="nodeContent"></param>
        /// <returns></returns>
        public FolderItem CreateFolderItem(QueryPath parent, NeeLaboratory.IO.Search.NodeContent nodeContent)
        {
            if (nodeContent.FileInfo.IsDirectory)
            {
                return new FileFolderItem(_isOverlayEnabled)
                {
                    Type = FolderItemType.Directory,
                    Place = parent,
                    Name = Path.GetFileName(nodeContent.Path),
                    TargetPath = new QueryPath(nodeContent.Path),
                    LastWriteTime = nodeContent.FileInfo.LastWriteTime,
                    Length = -1,
                    Attributes = FolderItemAttribute.Directory,
                    IsReady = true
                };
            }
            else
            {
                if (FileShortcut.IsShortcut(nodeContent.Path))
                {
                    var shortcut = new FileShortcut(nodeContent.Path);
                    if (shortcut.Target.Exists && (shortcut.Target.Attributes.HasFlag(FileAttributes.Directory) || ArchiverManager.Current.IsSupported(shortcut.TargetPath)))
                    {
                        return CreateFolderItem(parent, shortcut);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (ArchiverManager.Current.IsSupported(nodeContent.Path))
                {
                    // TODO: PlayListのことを考慮せよ
                    return new FileFolderItem(_isOverlayEnabled)
                    {
                        Type = FolderItemType.File,
                        Place = parent,
                        Name = Path.GetFileName(nodeContent.Path),
                        TargetPath = new QueryPath(nodeContent.Path),
                        LastWriteTime = nodeContent.FileInfo.LastWriteTime,
                        Length = nodeContent.FileInfo.Size,
                        IsReady = true
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region IDisposable Support

        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_searchResult != null)
                    {
                        _searchResult.Dispose();
                        _searchResult = null;
                    }
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
