using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class BookHubTools
    {
        /// <summary>
        /// ブックを開く。複数対応版
        /// </summary>
        /// <remarks>
        /// 複数ファイルの場合はプレイリスト化して本棚の場所として開く
        /// </remarks>
        public static string RequestLoad(object sender, IEnumerable<string> paths)
        {
            return RequestLoad(sender, paths, BookLoadOption.None, true);
        }

        /// <summary>
        /// ブックを開く。複数対応版
        /// </summary>
        /// <remarks>
        /// 複数ファイルの場合はプレイリスト化して本棚の場所として開く
        /// </remarks>
        public static string RequestLoad(object sender, IEnumerable<string> paths, BookLoadOption options, bool isRefreshFolderList)
        {
            if (paths is null || !paths.Any()) return null;

            if (paths.Count() >= 2)
            {
                var path = PlaylistSourceTools.CreateTempPlaylist(paths);
                BookHub.Current.RequestLoad(sender, path, null, options, false);
                if (isRefreshFolderList)
                {
                    BookshelfFolderList.Current.RequestPlace(new QueryPath(path), null, FolderSetPlaceOption.UpdateHistory);
                }
                return path;
            }
            else
            {
                var path = paths.First();
                BookHub.Current.RequestLoad(sender, path, null, options, isRefreshFolderList);
                return path;
            }
        }
    }
}

