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
    // プログラムの種類
    public enum ExternalProgramType
    {
        Normal,
        Protocol,
    }

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
                            files.Add(page.Content.CreateTempFile(true).Path);
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
        /// <summary>
        /// ProgramType property.
        /// </summary>
        [DataMember]
        public ExternalProgramType ProgramType { get; set; }


        // コマンド
        [DataMember]
        public string Command { get; set; }

        // コマンドパラメータ
        // $FILE = 渡されるファイルパス
        [DataMember]
        public string Parameter { get; set; }

        // プロトコル
        [DataMember]
        public string Protocol { get; set; }


        // 複数ページのときの動作
        [DataMember]
        public MultiPageOptionType MultiPageOption { get; set; }

        // 圧縮ファイルのときの動作
        [DataMember]
        public ArchiveOptionType ArchiveOption { get; set; }

        // 拡張子に関連付けられたアプリを起動するかの判定
        public bool IsDefaultApplication => string.IsNullOrWhiteSpace(Command);

        // コマンドパラメータで使用されるキーワード
        private const string _keyFile = "$File";
        private const string _keyUri = "$Uri";
        private const string _keyUriData = "UriData";

        // 最後に実行したコマンド
        public string LastCall { get; set; }


        // コマンドパラメータ文字列のバリデート
        public static string ValidateApplicationParam(string source)
        {
            if (source == null) source = "";
            source = source.Trim();
            return source.Contains(_keyFile) ? source : (source + $" \"{_keyFile}\"").Trim();
        }

        // コンストラクタ
        private void Constructor()
        {
            Parameter = "\"" + _keyFile + "\"";
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
        }

        // 外部アプリの実行
        public void Call(List<Page> pages)
        {
            this.LastCall = null;

            foreach (var page in pages)
            {
                // file
                if (page.Entry.IsFileSystem)
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
                            CallProcess(page.Content.CreateTempFile(true).Path);
                            break;
                    }
                }
                if (MultiPageOption == MultiPageOptionType.Once || ArchiveOption == ArchiveOptionType.SendArchiveFile) break;
            }
        }

        // 外部アプリの実行(コア)
        private void CallProcess(string fileName)
        {
            switch (this.ProgramType)
            {
                case ExternalProgramType.Normal:
                    if (IsDefaultApplication)
                    {
                        this.LastCall = $"\"{fileName}\"";
                        System.Diagnostics.Process.Start(fileName);
                    }
                    else
                    {
                        string param = ReplaceKeyword(this.Parameter, fileName);
                        this.LastCall= $"\"{Command}\" {param}";
                        System.Diagnostics.Process.Start(Command, param);
                    }
                    return;

                case ExternalProgramType.Protocol:
                    if (!string.IsNullOrWhiteSpace(this.Protocol))
                    {
                        string protocol = ReplaceKeyword(this.Protocol, fileName);
                        this.LastCall = protocol;
                        System.Diagnostics.Process.Start(protocol);
                    }
                    return;
            }
        }

        //
        private string ReplaceKeyword(string s, string filenName )
        {
            var uriData = Uri.EscapeDataString(filenName);
            var uri = uriData.Replace("%5C", "/").Replace("%3A", ":");

            //Debug.WriteLine($"UriData:{uriData}");
            //Debug.WriteLine($"Uri:{uri}");

            s = s.Replace(_keyUriData, uriData);
            s = s.Replace(_keyUri, uri);
            s = s.Replace(_keyFile, filenName);
            return s;
        }


        // インスタンスのクローン
        public ExternalApplication Clone()
        {
            return (ExternalApplication)MemberwiseClone();
        }
    }
}
