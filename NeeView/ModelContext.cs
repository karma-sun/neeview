// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// Model 共通コンテキスト (static)
    /// </summary>
    public static class ModelContext
    {
        public static Preference Preference { get; set; }

        public static JobEngine JobEngine { get; set; }

        public static SusieContext SusieContext { get; set; }
        public static Susie.Susie Susie => SusieContext.Susie;

        public static BookMementoCollection BookMementoCollection { get; set; }
        public static BookHistory BookHistory { get; set; }
        public static BookmarkCollection Bookmarks { get; set; }

        public static ArchiverManager ArchiverManager { get; set; }
        public static BitmapLoaderManager BitmapLoaderManager { get; set; }

        public static CommandTable CommandTable { get; set; }
        public static DragActionTable DragActionTable { get; set; }

        // RoutedCommand辞書
        public static Dictionary<CommandType, RoutedUICommand> BookCommands { get; set; } = new Dictionary<CommandType, RoutedUICommand>();

        public static Recycle Recycle { get; set; }

        //
        public static bool IsAutoGC { get; set; } = true;

        //
        public static void GarbageCollection()
        {
            if (!IsAutoGC) GC.Collect();
        }

        // 初期化
        public static void Initialize()
        {
            Preference = new Preference();

            // 
            JobEngine = new JobEngine();
            //
            BookMementoCollection = new BookMementoCollection();
            BookHistory = new BookHistory();
            Bookmarks = new BookmarkCollection();

            ArchiverManager = new ArchiverManager();
            BitmapLoaderManager = new BitmapLoaderManager();

            CommandTable = new CommandTable();
            DragActionTable = new DragActionTable();

            SusieContext = new SusieContext();

            Recycle = new Recycle();
        }


        // 終了処理
        public static void Terminate()
        {
            JobEngine.Dispose();
        }

        /// <summary>
        /// Preference反映
        /// </summary>
        public static void ApplyPreference()
        {
            // Jobワーカーサイズ
            JobEngine.Start(Preference["loader.thread.size"].Integer);

            // ワイドページ判定用比率
            Page.WideRatio = Preference["view.image.wideratio"].Double;

            // SevenZip対応拡張子設定
            ArchiverManager.UpdateSevenZipSupprtedFileTypes(Preference["loader.archiver.7z.supprtfiletypes"].String);

            // MainWindow Preference適用
            ((MainWindow)App.Current.MainWindow).ApplyPreference(Preference);
        }
    }


}

