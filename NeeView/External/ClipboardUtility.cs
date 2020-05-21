using NeeLaboratory.ComponentModel;
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
                System.Windows.Clipboard.SetDataObject(data); ;
            }
        }


        public static bool SetData(System.Windows.DataObject data, List<Page> pages, CopyFileCommandParameter parameter, CancellationToken token)
        {
            var files = PageUtility.CreateFilePathList(pages, parameter.MultiPagePolicy, parameter.ArchivePolicy, token);

            if (files.Count > 0)
            {
                data.SetData(System.Windows.DataFormats.FileDrop, files.ToArray());
                data.SetData(System.Windows.DataFormats.UnicodeText, string.Join("\r\n", files));
                return true;
            }
            else
            {
                return false;
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
