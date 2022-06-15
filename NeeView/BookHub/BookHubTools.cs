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

        /// <summary>
        /// このブックはサブフォルダーを読み込む設定？
        /// </summary>
        public static bool IsRecursiveBook(QueryPath query)
        {
            if (query is null) return false;

            // 開いているブックは現状の設定を返す ... これいらない？下の計算でやってる？
            var book = BookHub.Current.Book;
            if (book != null && book.Address == query.SimplePath)
            {
                return book.Source.IsRecursiveFolder;
            }

            // 開いていないブックは履歴と設定から計算する
            var lastBookMemento = book?.Address != null ? book.CreateMemento() : null;
            var loadOption = BookLoadOption.Resume | (IsFolderRecoursive(query.GetParent()) ? BookLoadOption.DefaultRecursive : BookLoadOption.None);
            var setting = BookHub.CreateOpenBookMemento(query.SimplePath, lastBookMemento, loadOption);
            return setting.IsRecursiveFolder;
        }

        /// <summary>
        /// この場所のブックは既定でサブフォルダーを読み込む設定？
        /// </summary>
        public static bool IsFolderRecoursive(QueryPath query)
        {
            var memento = BookHistoryCollection.Current.GetFolderMemento(query.SimplePath);
            return memento.IsFolderRecursive;
        }
    }
}

