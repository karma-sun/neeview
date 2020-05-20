using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// ページユーティリティ
    /// </summary>
    public static class PageUtility
    {
        /// <summary>
        /// ページ群の実ファイルリストに変換可能か
        /// </summary>
        public static bool CanCreateRealizedFilePathList(IEnumerable<Page> pages)
        {
            return pages.All(e => e.Entry.IsFileSystem || !e.ContentAccessor.Entry.IsArchiveDirectory());
        }

        /// <summary>
        /// ページ群の実ファイルリストを取得
        /// </summary>
        public static List<string> CreateRealizedFilePathList(IEnumerable<Page> pages, CancellationToken token)
        {
            var files = new List<string>();

            foreach (var page in pages)
            {
                token.ThrowIfCancellationRequested();

                if (page.Entry.IsFileSystem)
                {
                    files.Add(page.GetFilePlace());
                }
                else if (!page.ContentAccessor.Entry.IsArchiveDirectory())
                {
                    files.Add(page.ContentAccessor.CreateTempFile(true).Path);
                }
                else
                {
                    Debug.WriteLine($"CreateRealizedFilePathList: Not support archive folder: {page.EntryFullName}");
                }
            }

            return files;
        }
    }
}
