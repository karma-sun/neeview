// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // 複数ページのときの動作
    public enum MultiPageOptionType
    {
        Once, // 1ページのみ
        Twice, // 2ページとも
    };

    // 圧縮ファイルの時の動作
    public enum ArchiveOptionType
    {
        None, // 実行しない
        SendArchiveFile, // 圧縮ファイルを渡す
        SendExtractFile, // 出力したファイルを渡す(テンポラリ)
    }

    // コピー設定
    [DataContract]
    public class ClipboardUtility
    {
        // 複数ページのときの動作
        [DataMember]
        public MultiPageOptionType MultiPageOption { get; set; }

        // 圧縮ファイルのときの動作
        [DataMember]
        public ArchiveOptionType ArchiveOption { get; set; }


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
                if (page.IsFile())
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
                            files.Add(page.CreateTempFile(true).Path);
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


    // 外部アプリ起動
    [DataContract]
    public class ExternalApplication
    {
        // コマンド
        [DataMember]
        public string Command { get; set; }

        // コマンドパラメータ
        // $FILE = 渡されるファイルパス
        [DataMember]
        public string Parameter { get; set; }

        // 複数ページのときの動作
        [DataMember]
        public MultiPageOptionType MultiPageOption { get; set; }

        // 圧縮ファイルのときの動作
        [DataMember]
        public ArchiveOptionType ArchiveOption { get; set; }

        // 拡張子に関連付けられたアプリを起動するかの判定
        public bool IsDefaultApplication => string.IsNullOrWhiteSpace(Command);

        // コマンドパラメータで使用されるキーワード
        private const string _Keyword = "$File";

        // コマンドパラメータ文字列のバリデート
        public static string ValidateApplicationParam(string source)
        {
            if (source == null) source = "";
            source = source.Trim();
            return source.Contains(_Keyword) ? source : (source + $" \"{_Keyword}\"").Trim();
        }

        // コンストラクタ
        private void Constructor()
        {
            Parameter = "\"" + _Keyword + "\"";
            MultiPageOption = MultiPageOptionType.Once;
            ArchiveOption = ArchiveOptionType.SendExtractFile;
        }

        // コンストラクタ
        public ExternalApplication()
        {
            Constructor();
        }

        //
        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        [OnDeserialized]
        private void Deserialized(StreamingContext c)
        {
            if (Parameter != null)
            {
                Parameter = Parameter.Replace("$FILE", "$File");
            }
        }

        // 外部アプリの実行
        public void Call(List<Page> pages)
        {
            foreach (var page in pages)
            {
                // file
                if (page.IsFile())
                {
                    CallProcess(page.GetFilePlace());
                }
                // in archive
                else
                {
                    switch (ArchiveOption)
                    {
                        case ArchiveOptionType.None:
                            break;
                        case ArchiveOptionType.SendArchiveFile:
                            CallProcess(page.GetFilePlace());
                            break;
                        case ArchiveOptionType.SendExtractFile:
                            CallProcess(page.CreateTempFile(true).Path);
                            break;
                    }
                }
                if (MultiPageOption == MultiPageOptionType.Once || ArchiveOption == ArchiveOptionType.SendArchiveFile) break;
            }
        }

        // 外部アプリの実行(コア)
        private void CallProcess(string fileName)
        {
            if (IsDefaultApplication)
            {
                System.Diagnostics.Process.Start(fileName);
            }
            else
            {
                string param = Parameter.Replace(_Keyword, fileName);
                System.Diagnostics.Process.Start(Command, param);
            }
        }

        // インスタンスのクローン
        public ExternalApplication Clone()
        {
            return (ExternalApplication)MemberwiseClone();
        }
    }
}
