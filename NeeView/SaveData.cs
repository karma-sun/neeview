// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class SaveData
    {
        public static SaveData Current { get; } = new SaveData();

        public Setting Setting { get; set; }

        public bool IsEnableSave { get; set; } = true;

        private string _historyFileName { get; set; }
        private string _bookmarkFileName { get; set; }
        private string _pagemarkFileName { get; set; }

        private string _oldPagemarkFileName { get; set; }

        public SaveData()
        {
            _historyFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "History.xml");
            _bookmarkFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Bookmark.xml");
            _pagemarkFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Pagemark.xml");

            _oldPagemarkFileName = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Pagekmark.xml");
        }

        // アプリ設定作成
        public Setting CreateSetting()
        {
            var setting = new Setting();

            setting.App = App.Current.CreateMemento();

            setting.SusieMemento = SusieContext.Current.CreateMemento();
            setting.CommandMememto = CommandTable.Current.CreateMemento();
            setting.DragActionMemento = DragActionTable.Current.CreateMemento();

            setting.Memento = Models.Current.CreateMemento();

            return setting;
        }

        // アプリ設定反映
        public void RestoreSetting(Setting setting, bool fromLoad)
        {
            App.Current.Restore(setting.App);
            WindowShape.Current.WindowChromeFrame = App.Current.WindowChromeFrame;

            SusieContext.Current.Restore(setting.SusieMemento);
            CommandTable.Current.Restore(setting.CommandMememto);
            DragActionTable.Current.Restore(setting.DragActionMemento);

            Models.Current.Resore(setting.Memento, fromLoad);
        }

#pragma warning disable CS0612

        //
        public void RestoreSettingCompatible(Setting setting, bool fromLoad)
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
        public void LoadHistory(Setting setting)
        {
            BookHistory.Memento memento;

            if (System.IO.File.Exists(_historyFileName))
            {
                try
                {
                    memento = BookHistory.Memento.Load(_historyFileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    new MessageDialog($"原因: {e.Message}", "履歴の読み込みに失敗しました").ShowDialog();
                    memento = new BookHistory.Memento();
                }
            }
            else
            {
                memento = new BookHistory.Memento();
            }

#pragma warning disable CS0612

            // combatible: 設定ファイルに残っている履歴をマージ
            if (setting.BookHistoryMemento != null)
            {
                memento.Merge(setting.BookHistoryMemento);
            }

#pragma warning restore CS0612

            // 履歴反映
            BookHistory.Current.Restore(memento, true);
            MenuBar.Current.UpdateLastFiles();

            // フォルダーリストの場所に反映
            Models.Current.FolderList.ResetPlace(BookHistory.Current.LastFolder);
        }

        // ブックマーク読み込み
        public void LoadBookmark(Setting setting)
        {
            BookmarkCollection.Memento memento;

            // ブックマーク読み込み
            if (System.IO.File.Exists(_bookmarkFileName))
            {
                try
                {
                    memento = BookmarkCollection.Memento.Load(_bookmarkFileName);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    new MessageDialog($"原因: {e.Message}", "ブックマークの読み込みに失敗しました").ShowDialog();
                    memento = new BookmarkCollection.Memento();
                }
            }
            else
            {
                memento = new BookmarkCollection.Memento();
            }

            // ブックマーク反映
            BookmarkCollection.Current.Restore(memento);
        }

        // ページマーク読み込み
        public void LoadPagemark(Setting setting)
        {
            PagemarkCollection.Memento memento;

            // 読込ファイル名確定
            string filename = null;
            if (System.IO.File.Exists(_pagemarkFileName))
            {
                filename = _pagemarkFileName;
            }
            else if (System.IO.File.Exists(_oldPagemarkFileName))
            {
                filename = _oldPagemarkFileName;
            }

            // ページマーク読み込み
            if (filename != null)
            {
                try
                {
                    memento = PagemarkCollection.Memento.Load(filename);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    new MessageDialog($"原因: {e.Message}", "ページマークの読み込みに失敗しました").ShowDialog();
                    memento = new PagemarkCollection.Memento();
                }

                // 旧ファイル名の変更
                if (filename == _oldPagemarkFileName)
                {
                    System.IO.File.Move(filename, _pagemarkFileName);
                }
            }
            else
            {
                memento = new PagemarkCollection.Memento();
            }

            // ページマーク反映
            PagemarkCollection.Current.Restore(memento);
        }



        // アプリ設定保存
        public void SaveSetting()
        {
            if (!IsEnableSave) return;

            // 現在の本を履歴に登録
            BookHub.Current.SaveBookMemento(); // TODO: タイミングに問題有り？

            // 設定
            var setting = CreateSetting();

            // ウィンドウ座標保存
            setting.WindowShape = WindowShape.Current.SnapMemento;

            try
            {
                // 設定をファイルに保存
                setting.Save(App.Current.Option.SettingFilename);
            }
            catch { }

            // 保存しないフラグ
            bool disableSave = App.Current.IsDisableSave;

            try
            {
                if (disableSave)
                {
                    // 履歴ファイルを削除
                    FileIO.Current.RemoveFile(_historyFileName);
                }
                else
                {
                    // 履歴をファイルに保存
                    var bookHistoryMemento = BookHistory.Current.CreateMemento(true);
                    bookHistoryMemento.Save(_historyFileName);
                }
            }
            catch { }

            try
            {
                if (disableSave)
                {
                    // ブックマークファイルを削除
                    FileIO.Current.RemoveFile(_bookmarkFileName);
                }
                else
                {
                    // ブックマークをファイルに保存
                    var bookmarkMemento = BookmarkCollection.Current.CreateMemento(true);
                    bookmarkMemento.Save(_bookmarkFileName);
                }
            }
            catch { }

            try
            {
                if (disableSave)
                {
                    // ページマークファイルを削除
                    FileIO.Current.RemoveFile(_pagemarkFileName);
                }
                else
                {
                    // ページマークをファイルに保存
                    var pagemarkMemento = PagemarkCollection.Current.CreateMemento(true);
                    pagemarkMemento.Save(_pagemarkFileName);
                }
            }
            catch { }
        }

        // アプリ設定読み込み
        public void LoadSetting(string filename)
        {
            // 設定の読み込み
            if (System.IO.File.Exists(filename))
            {
                try
                {
                    this.Setting = Setting.Load(filename);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    new MessageDialog("設定の読み込みに失敗しました。初期設定で起動します。", "設定の読み込みに失敗しました。").ShowDialog();

                    this.Setting = new Setting();
                }
            }
            else
            {
                this.Setting = new Setting();
            }
        }
    }

}
