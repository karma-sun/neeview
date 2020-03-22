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

        private Models _models;

        private SaveData()
        {
            _models = new Models();
            UpdateLocation();
        }

        public const string UserSettingFileName = "UserSetting.xml";
        public const string HistoryFileName = "History.xml";
        public const string BookmarkFileName = "Bookmark.xml";
        public const string PagemarkFileName = "Pagemark.xml";

        public static string DefaultHistoryFilePath => Path.Combine(Environment.LocalApplicationDataPath, HistoryFileName);
        public static string DefaultBookmarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, BookmarkFileName);
        public static string DefaultPagemarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, PagemarkFileName);

        public string UserSettingFilePath => App.Current.Option.SettingFilename;
        public string HistoryFilePath { get; private set; }
        public string BookmarkFilePath { get; private set; }
        public string PagemarkFilePath { get; private set; }

        public UserSetting UserSettingTemp { get; private set; }

        public bool IsEnableSave { get; set; } = true;


        public void UpdateLocation()
        {
            HistoryFilePath = Config.Current.History.HistoryFilePath ?? DefaultHistoryFilePath;
            BookmarkFilePath = Config.Current.Bookmark.BookmarkFilePath ?? DefaultBookmarkFilePath;
            PagemarkFilePath = Config.Current.Pagemark.PagemarkFilePath ?? DefaultPagemarkFilePath;
        }

        // アプリ設定作成
        public UserSetting CreateSetting()
        {
            var setting = new UserSetting();

            ////App.Current.WindowChromeFrame = WindowShape.Current.WindowChromeFrame;
            setting.App = App.Current.CreateMemento();

            setting.SusieMemento = SusiePluginManager.Current.CreateMemento();
            setting.CommandMememto = CommandTable.Current.CreateMemento();
            setting.DragActionMemento = DragActionTable.Current.CreateMemento();

            setting.Memento = _models.CreateMemento();

            return setting;
        }

        // アプリ設定反映
        public void RestoreSetting(UserSetting setting)
        {
            ////App.Current.Restore(setting.App);
            ////WindowShape.Current.WindowChromeFrame = App.Current.WindowChromeFrame;

            ////SusiePluginManager.Current.Restore(setting.SusieMemento);
            ////CommandTable.Current.Restore(setting.CommandMememto, false);
            DragActionTable.Current.Restore(setting.DragActionMemento);

            _models.Resore(setting.Memento);
        }

        // アプリ設定のシェイプを反映
        [Obsolete]
        public void RestoreSettingWindowShape(UserSetting setting)
        {
            if (setting == null) return;
            if (setting.WindowShape == null) return;

            // ウィンドウ状態をのぞく設定を反映
            ////var memento = setting.WindowShape.Clone();
            ////memento.State = WindowShape.Current.State;
            ////WindowShape.Current.Restore(memento);
            ////WindowShape.Current.Refresh();
        }

        #region Load

        /// <summary>
        /// 設定の読み込み(仮)
        /// </summary>
        public UserSettingV2 LoadConfig()
        {
            App.Current.SemaphoreWait();

            var v1FileName = Path.ChangeExtension(App.Current.Option.SettingFilename, ".xml");
            var v2FileName = Path.ChangeExtension(App.Current.Option.SettingFilename, ".json");

            try
            {
                if (File.Exists(v2FileName))
                {
                    var settingV2 = SafetyLoad(UserSettingV2Accessor.Load, v2FileName, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                    __TestV1Compatibilty(settingV2);
                    return settingV2;
                }

                if (File.Exists(v1FileName))
                {
                    var settingV1 = SafetyLoad(UserSetting.Load, v1FileName, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                    return settingV1.ConvertToV2();
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
                var settingV1 = SafetyLoad(UserSetting.Load, v1FileName, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                var settingV1Converted = settingV1.ConvertToV2();
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
                        foreach(var key in c1.Keys)
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
                        try
                        {
                            result = CheckValueEquality(property.GetValue(v1), property.GetValue(v2), name + $".{property.Name}") && result;
                        }
                        catch(Exception ex)
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


        /// <summary>
        /// 設定の読み込み
        /// 先行して設定ファイルのみ取得するため
        /// </summary>
        public UserSetting LoasUserSettingTemp()
        {
            if (UserSettingTemp != null)
            {
                return UserSettingTemp;
            }

            try
            {
                App.Current.SemaphoreWait();
                UserSettingTemp = SafetyLoad(UserSetting.Load, App.Current.Option.SettingFilename, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                return UserSettingTemp;
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        /// <summary>
        /// 設定領域の開放
        /// </summary>
        public void ReleaseUserSettingTemp()
        {
            UserSettingTemp = null;
        }

        /// <summary>
        /// 設定読み込みと反映
        /// </summary>
        public void LoadUserSetting()
        {
            Setting.SettingWindow.Current?.Cancel();
            MainWindowModel.Current.CloseCommandParameterDialog();

            try
            {
                App.Current.SemaphoreWait();
                var setting = SafetyLoad(UserSetting.Load, App.Current.Option.SettingFilename, Resources.NotifyLoadSettingFailed, Resources.NotifyLoadSettingFailedTitle);
                RestoreSetting(setting);
                ////RestoreSettingWindowShape(setting);
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        // 履歴読み込み
        public void LoadHistory()
        {
            try
            {
                App.Current.SemaphoreWait();
                BookHistoryCollection.Memento memento = SafetyLoad(BookHistoryCollection.Memento.Load, HistoryFilePath, Resources.NotifyLoadHistoryFailed, Resources.NotifyLoadHistoryFailedTitle);
                BookHistoryCollection.Current.Restore(memento, true);
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
                if (File.Exists(BookmarkFilePath))
                {
                    BookmarkCollection.Memento memento = SafetyLoad(BookmarkCollection.Memento.Load, BookmarkFilePath, Resources.NotifyLoadBookmarkFailed, Resources.NotifyLoadBookmarkFailedTitle);
                    BookmarkCollection.Current.Restore(memento);
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
        /// 設定の保存(仮)
        /// </summary>
        public void SaveConfig()
        {
            if (!IsEnableSave) return;

            try
            {
                App.Current.SemaphoreWait();
                SafetySave(new UserSettingV2Accessor().Save
                    , Path.ChangeExtension(App.Current.Option.SettingFilename, ".json")
                    , Config.Current.System.IsSettingBackup);
            }
            catch
            {
            }
            finally
            {
                App.Current.SemaphoreRelease();
            }
        }

        public void SaveUserSetting()
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
                SafetySave(setting.Save, App.Current.Option.SettingFilename, Config.Current.System.IsSettingBackup);
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
                    var bookHistoryMemento = BookHistoryCollection.Current.CreateMemento(true);

                    try
                    {
                        var fileInfo = new FileInfo(HistoryFilePath);
                        if (fileInfo.Exists && fileInfo.LastWriteTime > App.Current.StartTime)
                        {
                            var margeMemento = SafetyLoad(BookHistoryCollection.Memento.Load, HistoryFilePath, Resources.NotifyLoadHistoryFailed, Resources.NotifyLoadHistoryFailedTitle);
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
