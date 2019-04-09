using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeeView
{
    // コピー設定
    [DataContract]
    public class ClipboardUtility : BindableBase
    {
        private ArchiveOptionType _archiveOption;
        private string _archiveSeparater;

        // 複数ページのときの動作
        [DataMember]
        [PropertyMember("@ParamClipboardMultiPageOption")]
        public MultiPageOptionType MultiPageOption { get; set; }

        // 圧縮ファイルのときの動作
        [DataMember]
        [PropertyMember("@ParamClipboardArchiveOption")]
        public ArchiveOptionType ArchiveOption
        {
            get { return _archiveOption; }
            set { SetProperty(ref _archiveOption, value); }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember("@ParamClipboardArchiveSeparater", EmptyMessage = "\\")]
        public string ArchiveSeparater
        {
            get => _archiveSeparater;
            set => _archiveSeparater = string.IsNullOrEmpty(value) ? null : value;
        }


        // コンストラクタ
        private void Constructor()
        {
            MultiPageOption = MultiPageOptionType.Once;
            ArchiveOption = ArchiveOptionType.SendExtractFile;
        }

        // コンストラクタ
        public ClipboardUtility()
        {
            Constructor();
        }

        //
        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }


        // クリップボードにコピー
        public void Copy(List<Page> pages)
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
                    switch (ArchiveOption)
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
                            files.Add(page.Entry.CreateArchivePath(_archiveSeparater));
                            break;
                    }
                }
                if (MultiPageOption == MultiPageOptionType.Once || ArchiveOption == ArchiveOptionType.SendArchiveFile) break;
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


        // クリップボードからペースト
        public void Paste()
        {
            var data = System.Windows.Clipboard.GetDataObject(); // クリップボードからオブジェクトを取得する。
            if (data.GetDataPresent(System.Windows.DataFormats.FileDrop)) // テキストデータかどうか確認する。
            {
                var files = (string[])data.GetData(System.Windows.DataFormats.FileDrop); // オブジェクトからテキストを取得する。
                Debug.WriteLine("=> " + files[0]);
            }
        }


        // インスタンスのクローン
        public ClipboardUtility Clone()
        {
            return (ClipboardUtility)MemberwiseClone();
        }
    }
}
