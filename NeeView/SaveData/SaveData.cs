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

        private SaveData()
        {
        }

        public const string UserSettingFileName = "UserSetting.json";
        public const string HistoryFileName = "History.json";
        public const string BookmarkFileName = "Bookmark.json";
        public const string PagemarkFileName = "Pagemark.xml";

        public static string DefaultHistoryFilePath => Path.Combine(Environment.LocalApplicationDataPath, HistoryFileName);
        public static string DefaultBookmarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, BookmarkFileName);
        public static string DefaultPagemarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, PagemarkFileName);

        public string UserSettingFilePath => App.Current.Option.SettingFilename;
        public string HistoryFilePath => Config.Current.History.HistoryFilePath ?? DefaultHistoryFilePath;
        public string BookmarkFilePath => Config.Current.Bookmark.BookmarkFilePath ?? DefaultBookmarkFilePath;
        public string PagemarkFilePath => Config.Current.Pagemark.PagemarkFilePath ?? DefaultPagemarkFilePath;

        [Obsolete]
        public UserSetting UserSettingTemp { get; private set; }

        public bool IsEnableSave { get; set; } = true;


        // アプリ設定作成
        public UserSetting CreateSetting()
        {
            var setting = new UserSetting();

            setting.App = App.Current.CreateMemento();

            setting.SusieMemento = SusiePluginManager.Current.CreateMemento();
            setting.CommandMememto = CommandTable.Current.CreateMemento();
            setting.DragActionMemento = DragActionTable.Current.CreateMemento();

            setting.Memento = new Models().CreateMemento();

            return setting;
        }

        #region Load


        /// <summary>
        /// 設定の読み込み
        /// </summary>
        public UserSettingV2 LoadUserSetting()
        {
            if (App.Current.IsMainWindowLoaded)
            {
                Setting.SettingWindow.Current?.Cancel();
                MainWindowModel.Current.CloseCommandParameterDialog();
            }

            try
            {
                App.Current.SemaphoreWait();

                var filename = App.Current.Option.SettingFilename;
                var extension = Path.GetExtension(filename).ToLower();
                var filenameV1 = Path.ChangeExtension(filename, ".xml");

                if (extension == ".json" && File.Exists(filename))
                {
                    var setting = SafetyLoad(UserSettingTools.Load, filename, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                    __TestV1Compatibilty(setting);
                    return setting;
                }

                // before v.37
                else if (File.Exists(filenameV1))
                {
                    var settingV1 = SafetyLoad(UserSetting.LoadV1, filenameV1, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                    var settingV1Converted = settingV1.ConvertToV2();

                    var historyV1FilePath = Path.ChangeExtension(settingV1.App.HistoryFilePath ?? DefaultHistoryFilePath, ".xml");
                    var historyV1 = SafetyLoad(BookHistoryCollection.Memento.LoadV1, historyV1FilePath, Resources.NotifyLoadHistoryFailed, Resources.NotifyLoadHistoryFailedTitle); // 一部の履歴設定を反映
                    historyV1.RestoreConfig(settingV1Converted.Config);

                    _settingFilenameToDelete = filenameV1;

                    return settingV1Converted;
                }

                return new UserSettingV2();
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        /// <summary>
        /// 設定V1とのデータ互換性チェック
        /// </summary>
        /// <param name="settingV2"></param>
        [Conditional("DEBUG")]
        private void __TestV1Compatibilty(UserSettingV2 settingV2)
        {
            var v1FileName = Path.ChangeExtension(App.Current.Option.SettingFilename, ".xml");
            if (File.Exists(v1FileName))
            {
                var settingV1 = SafetyLoad(UserSetting.LoadV1, v1FileName, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                var settingV1Converted = settingV1.ConvertToV2();

                var historyV1FilePath = Path.ChangeExtension(settingV1.App.HistoryFilePath ?? DefaultHistoryFilePath, ".xml");
                if (File.Exists(historyV1FilePath))
                {
                    var historyV1 = SafetyLoad(BookHistoryCollection.Memento.LoadV1, historyV1FilePath, Resources.NotifyLoadHistoryFailed, Resources.NotifyLoadHistoryFailedTitle); // 一部の履歴設定を反映
                    historyV1.RestoreConfig(settingV1Converted.Config);
                }
                else
                {
                    settingV1Converted.Config.StartUp.IsOpenLastFolder = settingV2.Config.StartUp.IsOpenLastFolder;
                    settingV1Converted.Config.StartUp.LastFolderPath = settingV2.Config.StartUp.LastFolderPath;
                    settingV1Converted.Config.StartUp.LastBookPath = settingV2.Config.StartUp.LastBookPath;
                    settingV1Converted.Config.History.IsKeepFolderStatus = settingV2.Config.History.IsKeepFolderStatus;
                    settingV1Converted.Config.History.IsKeepSearchHistory = settingV2.Config.History.IsKeepSearchHistory;
                    settingV1Converted.Config.History.LimitSize = settingV2.Config.History.LimitSize;
                    settingV1Converted.Config.History.LimitSpan = settingV2.Config.History.LimitSpan;
                }

                Debug.Assert(CheckValueEquality(settingV1Converted, settingV2, nameof(UserSettingV2)));
            }

            bool CheckValueEquality(object v1, object v2, string name)
            {
                if (v1 == null && v2 == null) return true;
                if (v1 == null || v2 == null)
                {
                    Debug.WriteLine($"!!!! {name}: {v1} != {v2}");
                    return false;
                }

                var type = v1.GetType();
                if (type != v2.GetType()) throw new InvalidOperationException();

                if (type.IsValueType || type == typeof(string))
                {
                    if (!Equals(v1, v2))
                    {
                        Debug.WriteLine($"!!!! {name}: {v1} != {v2}");
                        return false;
                    }
                }
                else if (type.GetInterfaces().Contains(typeof(System.Collections.IDictionary)))
                {
                    var c1 = (System.Collections.IDictionary)v1;
                    var c2 = (System.Collections.IDictionary)v2;
                    if (c1.Count != c2.Count)
                    {
                        Debug.WriteLine($"!!!! {v1}.Count != {v2}.Count");
                        return false;
                    }
                    else
                    {
                        bool result = true;
                        foreach (var key in c1.Keys)
                        {
                            var a1 = c1[key];
                            var a2 = c2[key];
                            result = CheckValueEquality(a1, a2, name + $"[{key}]") && result;
                        }
                        return result;
                    }
                }
                else if (type.GetInterfaces().Contains(typeof(System.Collections.ICollection)))
                {
                    var c1 = (System.Collections.ICollection)v1;
                    var c2 = (System.Collections.ICollection)v2;
                    if (c1.Count != c2.Count)
                    {
                        Debug.WriteLine($"!!!! {v1}.Count != {v2}.Count");
                        return false;
                    }
                    else
                    {
                        bool result = true;
                        var e1 = c1.GetEnumerator();
                        var e2 = c2.GetEnumerator();

                        for (int i = 0; i < c1.Count; ++i)
                        {
                            e1.MoveNext();
                            e2.MoveNext();
                            var a1 = e1.Current;
                            var a2 = e2.Current;
                            result = CheckValueEquality(a1, a2, name + $"[{i}]") && result;
                        }
                        return result;
                    }
                }
                else if (type.IsClass)
                {
                    bool result = true;
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var property in properties)
                    {
                        if (property.GetSetMethod(false) == null)
                        {
                            continue;
                        }

                        try
                        {
                            result = CheckValueEquality(property.GetValue(v1), property.GetValue(v2), name + $".{property.Name}") && result;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    return result;
                }
                else
                {
                    throw new InvalidOperationException();
                }

                return true;
            }

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

                if (extension == ".json" && File.Exists(filename))
                {
                    BookHistoryCollection.Memento memento = SafetyLoad(BookHistoryCollection.Memento.Load, HistoryFilePath, Resources.NotifyLoadHistoryFailed, Resources.NotifyLoadHistoryFailedTitle);
                    BookHistoryCollection.Current.Restore(memento, true);
                }
                // before v.37
                else if (File.Exists(filenameV1))
                {
                    BookHistoryCollection.Memento memento = SafetyLoad(BookHistoryCollection.Memento.LoadV1, filenameV1, Resources.NotifyLoadHistoryFailed, Resources.NotifyLoadHistoryFailedTitle);
                    BookHistoryCollection.Current.Restore(memento, true);

                    _historyFilenameToDelete = filenameV1;
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

                if (extension == ".json" && File.Exists(filename))
                {
                    BookmarkCollection.Memento memento = SafetyLoad(BookmarkCollection.Memento.Load, filename, Resources.NotifyLoadBookmarkFailed, Resources.NotifyLoadBookmarkFailedTitle);
                    BookmarkCollection.Current.Restore(memento);
                }
                // before v.37
                else if (File.Exists(filenameV1))
                {
                    BookmarkCollection.Memento memento = SafetyLoad(BookmarkCollection.Memento.LoadV1, filenameV1, Resources.NotifyLoadBookmarkFailed, Resources.NotifyLoadBookmarkFailedTitle);
                    BookmarkCollection.Current.Restore(memento);

                    _bookmarkFilenameToDelete = filenameV1;
                }
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
                var oldPagemarkFileName = Path.Combine(Environment.LocalApplicationDataPath, "Pagekmark.xml");
                if (!File.Exists(PagemarkFilePath) && File.Exists(oldPagemarkFileName))
                {
                    File.Move(oldPagemarkFileName, PagemarkFilePath);
                }
            }
            catch { }

            try
            {
                App.Current.SemaphoreWait();
                if (File.Exists(PagemarkFilePath))
                {
                    PagemarkCollection.Memento memento = SafetyLoad(PagemarkCollection.Memento.Load, PagemarkFilePath, Resources.NotifyLoadPagemarkFailed, Resources.NotifyLoadPagemarkFailedTitle);
                    PagemarkCollection.Current.Restore(memento);
                }
            }
            finally
            {
                App.Current.SemaphoreRelease();
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
                SafetySave(UserSettingTools.Save, Path.ChangeExtension(App.Current.Option.SettingFilename, ".json"), Config.Current.System.IsSettingBackup);
            }
            catch
            {
            }
            finally
            {
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

            try
            {
                App.Current.SemaphoreWait();
                Debug.WriteLine($"RemoveLegacyUserSetting: {_settingFilenameToDelete}");
                FileIO.RemoveFile(_settingFilenameToDelete);
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }

            _settingFilenameToDelete = null;
        }

        [Obsolete]
        [Conditional("DEBUG")]
        public void SaveUserSettingV1()
        {
            if (!IsEnableSave) return;

            // 設定
            var setting = CreateSetting();

            // ウィンドウ状態保存
            setting.WindowShape = WindowShape.Current.CreateMemento();

            // ウィンドウ座標保存
            setting.WindowPlacement = WindowPlacement.Current.CreateMemento();

            // 設定をファイルに保存
            try
            {
                App.Current.SemaphoreWait();
                SafetySave(setting.SaveV1, Path.ChangeExtension(App.Current.Option.SettingFilename, ".xml"), Config.Current.System.IsSettingBackup);
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
                            var margeMemento = SafetyLoad(BookHistoryCollection.Memento.LoadV1, HistoryFilePath, Resources.NotifyLoadHistoryFailed, Resources.NotifyLoadHistoryFailedTitle);
                            bookHistoryMemento.Merge(margeMemento);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    SafetySave(bookHistoryMemento.Save, HistoryFilePath, false);
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

            try
            {
                App.Current.SemaphoreWait();
                Debug.WriteLine($"RemoveLegacyHistory: {_historyFilenameToDelete}");
                FileIO.RemoveFile(_historyFilenameToDelete);
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }

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
                SafetySave(bookmarkMemento.Save, BookmarkFilePath, false);
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

            try
            {
                App.Current.SemaphoreWait();
                Debug.WriteLine($"RemoveLegacyBookmark: {_bookmarkFilenameToDelete}");
                FileIO.RemoveFile(_bookmarkFilenameToDelete);
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }

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
        /// Pagemarkの保存
        /// </summary>
        public void SavePagemark()
        {
            if (!IsEnableSave) return;
            if (!Config.Current.Pagemark.IsSavePagemark) return;

            try
            {
                App.Current.SemaphoreWait();
                var pagemarkMemento = PagemarkCollection.Current.CreateMemento();
                SafetySave(pagemarkMemento.Save, PagemarkFilePath, false);
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
        /// 必要であるならば、Pagemarkを削除
        /// </summary>
        public void RemovePagemarkIfNotSave()
        {
            if (!IsEnableSave) return;
            if (Config.Current.Pagemark.IsSavePagemark) return;

            try
            {
                App.Current.SemaphoreWait();
                FileIO.RemoveFile(PagemarkFilePath);
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

        #endregion
    }
}
