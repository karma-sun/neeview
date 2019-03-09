using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if DEBUG
namespace NeeView
{
    public static class DebugTest
    {
        public static async Task ExecuteTestAsync()
        {
            Debug.WriteLine("\n[DebugTest]...");

            try
            {
                // ArchiveEntry収集テスト
                await ArchiveEntryCollectionTest.ExecuteAsync(CancellationToken.None);

                // ブックサムネイル作成テスト
                ////await DebugCreateBookThumbnail.TestAsync();
                ////return;

                // 致命的エラーのテスト
                ////InnerExcepionTest.Execute();

                // アーカイブのアンロック
                ////await Task.Run(() => BookOperation.Current.Unlock());

                ////ページマーク多数登録テスト
                ////Models.Current.BookOperation.Test_MakeManyPagemark();

                
                //Config.Current.RemoveApplicationData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debugger.Break();
            }

            // done.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Debug.WriteLine("[DebugTest] done.");
            ////Debugger.Break();
        }


        static class InnerExcepionTest
        {
            public static void Execute()
            {
                throw new ApplicationException("Exception test");
            }
        }

        static class ArchiveEntryCollectionTest
        {
            public static async Task ExecuteAsync(CancellationToken token)
            {
                var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip";
                //var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip\サブフォルダテストX";
                //var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip\圧縮再帰♥.zip\root";
                //var path = @"E:\Work\Labo\サンプル\サブフォルダテストX.zip\圧縮再帰♥.zip\root\dir2?.zip";

                var collection = new ArchiveEntryCollection(path, ArchiveEntryCollectionMode.IncludeSubArchives, ArchiveEntryCollectionMode.IncludeSubArchives, ArchiveEntryCollectionOption.None);

                Debug.WriteLine($"\n> {collection.Path}");

                var entries = await collection.GetEntriesAsync(token);

                var prefix = LoosePath.TrimDirectoryEnd(collection.Path);
                DumpEntries("Raw", entries, prefix);

                // filter: ページとして画像ファイルのみリストアップ
                var p1 = await collection.GetEntriesWhereImageAsync(token);
                DumpEntries("ImageFilter", p1, prefix);

                // filter: ページとしてすべてのファイルをリストアップ。フォルダーはk空フォルダーのみリストアップ
                var p2 = await collection.GetEntriesWherePageAllAsync(token);
                DumpEntries("AllPageFilter", p2, prefix);

                // filter: アーカイブのみリストアップ。以前の動作
                //var archives = entries.Select(e => e.IsArchive())
            }

            private static void DumpEntries(string label, IEnumerable<ArchiveEntry> entries, string prefix)
            {
                Debug.WriteLine($"\n[{label}]");
                foreach (var entry in entries)
                {
                    var attribute = entry.IsDirectory ? "D" : entry.IsArchive() ? "A" : entry.IsImage() ? "I" : "?";
                    var name = entry.SystemPath.Substring(prefix.Length);
                    Debug.WriteLine(attribute + " " + name);
                }
            }
        }
    }

}
#endif


