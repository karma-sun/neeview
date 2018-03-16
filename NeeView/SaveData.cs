using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class SaveData
    {
        public static SaveData Current { get; private set; }

        public UserSetting UserSetting { get; set; }

        public bool IsEnableSave { get; set; } = true;

        private string _historyFileName { get; set; }
        private string _bookmarkFileName { get; set; }
        private string _pagemarkFileName { get; set; }

        private string _oldPagemarkFileName { get; set; }

        private object _saveLock = new object();

        public const string UserSettingFileName = "UserSetting.xml";
        public const string HistoryFileName = "History.xml";
        public const string BookmarkFileName = "Bookmark.xml";
        public const string PagemarkFileName = "Pagemark.xml";

        //
        public SaveData()
        {
            Current = this;

            _historyFileName = System.IO.Path.Combine(Config.Current.LocalApplicationDataPath, HistoryFileName);
            _bookmarkFileName = System.IO.Path.Combine(Config.Current.LocalApplicationDataPath, BookmarkFileName);
            _pagemarkFileName = System.IO.Path.Combine(Config.Current.LocalApplicationDataPath, PagemarkFileName);

            _oldPagemarkFileName = System.IO.Path.Combine(Config.Current.LocalApplicationDataPath, "Pagekmark.xml");
        }

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
        public void RestoreSetting(UserSetting setting, bool fromLoad)
        {
            App.Current.Restore(setting.App);
            WindowShape.Current.WindowChromeFrame = App.Current.WindowChromeFrame;

            SusieContext.Current.Restore(setting.SusieMemento);
            CommandTable.Current.Restore(setting.CommandMememto, false);
            DragActionTable.Current.Restore(setting.DragActionMemento);

            Models.Current.Resore(setting.Memento, fromLoad);
        }

#pragma warning disable CS0612

        //
        public void RestoreSettingCompatible(UserSetting setting, bool fromLoad)
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
                Models.Current.ImageEffect.Restore(setting.ImageEffectMemento, fromLoad);
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
        public void LoadHistory(UserSetting setting)
        {
            BookHistory.Memento memento = SafetyLoad(BookHistory.Memento.Load, _historyFileName, "履歴の読み込みに失敗しました", "履歴の読み込みに失敗しました。");

#pragma warning disable CS0612

            // compatible: 設定ファイルに残っている履歴をマージ
            if (setting.BookHistoryMemento != null)
            {
                memento.Merge(setting.BookHistoryMemento);
            }

#pragma warning restore CS0612

            RestoreHistory(memento);
        }

        // 履歴反映
        private void RestoreHistory(BookHistory.Memento memento)
        {
            // 履歴反映
            BookHistory.Current.Restore(memento, true);
            MenuBar.Current.UpdateLastFiles();

            // フォルダーリストの場所に反映
            Models.Current.FolderList.ResetPlace(BookHistory.Current.LastFolder);
        }


        // ブックマーク読み込み
        public void LoadBookmark(UserSetting setting)
        {
            BookmarkCollection.Memento memento = SafetyLoad(BookmarkCollection.Memento.Load, _bookmarkFileName, "ブックマークの読み込みに失敗しました", "ブックマークの読み込みに失敗しました。");

            // ブックマーク反映
            BookmarkCollection.Current.Restore(memento);
        }

        // ページマーク読み込み
        public void LoadPagemark(UserSetting setting)
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

            // ページマーク読み込み
            PagemarkCollection.Memento memento = SafetyLoad(PagemarkCollection.Memento.Load, _pagemarkFileName, "ページマークの読み込みに失敗しました", "ページマークの読み込みに失敗しました。");

            // ページマーク反映
            PagemarkCollection.Current.Restore(memento);
        }

        // アプリ設定読み込み
        public void LoadSetting(string filename)
        {
            this.UserSetting = SafetyLoad(UserSetting.Load, filename, "設定の読み込みに失敗しました。初期設定で起動します。", "設定の読み込みに失敗しました。");
        }

        // 全データ保存
        public void SaveAll()
        {
            if (!IsEnableSave) return;

            lock (_saveLock)
            {
                Debug.WriteLine(">> SAVE");
                SaveAllInner();
            }
        }

        private void SaveAllInner()
        {
            // 現在の本を履歴に登録
            BookHub.Current.SaveBookMemento(); // TODO: タイミングに問題有り？

            // 設定
            var setting = CreateSetting();

            // ウィンドウ状態保存
            setting.WindowShape = WindowShape.Current.SnapMemento;

            // ウィンドウ座標保存
            setting.WindowPlacement = WindowPlacement.Current.CreateMemento();

            try
            {
                // 設定をファイルに保存
                SafetySave(setting.Save, App.Current.Option.SettingFilename, App.Current.IsSettingBackup);
            }
            catch { }

            // 保存しないフラグ
            bool disableSave = App.Current.IsDisableSave;

            try
            {
                if (disableSave)
                {
                    // 履歴ファイルを削除
                    FileIO.RemoveFile(_historyFileName);
                }
                else
                {
                    // 履歴をファイルに保存
                    var bookHistoryMemento = BookHistory.Current.CreateMemento(true);
                    SafetySave(bookHistoryMemento.Save, _historyFileName, false);
                }
            }
            catch { }

            try
            {
                if (disableSave)
                {
                    // ブックマークファイルを削除
                    FileIO.RemoveFile(_bookmarkFileName);
                }
                else
                {
                    // ブックマークをファイルに保存
                    var bookmarkMemento = BookmarkCollection.Current.CreateMemento(true);
                    SafetySave(bookmarkMemento.Save, _bookmarkFileName, false);
                }
            }
            catch { }

            try
            {
                if (disableSave)
                {
                    // ページマークファイルを削除
                    FileIO.RemoveFile(_pagemarkFileName);
                }
                else
                {
                    // ページマークをファイルに保存
                    var pagemarkMemento = PagemarkCollection.Current.CreateMemento(true);
                    SafetySave(pagemarkMemento.Save, _pagemarkFileName, false);
                }
            }
            catch { }
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
            dialog.FileName = $"NeeView{Config.Current.ProductVersion}-{DateTime.Now.ToString("yyyyMMdd")}";
            dialog.DefaultExt = backupDialogDefaultExt;
            dialog.Filter = backupDialogFilder;
            dialog.Title = "全設定をエクスポート";

            if (dialog.ShowDialog(MainWindow.Current) == true)
            {
                try
                {
                    SaveBackupFile(dialog.FileName);
                }
                catch (Exception ex)
                {
                    new MessageDialog($"原因: {ex.Message}", "エクスポートに失敗しました").ShowDialog();
                }
            }
        }

        // バックアップファイル作成
        public void SaveBackupFile(string filename)
        {
            // 保存
            WindowShape.Current.CreateSnapMemento();
            SaveAll();

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
            dialog.Title = "全設定をインポート";

            if (dialog.ShowDialog(MainWindow.Current) == true)
            {
                try
                {
                    LoadBackupFile(dialog.FileName);
                }
                catch (Exception ex)
                {
                    new MessageDialog($"原因: {ex.Message}", "インポートに失敗しました").ShowDialog();
                }
            }
        }

        // バックアップファイル復元
        public void LoadBackupFile(string filename)
        {
            UserSetting setting = null;
            BookHistory.Memento history = null;
            BookmarkCollection.Memento bookmark = null;
            PagemarkCollection.Memento pagemark = null;

            var selector = new BackupSelectControl();
            selector.FileNameTextBlock.Text = $"インポート: {Path.GetFileName(filename)}";

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

                    var dialog = new MessageDialog(selector, "インポートする項目を選択してください");
                    dialog.Commands.Add(new UICommand("インポート"));
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
                        history = BookHistory.Memento.Load(stream);
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
                RestoreSetting(setting, true);
                RestoreSettingCompatible(setting, true);
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
            }

            // ページマーク読込
            if (pagemark != null)
            {
                PagemarkCollection.Current.Restore(pagemark);
            }

            if (recoverySettingWindow)
            {
                MainWindowModel.Current.OpenSettingWindow();
            }
        }

        #endregion
    }
}
