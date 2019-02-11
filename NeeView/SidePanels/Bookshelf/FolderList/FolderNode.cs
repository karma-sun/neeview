using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 一時的にフォルダーリスト階層を作る。巡回移動用
    /// </summary>
    /// HACK: パスをQueryPathで管理
    public class FolderNode
    {
        // Fields

        private object _lock = new object();

        private bool _isParentValid;
        private bool _isChildrenValid;
        public Archiver _archiver;


        // Constructors

        public FolderNode(FolderNode parent, string name, FolderItem content)
        {
            Parent = parent;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Content = content;
            _isParentValid = _parent?.Content != null;
        }

        public FolderNode(FolderCollection collection, FolderItem content)
        {
            if (collection is FolderSearchCollection) throw new ArgumentException("collection is not folder entry");

            var parent = new FolderNode(null, collection.Place.FullPath, null);
            parent.Children = collection.Items
                .Where(e => !e.IsEmpty())
                .Select(e => new FolderNode(parent, e.Name, e) { Place = collection.Place.FullPath })
                .ToList();
            parent._isChildrenValid = true;

            if (collection is FolderArchiveCollection archiveCollection)
            {
                parent._archiver = archiveCollection.Archiver;
            }

            // 重複名はターゲットで区別する
            var index = parent.Children.FindIndex(e => e.Name == content.Name && e.Content.TargetPath == content.TargetPath);
            if (index < 0) throw new ArgumentException("collection dont have content");

            this.Parent = parent;
            this.Place = collection.Place.FullPath;
            this.Name = content.Name;
            this.Content = content;
            _isParentValid = true;

            parent.Children[index] = this;
        }


        // Properties

        private FolderNode _parent;
        public FolderNode Parent
        {
            get => _isParentValid ? _parent : throw new InvalidOperationException("parent not initialize");
            private set => _parent = value;
        }

        private List<FolderNode> _children;
        public List<FolderNode> Children
        {
            get => _isChildrenValid ? _children : throw new InvalidOperationException("children not initialize");
            private set => _children = value;
        }

        private string _place;
        public string Place
        {
            get { return _parent != null ? _parent.FullName : _place; }
            set { _place = value; }
        }

        public string Name { get; set; }

        public string FullName => _parent != null ? LoosePath.Combine(_parent.FullName, Name) : Name;

        public FolderItem Content { get; set; }

        
        // Methods

        public async Task<FolderNode> GetParent(CancellationToken cancel)
        {
            if (!_isParentValid)
            {
                var parent = await CreateParent(cancel);
                if (parent != null)
                {
                    var name = LoosePath.GetFileName(FullName, parent.FullName);
                    var index = parent.Children.FindIndex(e => e.Name == name); // 重複名は区別できていない
                    if (index < 0) throw new KeyNotFoundException();

                    lock (_lock)
                    {
                        if (!_isParentValid)
                        {
                            this.Parent = parent;
                            this.Place = null;
                            this.Content = Content ?? parent.Children[index].Content;
                            parent.Children[index] = this;
                            _isParentValid = true;
                        }
                    }
                }
                else
                {
                    _isParentValid = true;
                }
            }

            return Parent;
        }

        private async Task<FolderNode> CreateParent(CancellationToken cancel)
        {
            if (_isParentValid) return Parent;

            FolderNode parent;
            string name;

            // archive folder
            var parentArchiver = _archiver?.Parent;
            if (parentArchiver != null)
            {
                parent = new FolderNode(null, new QueryPath(parentArchiver.SystemPath).FullPath, null) { _archiver = parentArchiver };
                name = LoosePath.GetFileName(Name, parent.FullName);
            }

            // normal folder
            else
            {
                var directory = LoosePath.GetDirectoryName(Name);
                if (string.IsNullOrEmpty(directory))
                {
                    Parent = null;
                    _isParentValid = true;
                    return null;
                }

                parent = new FolderNode(null, directory, null);
                name = LoosePath.GetFileName(Name);
            }

            // 子の生成
            await parent.GetChildren(cancel);

            return parent;
        }


        public async Task<List<FolderNode>> GetChildren(CancellationToken cancel)
        {
            if (!_isChildrenValid)
            {
                var children = await CreateChildren(cancel);

                lock (_lock)
                {
                    if (!_isChildrenValid)
                    {
                        _isChildrenValid = true;
                        Children = children;
                    }
                }

                cancel.ThrowIfCancellationRequested();
            }

            return Children;
        }

        private async Task<List<FolderNode>> CreateChildren(CancellationToken cancel)
        {
            if (_isChildrenValid) return Children;

            if (Content != null && !Content.CanOpenFolder())
            {
                return new List<FolderNode>();
            }

            var path = new QueryPath(FullName);
            if (Content != null && Content.Attributes.HasFlag(FolderItemAttribute.Shortcut))
            {
                path = Content.TargetPath;
            }

            using (var collection = await BookshelfFolderList.Current.FolderCollectionFactory.CreateFolderCollectionAsync(path, false, cancel))
            {
                var children = collection.Items
                    .Where(e => !e.IsEmpty())
                    .Select(e => new FolderNode(this, e.Name, e) { Place = collection.Place.FullPath })
                    .ToList();

                return children;
            }
        }


        public async Task<FolderNode> CruisePrev(CancellationToken cancel)
        {
            var parent = await GetParent(cancel);
            if (parent == null) return null;

            // 兄の末裔
            var brother = await parent.GetChildren(cancel);
            var index = brother.IndexOf(this);
            if (index - 1 >= 0)
            {
                return await brother[index - 1].CruiseDescendant(cancel);
            }

            // 親
            await parent.GetParent(cancel); // コンテンツ確定
            return parent;
        }

        private async Task<FolderNode> CruiseDescendant(CancellationToken cancel)
        {
            var children = await GetChildren(cancel);
            if (children.Count > 0)
            {
                return await children.Last().CruiseDescendant(cancel);
            }
            else
            {
                return this;
            }
        }


        public async Task<FolderNode> CruiseNext(CancellationToken cancel)
        {
            // 長男
            var children = await GetChildren(cancel);
            if (children.Count > 0)
            {
                return children.First();
            }

            return await CruiseNextUp(cancel);
        }

        private async Task<FolderNode> CruiseNextUp(CancellationToken cancel)
        {
            var parent = await GetParent(cancel);
            if (parent == null) return null;

            // 弟
            var brother = await parent.GetChildren(cancel);
            var index = brother.IndexOf(this);
            if (index + 1 < brother.Count)
            {
                return brother[index + 1];
            }

            // 親へ
            return await parent.CruiseNextUp(cancel);
        }
    }
}
