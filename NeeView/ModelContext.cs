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
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// Model 共通コンテキスト (static)
    /// </summary>
    public static class ModelContext
    {
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
            // Jobワーカーサイズ
            JobEngine = new JobEngine();
            int jobWorkerSize;
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("ThreadSize"), out jobWorkerSize))
            {
                jobWorkerSize = 2; // 標準サイズ
            }
            JobEngine.Start(jobWorkerSize);

            // ワイドページ判定用比率
            double wideRatio;
            if (!double.TryParse(ConfigurationManager.AppSettings.Get("WideRatio"), out wideRatio))
            {
                wideRatio = 1.0;
            }
            Page.WideRatio = wideRatio;

            //
            BookMementoCollection = new BookMementoCollection();
            BookHistory = new BookHistory();
            Bookmarks = new BookmarkCollection();

            ArchiverManager = new ArchiverManager();
            BitmapLoaderManager = new BitmapLoaderManager();

            CommandTable = new CommandTable();
            DragActionTable = new DragActionTable();

            SusieContext = new SusieContext();

            // SevenZip対応拡張子設定
            ArchiverManager.UpdateSevenZipSupprtedFileTypes(ConfigurationManager.AppSettings.Get("SevenZipSupportFileType"));
        }


        // 終了処理
        public static void Terminate()
        {
            JobEngine.Dispose();
        }


        // ファイルを削除する
        public static bool RemoveFile(object sender, string path, System.Windows.FrameworkElement visual)
        {
            if (visual == null)
            {
                var textblock = new System.Windows.Controls.TextBlock();
                textblock.Text = Path.GetFileName(path);

                visual = textblock;
            }

            bool isDirectory = System.IO.Directory.Exists(path);
            string itemType = isDirectory ? "フォルダ" : "ファイル";

            // 削除確認
            var param = new MessageBoxParams()
            {
                Caption = "削除の確認",
                MessageBoxText = "この" + itemType + "をごみ箱に移動しますか？",
                Button = System.Windows.MessageBoxButton.OKCancel,
                Icon = MessageBoxExImage.RecycleBin,
                VisualContent = visual,
            };
            var result = Messenger.Send(sender, new MessageEventArgs("MessageBox") { Parameter = param });

            // 削除する
            if (result == true)
            {
                try
                {
                    // ゴミ箱に捨てる
                    if (isDirectory)
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    }

                }
                catch (Exception e)
                {
                    Messenger.MessageBox(sender, $"{itemType}削除に失敗しました\n\n原因: {e.Message}", "エラー", System.Windows.MessageBoxButton.OK, MessageBoxExImage.Error);
                }
            }

            return result == true;
        }
    }


}

