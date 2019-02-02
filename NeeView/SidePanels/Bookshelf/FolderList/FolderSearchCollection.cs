﻿using NeeView.IO;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{

    /// <summary>
    /// 検索コレクション
    /// </summary>
    public class FolderSearchCollection : FolderCollection, IDisposable
    {
        // Fieds

        /// <summary>
        /// 検索結果
        /// </summary>
        private NeeLaboratory.IO.Search.SearchResultWatcher _searchResult;


        // Constructors

        public FolderSearchCollection(QueryPath path, NeeLaboratory.IO.Search.SearchResultWatcher searchResult, bool isActive) : base(path, isActive)
        {
            if (searchResult == null) throw new ArgumentNullException(nameof(searchResult));
            Debug.Assert(path.Search == searchResult.Keyword);

            var items = searchResult.Items
                .Select(e => CreateFolderItem(e))
                .Where(e => e != null)
                .ToList();

            var list = Sort(items).ToList();

            if (!list.Any())
            {
                list.Add(CreateFolderItemEmpty());
            }

            this.Items = new ObservableCollection<FolderItem>(list);
            BindingOperations.EnableCollectionSynchronization(this.Items, new object());

            _searchResult = searchResult;
            _searchResult.SearchResultChanged += SearchResult_NodeChanged;
        }


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
        public FolderItem CreateFolderItem(NeeLaboratory.IO.Search.NodeContent nodeContent)
        {
            if (nodeContent.FileInfo.IsDirectory)
            {
                return new FileFolderItem()
                {
                    Type = FolderItemType.Directory,
                    Place = new QueryPath(Path.GetDirectoryName(nodeContent.Path)),
                    Name = Path.GetFileName(nodeContent.Path),
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
                        return CreateFolderItem(shortcut);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (ArchiverManager.Current.IsSupported(nodeContent.Path))
                {
                    return new FileFolderItem()
                    {
                        Type = FolderItemType.File,
                        Place = new QueryPath(Path.GetDirectoryName(nodeContent.Path)),
                        Name = Path.GetFileName(nodeContent.Path),
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