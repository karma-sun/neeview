using NeeView.Effects;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace NeeView
{
    public class SaveData
    {
        static SaveData() => Current = new SaveData();
        public static SaveData Current { get; }

        private string _settingFilenameToDelete;
        private string _historyFilenameToDelete;
        private string _bookmarkFilenameToDelete;
        private string _pagemarkFilenameToDelete;

        // 設定のバックアップを１起動に付き１回に制限するフラグ
        private bool _keepBackup = false;


        private SaveData()
        {
        }

        public const string UserSettingFileName = "UserSetting.json";
        public const string HistoryFileName = "History.json";
        public const string BookmarkFileName = "Bookmark.json";
        public const string PagemarkFileName = "Pagemark.json";
        public const string CustomThemeFileName = "CustomTheme.json";
        public const string PlaylistsFolder = "Playlists";

        public static string DefaultHistoryFilePath => Path.Combine(Environment.LocalApplicationDataPath, HistoryFileName);
        public static string DefaultBookmarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, BookmarkFileName);
        public static string DefaultPagemarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, PagemarkFileName);
        public static string DefaultCustomThemeFilePath => Path.Combine(Environment.LocalApplicationDataPath, CustomThemeFileName);
        public static string DefaultPlaylistsFolder => Path.Combine(Environment.LocalApplicationDataPath, PlaylistsFolder);


        public string UserSettingFilePath => App.Current.Option.SettingFilename;
        public string HistoryFilePath => Config.Current.History.HistoryFilePath;
        public string BookmarkFilePath => Config.Current.Bookmark.BookmarkFilePath;

        public bool IsEnableSave { get; set; } = true;


        #region Load

        /// <summary>
        /// 設定の読み込み
        /// </summary>
        public UserSetting LoadUserSetting(bool cancellable)
        {
            if (App.Current.IsMainWindowLoaded)
            {
                Setting.SettingWindow.Current?.Cancel();
                MainWindowModel.Current.CloseCommandParameterDialog();
            }

            UserSetting setting;

            try
            {
                App.Current.SemaphoreWait();

                var filename = App.Current.Option.SettingFilename;
                var extension = Path.GetExtension(filename).ToLower();
                var filenameV1 = Path.ChangeExtension(filename, ".xml");

                var failedDialog = new LoadFailedDialog(Resources.Notice_LoadSettingFailed, Resources.Notice_LoadSettingFailedTitle);
                failedDialog.OKCommand = new UICommand(Resources.Notice_LoadSettingFailedButtonContinue) { IsPositibe = true };
                if (cancellable)
                {
                    failedDialog.CancelCommand = new UICommand(Resources.Notice_LoadSettingFailedButtonQuit) { Alignment = UICommandAlignment.Left };
                }

                if (extension == ".json" && File.Exists(filename))
                {
                    setting = SafetyLoad(UserSettingTools.Load, filename, failedDialog, true, LoadUserSettingBackupCallback);
                }
                // before v.37
                else if (File.Exists(filenameV1))
                {
                    var settingV1 = SafetyLoad(UserSettingV1.LoadV1, filenameV1, failedDialog, true);
                    var settingV1Converted = settingV1.ConvertToV2();

                    var historyV1FilePath = Path.ChangeExtension(settingV1.App.HistoryFilePath ?? DefaultHistoryFilePath, ".xml");
                    var historyV1 = SafetyLoad(BookHistoryCollection.Memento.LoadV1, historyV1FilePath, null); // 一部の履歴設定を反映
                    historyV1?.RestoreConfig(settingV1Converted.Config);

#pragma warning disable CS0612 // 型またはメンバーが旧型式です
                    var pagemarkV1FilePath = Path.ChangeExtension(settingV1.App.PagemarkFilePath ?? DefaultPagemarkFilePath, ".xml");
                    var pagemarkV1 = SafetyLoad(PagemarkCollection.Memento.LoadV1, pagemarkV1FilePath, null); // 一部のページマーク設定を反映
                    pagemarkV1?.RestoreConfig(settingV1Converted.Config);
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

                    _settingFilenameToDelete = filenameV1;
                    if (Path.GetExtension(App.Current.Option.SettingFilename).ToLower() == ".xml")
                    {
                        App.Current.Option.SettingFilename = Path.ChangeExtension(App.Current.Option.SettingFilename, ".json");
                    }

                    setting = settingV1Converted;
                }
                else
                {
                    setting = new UserSetting();
                }
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }

            return setting;
        }

        private void LoadUserSettingBackupCallback()
        {
            _keepBackup = true;
        }


        // 履歴読み込み
        public void LoadHistory()
        {
            try
            {
                App.Current.SemaphoreWait();

                var filename = HistoryFilePath;
                var extension = Path.GetExtension(filename).ToLower();
                var filenameV1 = Path.ChangeExtension(filename, ".xml");
                var failedDialog = new LoadFailedDialog(Resources.Notice_LoadHistoryFailed, Resources.Notice_LoadHistoryFailedTitle);

                if (extension == ".json" && File.Exists(filename))
                {
                    BookHistoryCollection.Memento memento = SafetyLoad(BookHistoryCollection.Memento.Load, HistoryFilePath, failedDialog);
                    BookHistoryCollection.Current.Restore(memento, true);
                }
                // before v.37
                else if (File.Exists(filenameV1))
                {
                    BookHistoryCollection.Memento memento = SafetyLoad(BookHistoryCollection.Memento.LoadV1, filenameV1, failedDialog);
                    BookHistoryCollection.Current.Restore(memento, true);

                    _historyFilenameToDelete = filenameV1;
                    if (Path.GetExtension(HistoryFilePath).ToLower() == ".xml")
                    {
                        Config.Current.History.HistoryFilePath = Path.ChangeExtension(HistoryFilePath, ".json");
                    }
                }
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        // ブックマーク読み込み
        public void LoadBookmark()
        {
            try
            {
                App.Current.SemaphoreWait();

                var filename = BookmarkFilePath;
                var extension = Path.GetExtension(filename).ToLower();
                var filenameV1 = Path.ChangeExtension(filename, ".xml");
                var failedDialog = new LoadFailedDialog(Resources.Notice_LoadBookmarkFailed, Resources.Notice_LoadBookmarkFailedTitle);

                if (extension == ".json" && File.Exists(filename))
                {
                    BookmarkCollection.Memento memento = SafetyLoad(BookmarkCollection.Memento.Load, filename, failedDialog);
                    BookmarkCollection.Current.Restore(memento);
                }
                // before v.37
                else if (File.Exists(filenameV1))
                {
                    BookmarkCollection.Memento memento = SafetyLoad(BookmarkCollection.Memento.LoadV1, filenameV1, failedDialog);
                    BookmarkCollection.Current.Restore(memento);

                    _bookmarkFilenameToDelete = filenameV1;
                    if (Path.GetExtension(BookmarkFilePath).ToLower() == ".xml")
                    {
                        Config.Current.Bookmark.BookmarkFilePath = Path.ChangeExtension(BookmarkFilePath, ".json");
                    }
                }
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        /// <summary>
        /// 正規ファイルの読み込みに失敗したらバックアップからの復元を試みる。
        /// エラー時にはダイアログ表示。選択によってはOperationCancelExceptionを発生させる。
        /// </summary>
        /// <param name="useDefault">データが読み込めなかった場合に初期化されたインスタンスを返す。falseの場合はnullを返す</param>
        private T SafetyLoad<T>(Func<string, T> load, string path, LoadFailedDialog loadFailedDialog, bool useDefault = false, Action loadBackupCallback = null)
            where T : class, new()
        {
            try
            {
                var instance = SafetyLoad(load, path, loadBackupCallback);
                return (instance is null && useDefault) ? new T() : instance;
            }
            catch (Exception ex)
            {
                if (loadFailedDialog != null)
                {
                    var result = loadFailedDialog.ShowDialog(ex);
                    if (result != true)
                    {
                        throw new OperationCanceledException();
                    }
                }

                return useDefault ? new T() : null;
            }
        }


        /// <summary>
        /// 正規ファイルの読み込みに失敗したらバックアップからの復元を試みる
        /// </summary>
        private T SafetyLoad<T>(Func<string, T> load, string path, Action loadBackupCallback)
            where T : class, new()
        {
            var old = path + ".bak";

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
                        loadBackupCallback?.Invoke();
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
                return null;
            }
        }

#endregion

#region Save

        /// <summary>
        /// 設定の保存
        /// </summary>
        public void SaveUserSetting()
        {
            if (!IsEnableSave) return;

            try
            {
                App.Current.SemaphoreWait();
                SafetySave(UserSettingTools.Save, App.Current.Option.SettingFilename, Config.Current.System.IsSettingBackup, _keepBackup);
            }
            catch
            {
            }
            finally
            {
                _keepBackup = true;
                App.Current.SemaphoreRelease();
            }

            RemoveLegacyUserSetting();
        }

        /// <summary>
        /// 必要であるならば、古い設定ファイルを削除
        /// </summary>
        public void RemoveLegacyUserSetting()
        {
            if (_settingFilenameToDelete == null) return;

            RemoveLegacyFile(_settingFilenameToDelete);
            _settingFilenameToDelete = null;
        }

        /// <summary>
        /// 古いファイルを削除
        /// </summary>
        private void RemoveLegacyFile(string filename)
        {
            try
            {
                App.Current.SemaphoreWait();

                Debug.WriteLine($"Remove: {filename}");
                FileIO.RemoveFile(filename);

                // バックアップファイルも削除
                var backup = filename + ".old";
                if (File.Exists(backup))
                {
                    Debug.WriteLine($"Remove: {backup}");
                    FileIO.RemoveFile(backup);
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

        // 履歴をファイルに保存
        public void SaveHistory()
        {
            if (!IsEnableSave) return;

            // 現在の本を履歴に登録
            BookHub.Current.SaveBookMemento(); // TODO: タイミングに問題有り？

            try
            {
                App.Current.SemaphoreWait();
                if (Config.Current.History.IsSaveHistory)
                {
                    var bookHistoryMemento = BookHistoryCollection.Current.CreateMemento();

                    try
                    {
                        var fileInfo = new FileInfo(HistoryFilePath);
                        if (fileInfo.Exists && fileInfo.LastWriteTime > App.Current.StartTime)
                        {
                            var failedDialog = new LoadFailedDialog(Resources.Notice_LoadHistoryFailed, Resources.Notice_LoadHistoryFailedTitle);
                            var margeMemento = SafetyLoad(BookHistoryCollection.Memento.Load, HistoryFilePath, failedDialog);
                            bookHistoryMemento.Merge(margeMemento);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    SafetySave(bookHistoryMemento.Save, HistoryFilePath, false, false);
                }
                else
                {
                    FileIO.RemoveFile(HistoryFilePath);
                }
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }

            RemoveLegacyHistory();
        }

        /// <summary>
        /// 必要であるならば、古い設定ファイルを削除
        /// </summary>
        public void RemoveLegacyHistory()
        {
            if (_historyFilenameToDelete == null) return;

            RemoveLegacyFile(_historyFilenameToDelete);
            _historyFilenameToDelete = null;
        }

        /// <summary>
        /// Bookmarkの保存
        /// </summary>
        public void SaveBookmark()
        {
            if (!IsEnableSave) return;
            if (!Config.Current.Bookmark.IsSaveBookmark) return;

            try
            {
                App.Current.SemaphoreWait();
                var bookmarkMemento = BookmarkCollection.Current.CreateMemento();
                SafetySave(bookmarkMemento.Save, BookmarkFilePath, false, false);
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }

            RemoveLegacyBookmark();
        }

        /// <summary>
        /// 必要であるならば、古い設定ファイルを削除
        /// </summary>
        public void RemoveLegacyBookmark()
        {
            if (_bookmarkFilenameToDelete == null) return;

            RemoveLegacyFile(_bookmarkFilenameToDelete);
            _bookmarkFilenameToDelete = null;
        }

        /// <summary>
        /// 必要であるならば、Bookmarkを削除
        /// </summary>
        public void RemoveBookmarkIfNotSave()
        {
            if (!IsEnableSave) return;
            if (Config.Current.Bookmark.IsSaveBookmark) return;

            try
            {
                App.Current.SemaphoreWait();
                FileIO.RemoveFile(BookmarkFilePath);
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
        /// 必要であるならば、古い設定ファイルを削除
        /// </summary>
        public void RemoveLegacyPagemark()
        {
            if (_pagemarkFilenameToDelete == null) return;

            RemoveLegacyFile(_pagemarkFilenameToDelete);
            _pagemarkFilenameToDelete = null;
        }

        /// <summary>
        /// アプリ強制終了でもファイルがなるべく破壊されないような保存
        /// </summary>
        /// <param name="save">SAVE関数</param>
        /// <param name="path">保存ファイル名</param>
        /// <param name="isBackup">バックアップを作る。falseの場合はバックアップファイル削除</param>
        /// <param name="keepBackup">バックアップファイルは変更しない</param>
        private void SafetySave(Action<string> save, string path, bool isBackup, bool keepBackup)
        {
            try
            {
                var backupPath = path + ".bak";
                var tmpPath = path + ".tmp";

                FileIO.RemoveFile(tmpPath);
                save(tmpPath);

                lock (App.Current.Lock)
                {
                    var newFile = new FileInfo(tmpPath);
                    var oldFile = new FileInfo(path);

                    if (oldFile.Exists)
                    {
                        if (keepBackup)
                        {
                            oldFile.Delete();
                        }
                        else
                        {
                            FileIO.RemoveFile(backupPath);
                            oldFile.MoveTo(backupPath);
                        }
                    }

                    newFile.MoveTo(path);

                    if (!isBackup)
                    {
                        FileIO.RemoveFile(backupPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

#endregion
    }


    /// <summary>
    /// データロードエラーダイアログ
    /// </summary>
    public class LoadFailedDialog
    {
        public LoadFailedDialog(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public string Title { get; set; }
        public string Message { get; set; }
        public UICommand OKCommand { get; set; } = UICommands.OK;
        public UICommand CancelCommand { get; set; }


        public bool ShowDialog(Exception ex)
        {
            var textBox = new System.Windows.Controls.TextBox()
            {
                IsReadOnly = true,
                Text = Message + System.Environment.NewLine + ex.Message,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled,
            };

            var dialog = new MessageDialog(textBox, Title);
            dialog.SizeToContent = System.Windows.SizeToContent.Manual;
            dialog.Height = 320.0;
            dialog.ResizeMode = System.Windows.ResizeMode.CanResize;
            dialog.Commands.Add(OKCommand);
            if (CancelCommand != null)
            {
                dialog.Commands.Add(CancelCommand);
            }

            var result = dialog.ShowDialog();
            return result == OKCommand || CancelCommand == null;
        }
    }
}
