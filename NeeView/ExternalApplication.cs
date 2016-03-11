// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // 外部アプリ起動
    [DataContract]
    public class ExternalApplication
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

        // 確証しに関連付けられたアプリを起動するかの判定
        public bool IsDefaultApplication => string.IsNullOrWhiteSpace(Command);

        // コマンドパラメータで使用されるキーワード
        const string _Keyword = "$FILE";

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
                            CallProcess(page.CreateTempFile());
                            break;
                    }
                }
                if (MultiPageOption == MultiPageOptionType.Once) break;
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
