using Microsoft.Win32;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NeeView
{
    public class SaveData
    {
        public static SaveData Current { get; private set; } = new SaveData();


        public UserSetting _userSetting;
        private string _historyFileName;
        private string _bookmarkFileName;
        private string _pagemarkFileName;
        private string _oldPagemarkFileName;
        private object _saveLock = new object();


        public SaveData()
        {
            _historyFileName = System.IO.Path.Combine(Config.Current.LocalApplicationDataPath, HistoryFileName);
            _bookmarkFileName = System.IO.Path.Combine(Config.Current.LocalApplicationDataPath, BookmarkFileName);
            _pagemarkFileName = System.IO.Path.Combine(Config.Current.LocalApplicationDataPath, PagemarkFileName);

            _oldPagemarkFileName = System.IO.Path.Combine(Config.Current.LocalApplicationDataPath, "Pagekmark.xml");
        }


        public const string UserSettingFileName = "UserSetting.xml";
        public const string HistoryFileName = "History.xml";
        public const string BookmarkFileName = "Bookmark.xml";
        public const string PagemarkFileName = "Pagemark.xml";

        public bool IsEnableSave { get; set; } = true;


        // アプリ設定作成
        public UserSetting CreateSetting()
        {
            var setting = new UserSetting();

            App.Current.WindowChromeFrame = WindowShape.Current.WindowChromeFrame;
            setting.App = App.Current.CreateMemento();

            setting.SusieMemento = SusieContext.Current.CreateMemento();
            setting.CommandMememto = CommandTable.Current.CreateMemento();
            setting.DragActionMemento = DragActionTable.Current.CreateMemento();

            setting.Memento = Models.Current.CreateMemento();

            return setting;
        }

        // アプリ設定反映
        public void RestoreSetting(UserSetting setting)
        {
            App.Current.Restore(setting.App);
            WindowShape.Current.WindowChromeFrame = App.Current.WindowChromeFrame;

            SusieContext.Current.Restore(setting.SusieMemento);
            CommandTable.Current.Restore(setting.CommandMememto, false);
            DragActionTable.Current.Restore(setting.DragActionMemento);

            Models.Current.Resore(setting.Memento);
        }


#pragma warning disable CS0612

        // アプリ設定反映(互換用)
        public void RestoreSettingCompatible(UserSetting setting)
        {
            if (setting == null) return;

            if (setting.ViewMemento != null)
            {
                MainWindowVM.RestoreCompatible(setting.ViewMemento);
            }

            if (setting.BookHubMemento != null)
            {
                BookHub.Current.Restore(setting.BookHubMemento);
                BookHub.Current.RestoreCompatible(setting.BookHubMemento);
            }

            if (setting.ImageEffectMemento != null)
            {
                Models.Current.ImageEffect.Restore(setting.ImageEffectMemento);
            }

            if (setting.ExporterMemento != null)
            {
                Exporter.RestoreCompatible(setting.ExporterMemento);
            }

            // Preference.Compatible
            if (setting.PreferenceMemento != null)
            {
                var preference = new Preference();
                preference.Restore(setting.PreferenceMemento);
                preference.RestoreCompatible();
            }

            // Model.Compatible
            Models.Current.ResoreCompatible(setting.Memento);
        }

#pragma warning restore CS0612


        // 履歴読み込み
        public void LoadHistory()
        {
            try
            {
                App.Current.SemaphoreWait();
                BookHistoryCollection.Memento memento = SafetyLoad(BookHistoryCollection.Memento.Load, _historyFileName, Resources.NotifyLoadHistoryFailed, Resources.NotifyLoadHistoryFailedTitle);
                RestoreHistory(memento);
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        // 履歴反映
        private void RestoreHistory(BookHistoryCollection.Memento memento)
        {
            BookHistoryCollection.Current.Restore(memento, true);
            MenuBar.Current.UpdateLastFiles();
        }


        // ブックマーク読み込み
        public void LoadBookmark()
        {
            try
            {
                App.Current.SemaphoreWait();
                BookmarkCollection.Memento memento = SafetyLoad(BookmarkCollection.Memento.Load, _bookmarkFileName, Resources.NotifyLoadBookmarkFailed, Resources.NotifyLoadBookmarkFailedTitle);
                BookmarkCollection.Current.Restore(memento);
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        // ページマーク読み込み
        public void LoadPagemark()
        {
            // 旧ファイル名の変更
            try
            {
                if (!File.Exists(_pagemarkFileName) && File.Exists(_oldPagemarkFileName))
                {
                    File.Move(_oldPagemarkFileName, _pagemarkFileName);
                }
            }
            catch { }

            try
            {
                App.Current.SemaphoreWait();
                PagemarkCollection.Memento memento = SafetyLoad(PagemarkCollection.Memento.Load, _pagemarkFileName, Resources.NotifyLoadPagemarkFailed, Resources.NotifyLoadPagemarkFailedTitle);
                PagemarkCollection.Current.Restore(memento);
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        /// <summary>
        /// 設定の読み込み
        /// 先行して設定ファイルのみ取得するため
        /// </summary>
        public UserSetting LoadUserSetting()
        {
            if (_userSetting != null)
            {
                return _userSetting;
            }

            try
            {
                App.Current.SemaphoreWait();
                _userSetting = SafetyLoad(UserSetting.Load, App.Current.Option.SettingFilename, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                return _userSetting;
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        /// <summary>
        /// 設定の取得
        /// </summary>
        public UserSetting GetUserSetting()
        {
            return _userSetting;
        }

        /// <summary>
        /// 設定領域の開放
        /// </summary>
        public void ReleaseUserSetting()
        {
            _userSetting = null;
        }

        /// <summary>
        /// 設定読み込みと反映
        /// </summary>
        public void LoadAndApplyUserSetting()
        {
            Setting.SettingWindow.Current?.Cancel();

            try
            {
                App.Current.SemaphoreWait();
                var setting = SafetyLoad(UserSetting.Load, App.Current.Option.SettingFilename, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                RestoreSetting(setting);
                RestoreSettingCompatible(setting);
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }


        //
        public void SaveUserSetting()
        {
            if (!IsEnableSave) return;

            // 設定
            var setting = CreateSetting();

            // ウィンドウ状態保存
            setting.WindowShape = WindowShape.Current.SnapMemento;

            // ウィンドウ座標保存
            setting.WindowPlacement = WindowPlacement.Current.CreateMemento();

            // 設定をファイルに保存
            try
            {
                App.Current.SemaphoreWait();
                SafetySave(setting.Save, App.Current.Option.SettingFilename, App.Current.IsSettingBackup);
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        // 履歴をファイルに保存
        public void SaveHistory()
        {
            if (!IsEnableSave) return;

            // 現在の本を履歴に登録
            BookHub.Current.SaveBookMemento(); // TODO: タイミングに問題有り？

            try
            {
                App.Current.SemaphoreWait();
                if (App.Current.IsSaveHistory)
                {
                    var bookHistoryMemento = BookHistoryCollection.Current.CreateMemento(true);
                    SafetySave(bookHistoryMemento.Save, _historyFileName, false);
                }
                else
                {
                    FileIO.RemoveFile(_historyFileName);
                }
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        /// <summary>
        /// Bookmarkの保存
        /// </summary>
        public void SaveBookmark()
        {
            if (!IsEnableSave) return;

            try
            {
                App.Current.SemaphoreWait();
                if (App.Current.IsSaveBookmark)
                {
                    var bookmarkMemento = BookmarkCollection.Current.CreateMemento();
                    SafetySave(bookmarkMemento.Save, _bookmarkFileName, false);
                }
                else
                {
                    FileIO.RemoveFile(_bookmarkFileName);
                }
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        /// <summary>
        /// Pagemarkの保存
        /// </summary>
        public void SavePagemark()
        {
            if (!IsEnableSave) return;

            try
            {
                App.Current.SemaphoreWait();
                if (App.Current.IsSavePagemark)
                {
                    var pagemarkMemento = PagemarkCollection.Current.CreateMemento();
                    SafetySave(pagemarkMemento.Save, _pagemarkFileName, false);
                }
                else
                {
                    FileIO.RemoveFile(_pagemarkFileName);
                }
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        /// <summary>
        /// アプリ強制終了でもファイルがなるべく破壊されないような保存
        /// </summary>
        private void SafetySave(Action<string> save, string path, bool isBackup)
        {
            try
            {
                var oldPath = path + ".old";
                var tmpPath = path + ".tmp";

                FileIO.RemoveFile(tmpPath);
                save(tmpPath);

                lock (App.Current.Lock)
                {
                    var newFile = new FileInfo(tmpPath);
                    var oldFile = new FileInfo(path);

                    if (oldFile.Exists)
                    {
                        FileIO.RemoveFile(oldPath);
                        oldFile.MoveTo(oldPath);
                    }

                    newFile.MoveTo(path);

                    if (!isBackup)
                    {
                        FileIO.RemoveFile(oldPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 正規ファイルの読み込みに失敗したらバックアップからの復元を試みる
        /// </summary>
        private T SafetyLoad<T>(Func<string, T> load, string path, string failedMessage, string failedTitle)
            where T : new()
        {
            var old = path + ".old";

            try
            {
                if (File.Exists(path))
                {
                    try
                    {
                        return load(path);
                    }
                    catch
                    {
                        if (File.Exists(old))
                        {
                            return load(old);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else if (File.Exists(old))
                {
                    return load(old);
                }
                else
                {
                    return new T();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                new MessageDialog(failedMessage, failedTitle).ShowDialog();
                return new T();
            }
        }


        #region Backup

        private const string backupDialogDefaultExt = ".nvzip";
        private const string backupDialogFilder = "NeeView Backup (.nvzip)|*.nvzip";


        /// <summary>
        /// バックアップファイルの出力
        /// </summary>
        public void ExportBackup()
        {
            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.OverwritePrompt = true;
            dialog.AddExtension = true;
            dialog.FileName = $"NeeView{Config.Current.DispVersion}-{DateTime.Now.ToString("yyyyMMdd")}";
            dialog.DefaultExt = backupDialogDefaultExt;
            dialog.Filter = backupDialogFilder;
            dialog.Title = Resources.DialogExportTitle;

            if (dialog.ShowDialog(MainWindow.Current) == true)
            {
                try
                {
                    SaveDataSync.Current.Flush();
                    SaveBackupFile(dialog.FileName);
                }
                catch (Exception ex)
                {
                    new MessageDialog($"{Resources.WordCause}: {ex.Message}", Resources.DialogExportErrorTitle).ShowDialog();
                }
            }
        }

        // バックアップファイル作成
        public void SaveBackupFile(string filename)
        {
            // 保存
            WindowShape.Current.CreateSnapMemento();
            SaveDataSync.Current.SaveUserSetting(false);
            SaveDataSync.Current.SaveHistory();

            try
            {
                // 保存されたファイルをzipにまとめて出力
                using (ZipArchive archive = new ZipArchive(new FileStream(filename, FileMode.Create, FileAccess.ReadWrite), ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(App.Current.Option.SettingFilename, UserSettingFileName);

                    if (File.Exists(_historyFileName))
                    {
                        archive.CreateEntryFromFile(_historyFileName, HistoryFileName);
                    }
                    if (File.Exists(_bookmarkFileName))
                    {
                        archive.CreateEntryFromFile(_bookmarkFileName, BookmarkFileName);
                    }
                    if (File.Exists(_pagemarkFileName))
                    {
                        archive.CreateEntryFromFile(_pagemarkFileName, PagemarkFileName);
                    }
                }
            }
            catch (Exception)
            {
                // 中途半端なファイルは削除
                if (File.Exists(filename))
                {
                    Debug.WriteLine($"Delete {filename}");
                    File.Delete(filename);
                }

                throw;
            }
        }



        /// <summary>
        /// バックアップ復元
        /// </summary>
        public void ImportBackup()
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.AddExtension = true;
            dialog.DefaultExt = backupDialogDefaultExt;
            dialog.Filter = backupDialogFilder;
            dialog.Title = Resources.DialogImportTitle;

            if (dialog.ShowDialog(MainWindow.Current) == true)
            {
                try
                {
                    LoadBackupFile(dialog.FileName);
                }
                catch (Exception ex)
                {
                    new MessageDialog($"{Resources.WordCause}: {ex.Message}", Resources.DialogImportErrorTitle).ShowDialog();
                }
            }
        }

        // バックアップファイル復元
        public void LoadBackupFile(string filename)
        {
            UserSetting setting = null;
            BookHistoryCollection.Memento history = null;
            BookmarkCollection.Memento bookmark = null;
            PagemarkCollection.Memento pagemark = null;

            var selector = new BackupSelectControl();
            selector.FileNameTextBlock.Text = $"{Resources.WordImport}: {Path.GetFileName(filename)}";

            using (var archiver = ZipFile.OpenRead(filename))
            {
                var settingEntry = archiver.GetEntry(UserSettingFileName);
                var historyEntry = archiver.GetEntry(HistoryFileName);
                var bookmarkEntry = archiver.GetEntry(BookmarkFileName);
                var pagemarkEntry = archiver.GetEntry(PagemarkFileName);

                // 選択
                {
                    if (settingEntry != null)
                    {
                        selector.UserSettingCheckBox.IsEnabled = true;
                        selector.UserSettingCheckBox.IsChecked = true;
                    }
                    if (historyEntry != null)
                    {
                        selector.HistoryCheckBox.IsEnabled = true;
                        selector.HistoryCheckBox.IsChecked = true;
                    }
                    if (bookmarkEntry != null)
                    {
                        selector.BookmarkCheckBox.IsEnabled = true;
                        selector.BookmarkCheckBox.IsChecked = true;
                    }
                    if (pagemarkEntry != null)
                    {
                        selector.PagemarkCheckBox.IsEnabled = true;
                        selector.PagemarkCheckBox.IsChecked = true;
                    }

                    var dialog = new MessageDialog(selector, Resources.DialogImportSelectTitle);
                    dialog.Commands.Add(new UICommand(Resources.WordImport));
                    dialog.Commands.Add(UICommands.Cancel);
                    var answer = dialog.ShowDialog();

                    if (answer != dialog.Commands[0]) return;
                }

                // 読み込み
                if (selector.UserSettingCheckBox.IsChecked == true)
                {
                    using (var stream = settingEntry.Open())
                    {
                        setting = UserSetting.Load(stream);
                    }
                }

                if (selector.HistoryCheckBox.IsChecked == true)
                {
                    using (var stream = historyEntry.Open())
                    {
                        history = BookHistoryCollection.Memento.Load(stream);
                    }
                }

                if (selector.BookmarkCheckBox.IsChecked == true)
                {
                    using (var stream = bookmarkEntry.Open())
                    {
                        bookmark = BookmarkCollection.Memento.Load(stream);
                    }
                }

                if (selector.PagemarkCheckBox.IsChecked == true)
                {
                    using (var stream = pagemarkEntry.Open())
                    {
                        pagemark = PagemarkCollection.Memento.Load(stream);
                    }
                }
            }

            bool recoverySettingWindow = MainWindowModel.Current.CloseSettingWindow();

            // 適用
            if (setting != null)
            {
                Setting.SettingWindow.Current?.Cancel();
                RestoreSetting(setting);
                RestoreSettingCompatible(setting);
            }

            // 履歴読み込み
            if (history != null)
            {
                RestoreHistory(history);
            }

            // ブックマーク読み込み
            if (bookmark != null)
            {
                BookmarkCollection.Current.Restore(bookmark);
                SaveBookmark();
            }

            // ページマーク読込
            if (pagemark != null)
            {
                PagemarkCollection.Current.Restore(pagemark);
                SavePagemark();
            }

            if (recoverySettingWindow)
            {
                MainWindowModel.Current.OpenSettingWindow();
            }
        }

        #endregion
    }
}
