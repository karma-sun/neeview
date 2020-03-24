using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace NeeView
{
    /// <summary>
    /// BookshelfFolderList
    /// </summary>
    public class BookshelfFolderList : FolderList
    {
        static BookshelfFolderList() => Current = new BookshelfFolderList();
        public static BookshelfFolderList Current { get; }


        private Regex _excludeRegex;


        private BookshelfFolderList() : base(true, true, Config.Current.Layout.Bookshelf)
        {
            ApplicationDisposer.Current.Add(this);

            Config.Current.System.AddPropertyChanged(nameof(SystemConfig.IsHiddenFileVisibled), async (s, e) =>
            {
                await RefreshAsync(true, true);
            });

            Config.Current.Layout.Bookshelf.AddPropertyChanged(nameof(BookshelfPanelConfig.IsHistoryMark), (s, e) =>
            {
                FolderCollection?.RefreshIcon(null);
            });

            Config.Current.Layout.Bookshelf.AddPropertyChanged(nameof(BookshelfPanelConfig.IsBookmarkMark), (s, e) =>
            {
                FolderCollection?.RefreshIcon(null);
            });

            Config.Current.Layout.Bookshelf.AddPropertyChanged(nameof(BookshelfPanelConfig.ExcludePattern), (s, e) =>
            {
                UpdateExcludeRegex();
            });

            Config.Current.Layout.Bookshelf.AddPropertyChanged(nameof(BookshelfPanelConfig.IsSearchIncludeSubdirectories), (s, e) =>
            {
                RequestSearchPlace(true);
            });
        }

        // 除外パターンの正規表現
        public Regex ExcludeRegex
        {
            get { return _excludeRegex; }
            set { SetProperty(ref _excludeRegex, value); }
        }


        public override QueryPath GetFixedHome()
        {
            var path = new QueryPath(Config.Current.Layout.Bookshelf.Home);

            switch (path.Scheme)
            {
                case QueryScheme.Root:
                    return path;

                case QueryScheme.File:
                    if (Directory.Exists(Config.Current.Layout.Bookshelf.Home))
                    {
                        return path;
                    }
                    else
                    {
                        return GetDefaultHome();
                    }

                case QueryScheme.Bookmark:
                    if (BookmarkCollection.Current.FindNode(Config.Current.Layout.Bookshelf.Home)?.Value is BookmarkFolder)
                    {
                        return path;
                    }
                    else
                    {
                        return new QueryPath(QueryScheme.Bookmark, null, null);
                    }

                default:
                    Debug.WriteLine($"Not support yet: {Config.Current.Layout.Bookshelf.Home}");
                    return GetDefaultHome();
            }
        }

        private QueryPath GetDefaultHome()
        {
            var myPicture = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            if (Directory.Exists(myPicture))
            {
                return new QueryPath(myPicture);
            }

            // 救済措置
            return new QueryPath(System.Environment.CurrentDirectory);
        }

        public override async void Sync()
        {
            var address = BookHub.Current?.Book?.Address;

            if (address != null)
            {
                // TODO: Queryの求め方はこれでいいのか？
                var path = new QueryPath(address);
                var parent = new QueryPath(BookHub.Current?.Book?.Source.GetFolderPlace() ?? LoosePath.GetDirectoryName(address));

                SetDarty(); // 強制更新
                await SetPlaceAsync(parent, new FolderItemPosition(path), FolderSetPlaceOption.Focus | FolderSetPlaceOption.UpdateHistory | FolderSetPlaceOption.ResetKeyword | FolderSetPlaceOption.FileSystem);

                this.FolderListBoxModel.RaiseSelectedItemChanged(true);
            }
            else if (Place != null)
            {
                SetDarty(); // 強制更新
                await SetPlaceAsync(Place, null, FolderSetPlaceOption.Focus | FolderSetPlaceOption.FileSystem);

                this.FolderListBoxModel.RaiseSelectedItemChanged(true);
            }

            if (Config.Current.Layout.Bookshelf.IsSyncFolderTree && Place != null)
            {
                BookshelfFolderTreeModel.Current.SyncDirectory(Place.SimplePath);
            }
        }

        protected override void CloseBookIfNecessary()
        {
            if (Config.Current.Layout.Bookshelf.IsCloseBookWhenMove)
            {
                BookHub.Current.RequestUnload(true);
            }
        }

        protected override bool IsCruise()
        {
            return Config.Current.Layout.Bookshelf.IsCruise;
        }

        // 除外パターンの正規表現を更新
        private void UpdateExcludeRegex()
        {
            try
            {
                ExcludeRegex = string.IsNullOrWhiteSpace(Config.Current.Layout.Bookshelf.ExcludePattern) ? null : new Regex(Config.Current.Layout.Bookshelf.ExcludePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FolderList exclute: {ex.Message}");
                ExcludeRegex = null;
            }
        }

        protected override bool IsIncrementalSearchEnabled()
        {
            return Config.Current.Layout.Bookshelf.IsIncrementalSearchEnabled;
        }

        protected override bool IsSearchIncludeSubdirectories()
        {
            return Config.Current.Layout.Bookshelf.IsSearchIncludeSubdirectories;
        }



        #region Memento

        [DataContract]
        public new class Memento
        {
            [DataMember]
            public FolderList.Memento FolderList { get; set; }

            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }

            [DataMember]
            public string Home { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsInsertItem { get; set; }

            [DataMember]
            public bool IsMultipleRarFilterEnabled { get; set; }

            [DataMember]
            public string ExcludePattern { get; set; }

            [DataMember]
            public bool IsCruise { get; set; }

            [DataMember]
            public bool IsCloseBookWhenMove { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsIncrementalSearchEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSearchIncludeSubdirectories { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsOpenNextBookWhenRemove { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                FolderList.RestoreConfig(config.Layout.Bookshelf);

                config.Layout.Bookshelf.Home = Home;
                config.Layout.Bookshelf.IsHistoryMark = IsVisibleHistoryMark;
                config.Layout.Bookshelf.IsBookmarkMark = IsVisibleBookmarkMark;
                config.Layout.Bookshelf.IsSyncFolderTree = FolderList.IsSyncFolderTree;
                config.Layout.Bookshelf.IsCloseBookWhenMove = IsCloseBookWhenMove;
                config.Layout.Bookshelf.IsOpenNextBookWhenRemove = IsOpenNextBookWhenRemove;
                config.Layout.Bookshelf.IsInsertItem = IsInsertItem;
                config.Layout.Bookshelf.IsMultipleRarFilterEnabled = IsMultipleRarFilterEnabled;
                config.Layout.Bookshelf.ExcludePattern = ExcludePattern;
                config.Layout.Bookshelf.IsCruise = IsCruise;
                config.Layout.Bookshelf.IsIncrementalSearchEnabled = IsIncrementalSearchEnabled;
                config.Layout.Bookshelf.IsSearchIncludeSubdirectories = IsSearchIncludeSubdirectories;
            }
        }

        public new Memento CreateMemento()
        {
            var memento = new Memento();

            memento.FolderList = base.CreateMemento();
            memento.IsVisibleHistoryMark = Config.Current.Layout.Bookshelf.IsHistoryMark;
            memento.IsVisibleBookmarkMark = Config.Current.Layout.Bookshelf.IsBookmarkMark;
            memento.Home = Config.Current.Layout.Bookshelf.Home;
            memento.IsInsertItem = Config.Current.Layout.Bookshelf.IsInsertItem;
            memento.IsMultipleRarFilterEnabled = Config.Current.Layout.Bookshelf.IsMultipleRarFilterEnabled;
            memento.ExcludePattern = Config.Current.Layout.Bookshelf.ExcludePattern;
            memento.IsCruise = Config.Current.Layout.Bookshelf.IsCruise;
            memento.IsCloseBookWhenMove = Config.Current.Layout.Bookshelf.IsCloseBookWhenMove;
            memento.IsIncrementalSearchEnabled = Config.Current.Layout.Bookshelf.IsIncrementalSearchEnabled;
            memento.IsSearchIncludeSubdirectories = Config.Current.Layout.Bookshelf.IsSearchIncludeSubdirectories;
            memento.IsOpenNextBookWhenRemove = Config.Current.Layout.Bookshelf.IsOpenNextBookWhenRemove;

            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            ////base.Restore(memento.FolderList);
            ////this.IsVisibleHistoryMark = memento.IsVisibleHistoryMark;
            ////this.IsVisibleBookmarkMark = memento.IsVisibleBookmarkMark;
            ////this.Home = memento.Home;
            ////this.IsInsertItem = memento.IsInsertItem;
            ////this.IsMultipleRarFilterEnabled = memento.IsMultipleRarFilterEnabled;
            ////this.ExcludePattern = memento.ExcludePattern;
            ////this.IsCruise = memento.IsCruise;
            ////this.IsCloseBookWhenMove = memento.IsCloseBookWhenMove;
            ////this.IsIncrementalSearchEnabled = memento.IsIncrementalSearchEnabled;
            ////this.IsSearchIncludeSubdirectories = memento.IsSearchIncludeSubdirectories;
            ////this.IsOpenNextBookWhenRemove = memento.IsOpenNextBookWhenRemove;
        }

        #endregion
    }

}
