using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NeeView
{
    public class Importer : IDisposable
    {
        private ZipArchive _archive;
        private ZipArchiveEntry _settingEntry;
        private ZipArchiveEntry _settingEntryV1;
        private ZipArchiveEntry _historyEntry;
        private ZipArchiveEntry _historyEntryV1;
        private ZipArchiveEntry _bookmarkEntry;
        private ZipArchiveEntry _bookmarkEntryV1;
        private ZipArchiveEntry _pagemarkEntry;
        private ZipArchiveEntry _pagemarkEntryV1;
        private bool _disposedValue;
        private bool _isUserSettingEnabled = true;
        private bool _isHistoryEnabled = false;
        private bool _isBookmarkEnabled = false;
        private bool _isPagemarkEnabled = false;
        private bool _isPlaylistsEnabled = false;
        private bool _isThemesEnabled = false;
        private bool _isScriptsEnabled = false;


        public Importer(string filename)
        {
            this.FileName = filename;
            _archive = ZipFile.OpenRead(filename);

            Initialize();
        }


        public string FileName { get; private set; }

        public bool UserSettingExists { get; set; }

        public bool IsUserSettingEnabled
        {
            get => _isUserSettingEnabled && UserSettingExists;
            set => _isUserSettingEnabled = value;
        }

        public bool HistoryExists { get; set; }

        public bool IsHistoryEnabled
        {
            get => _isHistoryEnabled && HistoryExists;
            set => _isHistoryEnabled = value;
        }

        public bool BookmarkExists { get; set; }

        public bool IsBookmarkEnabled
        {
            get => _isBookmarkEnabled && BookmarkExists;
            set => _isBookmarkEnabled = value;
        }

        public bool PagemarkExists { get; set; }

        public bool IsPagemarkEnabled
        {
            get => _isPagemarkEnabled && PagemarkExists;
            set => _isPagemarkEnabled = value;
        }

        public bool PlaylistsExists { get; set; }

        public bool IsPlaylistsEnabled
        {
            get => _isPlaylistsEnabled && PlaylistsExists;
            set => _isPlaylistsEnabled = value;
        }

        public List<ZipArchiveEntry> PlaylistEntries { get; private set; }

        public bool ThemesExists { get; set; }

        public bool IsThemesEnabled
        {
            get => _isThemesEnabled && ThemesExists;
            set => _isThemesEnabled = value;
        }

        public List<ZipArchiveEntry> ThemeEntries { get; private set; }


        public bool ScriptsExists { get; set; }

        public bool IsScriptsEnabled
        {
            get => _isScriptsEnabled && ScriptsExists;
            set => _isScriptsEnabled = value;
        }

        public List<ZipArchiveEntry> ScriptEntries { get; private set; }


        public void Initialize()
        {
            _settingEntry = _archive.GetEntry(SaveData.UserSettingFileName);
            _settingEntryV1 = _archive.GetEntry(Path.ChangeExtension(SaveData.UserSettingFileName, ".xml"));
            _historyEntry = _archive.GetEntry(SaveData.HistoryFileName);
            _historyEntryV1 = _archive.GetEntry(Path.ChangeExtension(SaveData.HistoryFileName, ".xml"));
            _bookmarkEntry = _archive.GetEntry(SaveData.BookmarkFileName);
            _bookmarkEntryV1 = _archive.GetEntry(Path.ChangeExtension(SaveData.BookmarkFileName, ".xml"));
            _pagemarkEntry = _archive.GetEntry(SaveData.PagemarkFileName);
            _pagemarkEntryV1 = _archive.GetEntry(Path.ChangeExtension(SaveData.PagemarkFileName, ".xml"));

            this.PlaylistEntries = _archive.Entries.Where(e => e.FullName.StartsWith(@"Playlists\")).ToList();
            this.ThemeEntries = _archive.Entries.Where(e => e.FullName.StartsWith(@"Themes\")).ToList();
            this.ScriptEntries = _archive.Entries.Where(e => e.FullName.StartsWith(@"Scripts\")).ToList();

            this.UserSettingExists = _settingEntry != null || _settingEntryV1 != null;
            this.HistoryExists = _historyEntry != null || _historyEntryV1 != null;
            this.BookmarkExists = _bookmarkEntry != null || _bookmarkEntryV1 != null;
            this.PagemarkExists = _pagemarkEntry != null || _pagemarkEntryV1 != null;
            this.PlaylistsExists = PlaylistEntries.Any();
            this.ThemesExists = ThemeEntries.Any();
            this.ScriptsExists = ScriptEntries.Any();
        }

        public void Import()
        {
            MainWindowModel.Current.CloseCommandParameterDialog();
            bool recoverySettingWindow = MainWindowModel.Current.CloseSettingWindow();

            ImportUserSetting();
            ImportHistory();
            ImportBookmark();
            ImportPagemark();
            ImportPlaylists();
            ImportThemes();
            ImportScripts();

            if (recoverySettingWindow)
            {
                MainWindowModel.Current.OpenSettingWindow();
            }
        }

        public void ImportUserSetting()
        {
            if (!this.IsUserSettingEnabled) return;

            UserSetting setting = null;

            if (_settingEntry != null)
            {
                using (var stream = _settingEntry.Open())
                {
                    setting = UserSettingTools.Load(stream);
                }
            }
            else if (_settingEntryV1 != null)
            {
                using (var stream = _settingEntryV1.Open())
                {
                    var settingV1 = UserSettingV1.LoadV1(stream);
                    setting = settingV1.ConvertToV2();
                }
                // 他のファイルの一部設定を反映
                if (_historyEntryV1 != null)
                {
                    using (var stream = _historyEntryV1.Open())
                    {
                        var historyV1 = BookHistoryCollection.Memento.LoadV1(stream);
                        historyV1.RestoreConfig(setting.Config);
                    }
                }
                if (_pagemarkEntryV1 != null)
                {
                    using (var stream = _pagemarkEntryV1.Open())
                    {
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
                        var pagemarkV1 = PagemarkCollection.Memento.LoadV1(stream);
                        pagemarkV1.RestoreConfig(setting.Config);
#pragma warning restore CS0612 // 型またはメンバーが旧型式です
                    }
                }
            }

            if (setting != null)
            {
                Setting.SettingWindow.Current?.Cancel();
                MainWindowModel.Current.CloseCommandParameterDialog();

                setting.Config.Window.State = Config.Current.Window.State; // ウィンドウ状態は維持する
                UserSettingTools.Restore(setting);
            }
        }

        public void ImportHistory()
        {
            if (!this.IsHistoryEnabled) return;

            BookHistoryCollection.Memento history = null;

            if (_historyEntry != null)
            {
                using (var stream = _historyEntry.Open())
                {
                    history = BookHistoryCollection.Memento.Load(stream);
                }
            }
            else if (_historyEntryV1 != null)
            {
                using (var stream = _historyEntryV1.Open())
                {
                    history = BookHistoryCollection.Memento.LoadV1(stream);
                }
            }

            if (history != null)
            {
                BookHistoryCollection.Current.Restore(history, true);
            }
        }

        public void ImportBookmark()
        {
            if (!this.IsBookmarkEnabled) return;

            BookmarkCollection.Memento bookmark = null;

            if (_bookmarkEntry != null)
            {
                using (var stream = _bookmarkEntry.Open())
                {
                    bookmark = BookmarkCollection.Memento.Load(stream);
                }
            }
            else if (_bookmarkEntryV1 != null)
            {
                using (var stream = _bookmarkEntryV1.Open())
                {
                    bookmark = BookmarkCollection.Memento.LoadV1(stream);
                }
            }

            if (bookmark != null)
            {
                BookmarkCollection.Current.Restore(bookmark);
                SaveDataSync.Current.SaveBookmark(true);
            }
        }

        public void ImportPagemark()
        {
            if (!this.IsPagemarkEnabled) return;

#pragma warning disable CS0612 // 型またはメンバーが旧型式です
            PagemarkCollection.Memento pagemark = null;

            if (_pagemarkEntry != null)
            {
                using (var stream = _pagemarkEntry.Open())
                {
                    pagemark = PagemarkCollection.Memento.Load(stream);
                }
            }
            else if (_pagemarkEntryV1 != null)
            {
                using (var stream = _pagemarkEntryV1.Open())
                {
                    pagemark = PagemarkCollection.Memento.LoadV1(stream);
                }
            }
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

            if (pagemark != null)
            {
                PagemarkToPlaylistConverter.SavePagemarkPlaylist(pagemark);

                if (PlaylistHub.Current.SelectedItem == Config.Current.Playlist.DefaultPlaylist && PlaylistHub.Current.Playlist.Items?.Any() != true)
                {
                    PlaylistHub.Current.SelectedItem = Config.Current.Playlist.PagemarkPlaylist;
                }
            }
        }

        public void ImportPlaylists()
        {
            if (!IsPlaylistsEnabled) return;

            var directory = new DirectoryInfo(Config.Current.Playlist.PlaylistFolder);
            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var entry in this.PlaylistEntries)
            {
                var path = Path.Combine(directory.FullName, entry.Name);
                entry.ExtractToFile(path, true);
            }
        }

        public void ImportThemes()
        {
            if (!IsThemesEnabled) return;

            var directory = new DirectoryInfo(Config.Current.Theme.CustomThemeFolder);
            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var entry in this.ThemeEntries)
            {
                var path = Path.Combine(directory.FullName, entry.Name);
                entry.ExtractToFile(path, true);
            }

            // テーマの再適用
            ThemeManager.Current.RefreshThemeColor();
        }

        public void ImportScripts()
        {
            if (!IsScriptsEnabled) return;

            if (string.IsNullOrEmpty(Config.Current.Script.ScriptFolder))
            {
                return;
            }

            var directory = new DirectoryInfo(Config.Current.Script.ScriptFolder);
            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var entry in this.ScriptEntries)
            {
                var path = Path.Combine(directory.FullName, entry.Name);
                entry.ExtractToFile(path, true);
            }

            // スクリプトの再適用
            CommandTable.Current.ScriptManager.UpdateScriptCommands(isForce: true, isReplace: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _archive.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
