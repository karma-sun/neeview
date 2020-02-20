using NeeView.Effects;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public static string DefaultHistoryFilePath => Path.Combine(Config.Current.LocalApplicationDataPath, HistoryFileName);
        public static string DefaultBookmarkFilePath => Path.Combine(Config.Current.LocalApplicationDataPath, BookmarkFileName);
        public static string DefaultPagemarkFilePath => Path.Combine(Config.Current.LocalApplicationDataPath, PagemarkFileName);

        public string UserSettingFilePath => App.Current.Option.SettingFilename;
        public string HistoryFilePath { get; private set; }
        public string BookmarkFilePath { get; private set; }
        public string PagemarkFilePath { get; private set; }

        public UserSetting UserSettingTemp { get; private set; }

        public bool IsEnableSave { get; set; } = true;


        public void UpdateLocation()
        {
            HistoryFilePath = App.Current.HistoryFilePath ?? DefaultHistoryFilePath;
            BookmarkFilePath = App.Current.BookmarkFilePath ?? DefaultBookmarkFilePath;
            PagemarkFilePath = App.Current.PagemarkFilePath ?? DefaultPagemarkFilePath;
        }

        // アプリ設定作成
        public UserSetting CreateSetting()
        {
            var setting = new UserSetting();

            App.Current.WindowChromeFrame = WindowShape.Current.WindowChromeFrame;
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
            App.Current.Restore(setting.App);
            WindowShape.Current.WindowChromeFrame = App.Current.WindowChromeFrame;

            SusiePluginManager.Current.Restore(setting.SusieMemento);
            CommandTable.Current.Restore(setting.CommandMememto, false);
            DragActionTable.Current.Restore(setting.DragActionMemento);

            _models.Resore(setting.Memento);

#pragma warning disable CS0612

            // 互換設定反映

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
                ImageEffect.Current.Restore(setting.ImageEffectMemento);
            }

            // Preference.Compatible
            if (setting.PreferenceMemento != null)
            {
                var preference = new Preference();
                preference.Restore(setting.PreferenceMemento);
                preference.RestoreCompatible();
            }

            // Model.Compatible
            _models.ResoreCompatible(setting.Memento);

#pragma warning restore CS0612
        }

        // アプリ設定のシェイプを反映
        public void RestoreSettingWindowShape(UserSetting setting)
        {
            if (setting == null) return;
            if (setting.WindowShape == null) return;

            // ウィンドウ状態をのぞく設定を反映
            var memento = setting.WindowShape.Clone();
            memento.State = WindowShape.Current.State;
            WindowShape.Current.Restore(memento);
            WindowShape.Current.Refresh();
        }

        #region Load

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
                RestoreSettingWindowShape(setting);
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
                var oldPagemarkFileName = Path.Combine(Config.Current.LocalApplicationDataPath, "Pagekmark.xml");
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
            if (!App.Current.IsSaveBookmark) return;

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
            if (App.Current.IsSaveBookmark) return;

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
            if (!App.Current.IsSavePagemark) return;

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
            if (App.Current.IsSavePagemark) return;

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
