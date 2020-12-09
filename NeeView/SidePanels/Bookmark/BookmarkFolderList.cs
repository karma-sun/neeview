using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// BookmarkFolderList
    /// </summary>
    public class BookmarkFolderList : FolderList
    {
        static BookmarkFolderList() => Current = new BookmarkFolderList();
        public static BookmarkFolderList Current { get; }

        private BookmarkFolderList() : base(false, false, Config.Current.Bookmark)
        {
            ApplicationDisposer.Current.Add(this);

            Config.Current.Bookmark.AddPropertyChanged(nameof(BookmarkConfig.IsSyncBookshelfEnabled), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsSyncBookshelfEnabled));
            });
        }


        public override bool IsSyncBookshelfEnabled
        {
            get => Config.Current.Bookmark.IsSyncBookshelfEnabled;
            set => Config.Current.Bookmark.IsSyncBookshelfEnabled = value;
        }

        public void UpdateItems()
        {
            if (FolderCollection == null)
            {
                RequestPlace(new QueryPath(QueryScheme.Bookmark, null), null, FolderSetPlaceOption.None);
            }
        }

        public override bool CanMoveToParent()
        {
            var parentQuery = FolderCollection?.GetParentQuery();
            if (parentQuery == null) return false;
            return parentQuery.Scheme == QueryScheme.Bookmark;
        }

        public override void Sync()
        {
        }

        public override QueryPath GetFixedHome()
        {
            return new QueryPath(QueryScheme.Bookmark, null, null);
        }

        protected override bool CheckScheme(QueryPath query)
        {
            if (query.Scheme != QueryScheme.Bookmark) throw new NotSupportedException($"need scheme \"{QueryScheme.Bookmark.ToSchemeString()}\"");
            return true;
        }


        #region Memento

        [DataContract]
        public new class Memento
        {
            [DataMember]
            public FolderList.Memento FolderList { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsSyncBookshelfEnabled { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                FolderList.RestoreConfig(config.Bookmark);
                Config.Current.Bookmark.IsSyncBookshelfEnabled = IsSyncBookshelfEnabled;
            }
        }

        #endregion
    }


    // from https://stackoverflow.com/questions/9343594/how-to-call-asynchronous-method-from-synchronous-method-in-c
    internal static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return AsyncHelper._myTaskFactory
              .StartNew<Task<TResult>>(func)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            AsyncHelper._myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
