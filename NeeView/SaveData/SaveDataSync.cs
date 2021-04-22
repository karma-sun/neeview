using System;
using System.Diagnostics;
using NeeLaboratory.IO;
using NeeView.Data;
using NeeView.Threading;

namespace NeeView
{
    /// <summary>
    /// ブックマーク、ページマークは変更のたびに保存。
    /// 他プロセスからの要求でリロードを行う。
    /// </summary>
    public class SaveDataSync : IDisposable
    {
        // Note: Initialize()必須
        static SaveDataSync() => Current = new SaveDataSync();
        public static SaveDataSync Current { get; }


        private DelayAction _delaySaveBookmark;
        private DelayAction _delaySavePagemark;


        private SaveDataSync()
        {
            _delaySaveBookmark = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.2), () => SaveBookmark(true), TimeSpan.FromSeconds(0.5));
            _delaySavePagemark = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.2), () => SavePagemark(true), TimeSpan.FromSeconds(0.5));

            RemoteCommandService.Current.AddReciever("LoadUserSetting", LoadUserSetting);
            RemoteCommandService.Current.AddReciever("LoadHistory", LoadHistory);
            RemoteCommandService.Current.AddReciever("LoadBookmark", LoadBookmark);
            RemoteCommandService.Current.AddReciever("LoadPagemark", LoadPagemark);
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _delaySaveBookmark.Dispose();
                    _delaySavePagemark.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        public void Initialize()
        {
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
            PagemarkCollection.Current.PagemarkChanged += PagemarkCollection_PagemarkChanged;
        }

        private void BookmarkCollection_BookmarkChanged(object sender, BookmarkCollectionChangedEventArgs e)
        {
            if (e.Action == EntryCollectionChangedAction.Reset) return;
            _delaySaveBookmark.Request();
        }

        private void PagemarkCollection_PagemarkChanged(object sender, PagemarkCollectionChangedEventArgs e)
        {
            if (e.Action == EntryCollectionChangedAction.Reset) return;
            _delaySavePagemark.Request();
        }

        public void Flush()
        {
            _delaySaveBookmark.Flush();
            _delaySavePagemark.Flush();
        }

        private void LoadUserSetting(RemoteCommand command)
        {
            Debug.WriteLine($"{SaveData.UserSettingFileName} is updated by other process.");
            var setting = SaveData.Current.LoadUserSetting(false);
            UserSettingTools.Restore(setting);
        }

        private void LoadHistory(RemoteCommand command)
        {
            throw new NotImplementedException();
            // TODO: フラグ管理のみ？
        }

        private void LoadBookmark(RemoteCommand command)
        {
            Debug.WriteLine($"{SaveData.BookmarkFileName} is updated by other process.");
            SaveData.Current.LoadBookmark();
        }

        private void LoadPagemark(RemoteCommand command)
        {
            Debug.WriteLine($"{SaveData.PagemarkFileName} is updated by other process.");
            SaveData.Current.LoadPagemark();
        }

        public void SaveUserSetting(bool sync)
        {
            Debug.WriteLine($"Save UserSetting");

            SaveData.Current.SaveUserSetting();

            // TODO: 動作検証用に古い形式のデータも保存する
            ////SaveData.Current.SaveUserSettingV1();

            if (sync)
            {
                RemoteCommandService.Current.Send(new RemoteCommand("LoadUserSetting"), RemoteCommandDelivery.All);
            }
        }

        public void SaveHistory()
        {
            Debug.WriteLine($"Save History");
            SaveData.Current.SaveHistory();
        }

        public void SaveBookmark(bool sync)
        {
            Debug.WriteLine($"Save Bookmark");
            SaveData.Current.SaveBookmark();
            if (sync)
            {
                RemoteCommandService.Current.Send(new RemoteCommand("LoadBookmark"), RemoteCommandDelivery.All);
            }
        }

        public void RemoveBookmarkIfNotSave()
        {
            SaveData.Current.RemoveBookmarkIfNotSave();
        }

        public void SavePagemark(bool sync)
        {
            Debug.WriteLine($"Save Pagemark");
            SaveData.Current.SavePagemark();
            if (sync)
            {
                RemoteCommandService.Current.Send(new RemoteCommand("LoadPagemark"), RemoteCommandDelivery.All);
            }
        }

        public void RemovePagemarkIfNotSave()
        {
            SaveData.Current.RemovePagemarkIfNotSave();
        }

        /// <summary>
        /// すべてのセーブ処理を行う
        /// </summary>
        public void SaveAll(bool sync)
        {
            Flush();
            SaveUserSetting(sync);
            SaveHistory();
            SaveBookmark(sync);
            SavePagemark(sync);
            RemoveBookmarkIfNotSave();
            RemovePagemarkIfNotSave();

            PlaylistModel.Current.Flush();
        }
    }
}
