using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeeView
{
    // コピー設定
    [DataContract]
    public static class ClipboardUtility
    {
        // クリップボードにコピー
        public static void Copy(List<Page> pages)
        {
            var files = new List<string>();

            foreach (var page in pages)
            {
                // file
                if (page.Entry.IsFileSystem)
                {
                    files.Add(page.GetFilePlace());
                }
                // in archive
                else
                {
                    switch (Config.Current.Clipboard.ArchiveOption)
                    {
                        case ArchiveOptionType.None:
                            break;
                        case ArchiveOptionType.SendArchiveFile:
                            files.Add(page.GetFilePlace());
                            break;
                        case ArchiveOptionType.SendExtractFile:
                            files.Add(page.ContentAccessor.CreateTempFile(true).Path);
                            break;
                        case ArchiveOptionType.SendArchivePath:
                            files.Add(page.Entry.CreateArchivePath(Config.Current.Clipboard.ArchiveSeparater));
                            break;
                    }
                }
                if (Config.Current.Clipboard.MultiPageOption == MultiPageOptionType.Once || Config.Current.Clipboard.ArchiveOption == ArchiveOptionType.SendArchiveFile) break;
            }

            if (files.Count > 0)
            {
                var data = new System.Windows.DataObject();
                data.SetData(System.Windows.DataFormats.FileDrop, files.ToArray());
                data.SetData(System.Windows.DataFormats.UnicodeText, string.Join("\r\n", files));
                System.Windows.Clipboard.SetDataObject(data);
            }
        }

        // クリップボードに画像をコピー
        public static void CopyImage(System.Windows.Media.Imaging.BitmapSource image)
        {
            System.Windows.Clipboard.SetImage(image);
        }

        // クリップボードからペースト(テスト)
        public static void Paste()
        {
            var data = System.Windows.Clipboard.GetDataObject(); // クリップボードからオブジェクトを取得する。
            if (data.GetDataPresent(System.Windows.DataFormats.FileDrop)) // テキストデータかどうか確認する。
            {
                var files = (string[])data.GetData(System.Windows.DataFormats.FileDrop); // オブジェクトからテキストを取得する。
                Debug.WriteLine("=> " + files[0]);
            }
        }
    }
}
