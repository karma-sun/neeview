using NeeView.Collections.Generic;

namespace NeeView
{
    public static class PagemarkUtility
    {
        public static bool CanPagemark(Page page)
        {
            if (page.ContentAccessor is MediaContent)
            {
                return false;
            }

            if (page.BookAddress.StartsWith(Temporary.Current.TempDirectory))
            {
                return false;
            }

            if (page.BookAddress.StartsWith(QueryScheme.Pagemark.ToSchemeString()))
            {
                return false;
            }

            return true;
        }

        public static bool IsPagemarked(Page page)
        {
            return GetPagemark(page) != null;
        }

        public static Pagemark GetPagemark(Page page)
        {
            if (page is null) return null;

            var place = LoosePath.TrimEnd(page.BookAddress);
            return PagemarkCollection.Current.Find(place, page.EntryFullName);
        }

        public static Pagemark AddPagemark(Page page)
        {
            return AddPagemark(page.BookAddress, page.EntryFullName);
        }

        public static bool RemovePagemark(Page page)
        {
            return RemovePagemark(page.BookAddress, page.EntryFullName);
        }

        public static Pagemark SetPagemark(Page page, bool isPagemark)
        {
            if (isPagemark)
            {
                return AddPagemark(page);
            }
            else
            {
                RemovePagemark(page);
                return null;
            }
        }

        public static Pagemark TogglePagemark(Page page)
        {
            return SetPagemark(page, GetPagemark(page) is null);
        }

        private static Pagemark AddPagemark(string place, string entryName)
        {
            if (place == null) return null;
            if (entryName == null) return null;

            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node == null)
            {
                // TODO: 登録時にサムネイルキャッシュにも登録
                var pagemark = new Pagemark(place, entryName);
                PagemarkCollection.Current.Add(new TreeListNode<IPagemarkEntry>(pagemark));
                return pagemark;
            }
            else
            {
                return node.Value as Pagemark;
            }
        }

        private static bool RemovePagemark(string place, string entryName)
        {
            if (place == null) return false;
            if (entryName == null) return false;

            var node = PagemarkCollection.Current.FindNode(place, entryName);
            if (node != null)
            {
                return PagemarkCollection.Current.Remove(node);
            }
            else
            {
                return false;
            }
        }
    }
}
