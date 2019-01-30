using System;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// ブックマーク、ページマークは変更のたびに保存。
    /// ブックマーク、ページマークの保存データの変更を監視、更新されれば再読込。
    /// </summary>
    public class SaveDataSync
    {
        public static SaveDataSync Current { get; private set; }

        private System.IO.FileSystemWatcher _watcher;
        private volatile bool _isPagemarkSaving;
        private volatile bool _isBookmarkSaving;
        private volatile bool _isUserSettingSaving;
        private volatile bool _isHistorySaving;

        private DelayAction _delaySaveBookmark;
        private DelayAction _delaySavePagemark;

        private bool _isUserSettingChanged;
        private bool _isHistoryChanged;

        public SaveDataSync()
        {
            Current = this;

            _delaySaveBookmark = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.2), SaveBookmark, TimeSpan.FromSeconds(0.5));
            _delaySavePagemark = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.2), SavePagemark, TimeSpan.FromSeconds(0.5));

            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
            PagemarkCollection.Current.PagemarkChanged += PagemarkCollection_PagemarkChanged;

            _watcher = new System.IO.FileSystemWatcher();
            _watcher.Path = Config.Current.LocalApplicationDataPath;
            _watcher.NotifyFilter = System.IO.NotifyFilters.FileName;
            _watcher.Filter = "*.xml";
            _watcher.Renamed += new System.IO.RenamedEventHandler(FileSystemWatcher_Renamed);
            _watcher.EnableRaisingEvents = true;
        }


        public void Flush()
        {
            _delaySaveBookmark.Flush();
            _delaySavePagemark.Flush();
        }

        /// <summary>
        /// 保存データ監視
        /// 履歴ファイルは変更情報のみ保持
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void FileSystemWatcher_Renamed(object source, System.IO.RenamedEventArgs e)
        {
            if (e.Name == SaveData.BookmarkFileName && !_isBookmarkSaving)
            {
                Debug.WriteLine($"{SaveData.BookmarkFileName} is updated by other process.");
                App.Current.Dispatcher.Invoke(() => SaveData.Current.LoadBookmark(SaveData.Current.UserSetting));
            }
            else if (e.Name == SaveData.PagemarkFileName && !_isPagemarkSaving)
            {
                Debug.WriteLine($"{SaveData.PagemarkFileName} is updated by other process.");
                App.Current.Dispatcher.Invoke(() => SaveData.Current.LoadPagemark(SaveData.Current.UserSetting));
            }
            else if (e.Name == SaveData.UserSettingFileName && !_isUserSettingSaving)
            {
                Debug.WriteLine($"{SaveData.UserSettingFileName} is updated by other process.");
                ////App.Current.Dispatcher.Invoke(() => SaveData.Current.LoadUserSetting(false));
                _isUserSettingChanged = true;
                // TODO: 必要ならば設定の更新を行う
            }
            else if (e.Name == SaveData.HistoryFileName && !_isHistorySaving)
            {
                Debug.WriteLine($"{SaveData.HistoryFileName} is updated by other process.");
                _isHistoryChanged = true;
            }
        }

        private void BookmarkCollection_BookmarkChanged(object sender, BookmarkCollectionChangedEventArgs e)
        {
            if (e.Action == EntryCollectionChangedAction.Reset) return;
            _delaySaveBookmark.Request();
        }

        private void SaveBookmark()
        {
            Debug.WriteLine($"Save Bookmark");
            try
            {
                _isBookmarkSaving = true;
                SaveData.Current.SaveBookmark();
            }
            finally
            {
                _isBookmarkSaving = false;
            }
        }

        private void PagemarkCollection_PagemarkChanged(object sender, PagemarkCollectionChangedEventArgs e)
        {
            if (e.Action == EntryCollectionChangedAction.Reset) return;
            _delaySavePagemark.Request();
        }

        private void SavePagemark()
        {
            Debug.WriteLine($"Save Pagemark");
            try
            {
                _isPagemarkSaving = true;
                SaveData.Current.SavePagemark();
            }
            finally
            {
                _isPagemarkSaving = false;
            }
        }


        public void SaveUserSetting()
        {
            Debug.WriteLine($"Save UserSetting");
            try
            {
                _isUserSettingSaving = true;
                SaveData.Current.SaveUserSetting();
                _isUserSettingChanged = false;
            }
            finally
            {
                _isUserSettingSaving = false;
            }
        }

        public void SaveHistory()
        {
            Debug.WriteLine($"Save History");
            try
            {
                // TODO: 更新されている履歴のマージ
                _isHistorySaving = true;
                SaveData.Current.SaveHistory();
                _isHistoryChanged = false;
            }
            finally
            {
                _isHistorySaving = false;
            }
        }


        // ファイルシステム監視を停止
        public void StopFileSystemWatcher()
        {
            _watcher.Dispose();
        }
    }
}
