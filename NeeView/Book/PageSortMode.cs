using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // ページ整列
    public enum PageSortMode
    {
        [AliasName]
        FileName,

        [AliasName]
        FileNameDescending,

        [AliasName]
        TimeStamp,

        [AliasName]
        TimeStampDescending,

        [AliasName]
        Size,

        [AliasName]
        SizeDescending,

        [AliasName]
        Entry,

        [AliasName]
        EntryDescending,

        [AliasName]
        Random,
    }

    public static class PageSortModeExtension
    {
        public static PageSortMode GetToggle(this PageSortMode mode)
        {
            return (PageSortMode)(((int)mode + 1) % Enum.GetNames(typeof(PageSortMode)).Length);
        }

        public static bool IsDescending(this PageSortMode mode)
        {
            switch (mode)
            {
                case PageSortMode.FileNameDescending:
                case PageSortMode.TimeStampDescending:
                case PageSortMode.SizeDescending:
                case PageSortMode.EntryDescending:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFileNameCategory(this PageSortMode mode)
        {
            switch (mode)
            {
                case PageSortMode.FileName:
                case PageSortMode.FileNameDescending:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsEntryCategory(this PageSortMode mode)
        {
            switch (mode)
            {
                case PageSortMode.Entry:
                case PageSortMode.EntryDescending:
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum PageSortModeClass
    {
        None,
        Normal,
        WithEntry,
        Full,
    }

    public static class PageSortModeClassExtension
    {
        private static Dictionary<PageSortMode, string> _mapNone;
        private static Dictionary<PageSortMode, string> _mapNormal;
        private static Dictionary<PageSortMode, string> _mapWithEntry;
        private static Dictionary<PageSortMode, string> _mapFull;

        static PageSortModeClassExtension()
        {
            _mapFull = AliasNameExtensions.GetAliasNameDictionary<PageSortMode>();

            _mapWithEntry = _mapFull;

            _mapNormal = _mapWithEntry
                .Where(e => !e.Key.IsEntryCategory())
                .ToDictionary(e => e.Key, e => e.Value);

            _mapNone = _mapNormal
                .Where(e => e.Key == PageSortMode.FileName)
                .ToDictionary(e => e.Key, e => e.Value);
        }

        public static bool Contains(this PageSortModeClass self, PageSortMode mode)
        {
            return self.GetPageSortModeMap().ContainsKey(mode);
        }

        public static Dictionary<PageSortMode, string> GetPageSortModeMap(this PageSortModeClass self)
        {
            switch (self)
            {
                case PageSortModeClass.Full:
                    return _mapFull;
                case PageSortModeClass.WithEntry:
                    return _mapWithEntry;
                case PageSortModeClass.Normal:
                    return _mapNormal;
                default:
                    return _mapNone;
            }
        }

        public static PageSortMode ValidatePageSortMode(this PageSortModeClass self, PageSortMode mode)
        {
            var map = self.GetPageSortModeMap();
            if (map.ContainsKey(mode))
            {
                return mode;
            }
            else
            {
                return mode.IsDescending() ? PageSortMode.FileNameDescending : PageSortMode.FileName;
            }
        }

        public static PageSortMode GetTogglePageSortMode(this PageSortModeClass self, PageSortMode mode)
        {
            while (true)
            {
                mode = mode.GetToggle();
                if (self.Contains(mode))
                {
                    return mode;
                }
            }
        }
    }

}
