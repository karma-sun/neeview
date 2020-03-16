using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 最初のブック、フォルダーのロード
    /// </summary>
    public class FirstLoader
    {
        private string _bookPath;
        private string _folderPath;
        private bool _isFolderLink;
        private BookLoadOption _bookLoadOptions;


        public void Load()
        {
            _bookPath = null;
            _folderPath = App.Current.Option.FolderList;

            SetBookPlace();
            SetFolderPlace();
            LoadBook();
            LoadFolder();
        }

        private void SetBookPlace()
        {
            if (App.Current.Option.IsBlank == SwitchOption.on)
            {
                _bookPath = null;
                return;
            }

            // 起動引数の場所で開く
            var path = PlaylistBookLoader.CreateLoadPath(App.Current.Option.Values);
            if (path != null)
            {
                _bookPath = path;
                return;
            }

            // 最後に開いたブックを復元する
            if (Config.Current.StartUp.IsOpenLastBook)
            {
                path = BookHistoryCollection.Current.LastAddress;
                if (path != null)
                {
                    _bookPath = path;
                    _bookLoadOptions = BookLoadOption.Resume | BookLoadOption.IsBook;

                    if (_folderPath == null && BookHistoryCollection.Current.LastFolder != null)
                    {
                        // 前回開いていたフォルダーがブックマークであった場合、なるべくそのフォルダーをひらく
                        if (QueryScheme.Bookmark.IsMatch(BookHistoryCollection.Current.LastFolder))
                        {
                            var node = BookmarkCollection.Current.FindNode(BookHistoryCollection.Current.LastFolder);
                            if (node != null && node.Select(e => e.Value).OfType<Bookmark>().Any(e => e.Place == path))
                            {
                                _folderPath = BookHistoryCollection.Current.LastFolder;
                                _isFolderLink = true;
                                return;
                            }
                        }
                        // 前回開いていたフォルダーがプレイリストあった場合、なるべくそのフォルダーを開く
                        if (PlaylistArchive.IsSupportExtension(BookHistoryCollection.Current.LastFolder))
                        {
                            try
                            {
                                var playlist = PlaylistFile.Load(BookHistoryCollection.Current.LastFolder);
                                if (playlist.Items.Contains(path))
                                {
                                    _folderPath = BookHistoryCollection.Current.LastFolder;
                                    _isFolderLink = true;
                                    return;
                                }
                            }
                            catch
                            {
                                // nop.
                            }
                        }
                    }
                    return;
                }
            }
        }

        private void SetFolderPlace()
        {
            if (_folderPath != null)
            {
                return;
            }

            // Bookが指定されていなければ既定の場所を開く
            if (_bookPath == null)
            {
                // 前回開いていたフォルダーを復元する
                if (BookHistoryCollection.Current.LastFolder != null)
                {
                    _folderPath = BookHistoryCollection.Current.LastFolder;
                    return;
                }

                // ホームフォルダ
                _folderPath = BookshelfFolderList.Current.GetFixedHome().SimpleQuery;
                return;
            }
        }

        private void LoadBook()
        {
            if (_bookPath != null)
            {
                BookHub.Current.RequestLoad(_bookPath, null, _bookLoadOptions, _folderPath == null);
            }
        }

        private void LoadFolder()
        {
            if (_folderPath != null)
            {
                var select = _isFolderLink ? new FolderItemPosition(new QueryPath(_bookPath)) : null;
                BookshelfFolderList.Current.RequestPlace(new QueryPath(_folderPath), select, FolderSetPlaceOption.UpdateHistory);
            }
        }
    }

}
