﻿using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace NeeView
{
    // コピー設定
    [DataContract]
    public static class ClipboardUtility
    {
        // クリップボードにコピー
        public static void Copy(List<Page> pages, CopyFileCommandParameter parameter)
        {
            var data = new System.Windows.DataObject();

            if (SetData(data, pages, parameter, CancellationToken.None))
            {
                System.Windows.Clipboard.SetDataObject(data);
            }
        }


        public static bool SetData(System.Windows.DataObject data, List<Page> pages, CopyFileCommandParameter parameter, CancellationToken token)
        {
            bool result = false;

            if (pages.Count > 0)
            {
                data.SetData(pages.Select(x => new QueryPath(x.EntryFullName)).ToQueryPathCollection());
                result = true;
            }

            var files = PageUtility.CreateFilePathList(pages, parameter.MultiPagePolicy, parameter.ArchivePolicy, token);

            if (files.Count > 0)
            {
                data.SetData(System.Windows.DataFormats.FileDrop, files.ToArray());

                if (parameter.TextCopyPolicy != TextCopyPolicy.None)
                {
                    var paths = (parameter.ArchivePolicy == ArchivePolicy.SendExtractFile && parameter.TextCopyPolicy == TextCopyPolicy.OriginalPath)
                        ? PageUtility.CreateFilePathList(pages, parameter.MultiPagePolicy, ArchivePolicy.SendArchivePath, token)
                        : files;
                    data.SetData(System.Windows.DataFormats.UnicodeText, string.Join(System.Environment.NewLine, paths));
                }

                result = true;
            }

            return result;
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
