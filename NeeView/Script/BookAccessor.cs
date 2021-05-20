
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 本の状態
    /// </summary>
    public class BookAccessor
    {
        private CancellationToken _cancellationToken;


        [WordNodeMember]
        public string Path => BookOperation.Current.Book?.Address;

        [WordNodeMember]
        public bool IsMedia => BookOperation.Current.Book?.IsMedia == true;

        [WordNodeMember]
        public bool IsNew => BookOperation.Current.Book?.IsNew == true;

        [WordNodeMember(IsAutoCollect = false)]
        public BookConfigAccessor Config { get; } = new BookConfigAccessor();

        [WordNodeMember]
        public PageAccessor[] Pages
        {
            get
            {
                return BookOperation.Current.Book?.Pages.Select(e => new PageAccessor(e)).ToArray() ?? new PageAccessor[] { };
            }
        }

        [WordNodeMember]
        public ViewPageAccessor[] ViewPages
        {
            get
            {
                return BookOperation.Current.Book?.Viewer.ViewPageCollection.Collection.Select(e => new ViewPageAccessor(e.Page)).ToArray() ?? new ViewPageAccessor[] { };
            }
        }


        [WordNodeMember]
        public void Wait()
        {
            BookOperation.Current.Wait(_cancellationToken);
        }

        #region Obsolete

        [Obsolete] // ver.38
        public int PageSize
        {
            get => throw new NotSupportedException("Script: PageSize is obsolete. Use ViewPages.length instead.");
        }

        [Obsolete] // ver.38
        public int ViewPageSize
        {
            get => throw new NotSupportedException("Script: ViewPageSize is obsolete. Use Pages.length instead.");
        }

        [Obsolete] // ver.38
        public PageAccessor Page(int index)
        {
            throw new NotSupportedException("Script: Page() is obsolete. Use Pages[] instead.");
        }

        [Obsolete] // ver.38
        public PageAccessor ViewPage(int index)
        {
            throw new NotSupportedException("Script: ViewPage() is obsolete. Use ViewPages[] instead.");
        }

        #endregion Obsoletet

        internal void SetCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());

            node.Children.Add(Config.CreateWordNode(nameof(Config)));

            return node;
        }
    }
}
