﻿using NeeLaboratory.ComponentModel;
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


        private BookshelfFolderList() : base(true, true, Config.Current.Bookshelf)
        {
            ApplicationDisposer.Current.Add(this);

            Config.Current.System.AddPropertyChanged(nameof(SystemConfig.IsHiddenFileVisibled), async (s, e) =>
            {
                await RefreshAsync(true, true);
            });

            Config.Current.Bookshelf.AddPropertyChanged(nameof(BookshelfConfig.IsHistoryMark), (s, e) =>
            {
                FolderCollection?.RefreshIcon(null);
            });

            Config.Current.Bookshelf.AddPropertyChanged(nameof(BookshelfConfig.IsBookmarkMark), (s, e) =>
            {
                FolderCollection?.RefreshIcon(null);
            });

            Config.Current.Bookshelf.AddPropertyChanged(nameof(BookshelfConfig.ExcludePattern), (s, e) =>
            {
                UpdateExcludeRegex();
            });

            Config.Current.Bookshelf.AddPropertyChanged(nameof(BookshelfConfig.IsSearchIncludeSubdirectories), (s, e) =>
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
            var path = new QueryPath(Config.Current.Bookshelf.Home);

            switch (path.Scheme)
            {
                case QueryScheme.Root:
                    return path;

                case QueryScheme.File:
                    if (Directory.Exists(Config.Current.Bookshelf.Home))
                    {
                        return path;
                    }
                    else
                    {
                        return GetDefaultHome();
                    }

                case QueryScheme.Bookmark:
                    if (BookmarkCollection.Current.FindNode(Config.Current.Bookshelf.Home)?.Value is BookmarkFolder)
                    {
                        return path;
                    }
                    else
                    {
                        return new QueryPath(QueryScheme.Bookmark, null, null);
                    }

                default:
                    Debug.WriteLine($"Not support yet: {Config.Current.Bookshelf.Home}");
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

            if (Config.Current.Bookshelf.IsSyncFolderTree && Place != null)
            {
                BookshelfFolderTreeModel.Current.SyncDirectory(Place.SimplePath);
            }
        }

        protected override void CloseBookIfNecessary()
        {
            if (Config.Current.Bookshelf.IsCloseBookWhenMove)
            {
                BookHub.Current.RequestUnload(true);
            }
        }

        protected override bool IsCruise()
        {
            return Config.Current.Bookshelf.IsCruise;
        }

        // 除外パターンの正規表現を更新
        private void UpdateExcludeRegex()
        {
            try
            {
                ExcludeRegex = string.IsNullOrWhiteSpace(Config.Current.Bookshelf.ExcludePattern) ? null : new Regex(Config.Current.Bookshelf.ExcludePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FolderList exclute: {ex.Message}");
                ExcludeRegex = null;
            }
        }

        protected override bool IsIncrementalSearchEnabled()
        {
            return Config.Current.Bookshelf.IsIncrementalSearchEnabled;
        }

        protected override bool IsSearchIncludeSubdirectories()
        {
            return Config.Current.Bookshelf.IsSearchIncludeSubdirectories;
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
                FolderList.RestoreConfig(config.Bookshelf);

                config.Bookshelf.Home = Home;
                config.Bookshelf.IsHistoryMark = IsVisibleHistoryMark;
                config.Bookshelf.IsBookmarkMark = IsVisibleBookmarkMark;
                config.Bookshelf.IsSyncFolderTree = FolderList.IsSyncFolderTree;
                config.Bookshelf.IsCloseBookWhenMove = IsCloseBookWhenMove;
                config.Bookshelf.IsOpenNextBookWhenRemove = IsOpenNextBookWhenRemove;
                config.Bookshelf.IsInsertItem = IsInsertItem;
                config.Bookshelf.IsMultipleRarFilterEnabled = IsMultipleRarFilterEnabled;
                config.Bookshelf.ExcludePattern = ExcludePattern;
                config.Bookshelf.IsCruise = IsCruise;
                config.Bookshelf.IsIncrementalSearchEnabled = IsIncrementalSearchEnabled;
                config.Bookshelf.IsSearchIncludeSubdirectories = IsSearchIncludeSubdirectories;
            }
        }

        public new Memento CreateMemento()
        {
            var memento = new Memento();

            memento.FolderList = base.CreateMemento();
            memento.IsVisibleHistoryMark = Config.Current.Bookshelf.IsHistoryMark;
            memento.IsVisibleBookmarkMark = Config.Current.Bookshelf.IsBookmarkMark;
            memento.Home = Config.Current.Bookshelf.Home;
            memento.IsInsertItem = Config.Current.Bookshelf.IsInsertItem;
            memento.IsMultipleRarFilterEnabled = Config.Current.Bookshelf.IsMultipleRarFilterEnabled;
            memento.ExcludePattern = Config.Current.Bookshelf.ExcludePattern;
            memento.IsCruise = Config.Current.Bookshelf.IsCruise;
            memento.IsCloseBookWhenMove = Config.Current.Bookshelf.IsCloseBookWhenMove;
            memento.IsIncrementalSearchEnabled = Config.Current.Bookshelf.IsIncrementalSearchEnabled;
            memento.IsSearchIncludeSubdirectories = Config.Current.Bookshelf.IsSearchIncludeSubdirectories;
            memento.IsOpenNextBookWhenRemove = Config.Current.Bookshelf.IsOpenNextBookWhenRemove;

            return memento;
        }

        #endregion
    }

}
