using System;
using System.Diagnostics;
using NeeLaboratory.IO;

namespace NeeView
{
    /// <summary>
    /// ブックマーク、ページマークは変更のたびに保存。
    /// 他プロセスからの要求でリロードを行う。
    /// </summary>
    public class SaveDataSync
    {
        // Note: Initialize()必須
        static SaveDataSync() => Current = new SaveDataSync();
        public static SaveDataSync Current { get; }


        private DelayAction _delaySaveBookmark;
        private DelayAction _delaySavePagemark;


        private SaveDataSync()
        {
            _delaySaveBookmark = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.2), SaveBookmark, TimeSpan.FromSeconds(0.5));
            _delaySavePagemark = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.2), SavePagemark, TimeSpan.FromSeconds(0.5));

            RemoteCommandService.Current.AddReciever("LoadUserSetting", LoadUserSetting);
            RemoteCommandService.Current.AddReciever("LoadHistory", LoadHistory);
            RemoteCommandService.Current.AddReciever("LoadBookmark", LoadBookmark);
            RemoteCommandService.Current.AddReciever("LoadPagemark", LoadPagemark);
        }


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
            SaveData.Current.LoadUserSetting();
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

        // 他のNeeViewにUserSettingの再読み込みを要求
        public void RequestEveryoneToLoadUserSetting()
        {
            Debug.WriteLine($"Request reload UserSetting");
            RemoteCommandService.Current.Send(new RemoteCommand("LoadUserSetting"), RemoteCommandDelivery.All);
        }

        public void SaveUserSetting()
        {
            Debug.WriteLine($"Save UserSetting");
            SaveData.Current.SaveUserSetting();
        }

        public void SaveHistory()
        {
            Debug.WriteLine($"Save History");
            // TODO: 更新されている履歴のマージ
            SaveData.Current.SaveHistory();
        }

        public void SaveBookmark()
        {
            Debug.WriteLine($"Save Bookmark");
            SaveData.Current.SaveBookmark();
            RemoteCommandService.Current.Send(new RemoteCommand("LoadBookmark"), RemoteCommandDelivery.All);
        }

        public void RemoveBookmarkIfNotSave()
        {
            SaveData.Current.RemoveBookmarkIfNotSave();
        }

        public void SavePagemark()
        {
            Debug.WriteLine($"Save Pagemark");
            SaveData.Current.SavePagemark();
            RemoteCommandService.Current.Send(new RemoteCommand("LoadPagemark"), RemoteCommandDelivery.All);
        }

        public void RemovePagemarkIfNotSave()
        {
            SaveData.Current.RemovePagemarkIfNotSave();
        }
    }
}
