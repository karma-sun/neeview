using Microsoft.Win32;
using NeeView.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace NeeView
{
    /// <summary>
    /// 保存データのインポート、エクスポート
    /// </summary>
    public class SaveDataBackup
    {
        static SaveDataBackup() => Current = new SaveDataBackup();
        public static SaveDataBackup Current { get; }

        private SaveDataBackup()
        {
        }

        private const string backupDialogDefaultExt = ".nvzip";
        private const string backupDialogFilder = "NeeView Backup (.nvzip)|*.nvzip";

        /// <summary>
        /// バックアップファイルの出力
        /// </summary>
        public void ExportBackup()
        {
            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            dialog.OverwritePrompt = true;
            dialog.AddExtension = true;
            dialog.FileName = $"NeeView{Environment.DispVersion}-{DateTime.Now.ToString("yyyyMMdd")}";
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
            ////WindowShape.Current.CreateSnapMemento();
            SaveDataSync.Current.SaveUserSetting(false);
            SaveDataSync.Current.SaveHistory();
            SaveDataSync.Current.SaveBookmark(false);
            SaveDataSync.Current.SavePagemark(false);
            SaveDataSync.Current.RemoveBookmarkIfNotSave();
            SaveDataSync.Current.RemovePagemarkIfNotSave();

            try
            {
                // 保存されたファイルをzipにまとめて出力
                using (ZipArchive archive = new ZipArchive(new FileStream(filename, FileMode.Create, FileAccess.ReadWrite), ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(App.Current.Option.SettingFilename, SaveData.UserSettingFileName);

                    if (File.Exists(SaveData.Current.HistoryFilePath))
                    {
                        archive.CreateEntryFromFile(SaveData.Current.HistoryFilePath, SaveData.HistoryFileName);
                    }
                    if (File.Exists(SaveData.Current.BookmarkFilePath))
                    {
                        archive.CreateEntryFromFile(SaveData.Current.BookmarkFilePath, SaveData.BookmarkFileName);
                    }
                    if (File.Exists(SaveData.Current.PagemarkFilePath))
                    {
                        archive.CreateEntryFromFile(SaveData.Current.PagemarkFilePath, SaveData.PagemarkFileName);
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
            dialog.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
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
                var settingEntry = archiver.GetEntry(SaveData.UserSettingFileName);
                var historyEntry = archiver.GetEntry(SaveData.HistoryFileName);
                var bookmarkEntry = archiver.GetEntry(SaveData.BookmarkFileName);
                var pagemarkEntry = archiver.GetEntry(SaveData.PagemarkFileName);

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

            MainWindowModel.Current.CloseCommandParameterDialog();
            bool recoverySettingWindow = MainWindowModel.Current.CloseSettingWindow();

            // 適用
            if (setting != null)
            {
                Setting.SettingWindow.Current?.Cancel();
                SaveData.Current.RestoreSetting(setting);
                SaveData.Current.RestoreSettingWindowShape(setting);
            }

            // 履歴読み込み
            if (history != null)
            {
                BookHistoryCollection.Current.Restore(history, true);
            }

            // ブックマーク読み込み
            if (bookmark != null)
            {
                BookmarkCollection.Current.Restore(bookmark);
                SaveDataSync.Current.SaveBookmark(true);
            }

            // ページマーク読込
            if (pagemark != null)
            {
                PagemarkCollection.Current.Restore(pagemark);
                SaveDataSync.Current.SavePagemark(true);
            }

            if (recoverySettingWindow)
            {
                MainWindowModel.Current.OpenSettingWindow();
            }
        }
    }
}
