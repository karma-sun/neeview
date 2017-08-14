// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    //
    public class SevenZipArchiverProfile
    {
        public static SevenZipArchiverProfile Current { get; private set; }

        public SevenZipArchiverProfile()
        {
            Current = this;
        }

        public string X86DllPath { get; set; } = "";
        public string X64DllPath { get; set; } = "";
        public double LockTime { get; set; } = -1.0;
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".7z;.rar;.lzh;.cbr;.cbz");

        // 強制アンロックモード
        public bool IsUnlockMode { get; set; }

        // Solid圧縮ファイル事前展開サイズ上限
        public int PreExtractSolidSize { get; set; } = 1000;

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember, DefaultValue("")]
            [PropertyPath(Name = "7z.dll(32bit)の場所", Tips = "別の7z.dllを使用したい場合に設定します。反映にはアプリを開き直す必要があります")]
            public string X86DllPath { get; set; }

            [DataMember, DefaultValue("")]
            [PropertyPath(Name = "7z.dll(64bit)の場所", Tips = "別の7z.dllを使用したい場合に設定します。反映にはアプリを開き直す必要があります")]
            public string X64DllPath { get; set; }

            [DataMember, DefaultValue(".7z;.rar;.lzh;.cbr;.cbz")]
            [PropertyMember("7z.dllで展開する圧縮ファイルの拡張子", Tips = ";(セミコロン)区切りでサポートする拡張子を羅列します。\n拡張子は .zip のように指定します")]
            public string SupportFileTypes { get; set; }

            [DataMember, DefaultValue(-1.0)]
            [PropertyMember("7z.dllがファイルをロックする時間(秒)", Tips = "この時間アクセスがなければロック解除さます。\n-1でロック保持したままになります")]
            public double LockTime { get; set; }

            [DataMember, DefaultValue(1000)]
            [PropertyMember("7z.dllがソリッド圧縮ファイルを事前展開する最大ファイルサイズ(MB)", Tips = "このサイズ以下のソリッド圧縮ファイルはテンポラリフォルダーに事前展開してページ送り速度を改善します。\n事前展開を禁止する場合には0を設定します。")]
            public int PreExtractSolidSize { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext context)
            {
                this.PreExtractSolidSize = 1000;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.X86DllPath = this.X86DllPath;
            memento.X64DllPath = this.X64DllPath;
            memento.LockTime = this.LockTime;
            memento.SupportFileTypes = this.SupportFileTypes.ToString(); ;
            memento.PreExtractSolidSize = this.PreExtractSolidSize;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.X86DllPath = memento.X86DllPath;
            this.X64DllPath = memento.X64DllPath;
            this.LockTime = memento.LockTime;
            this.SupportFileTypes.FromString(memento.SupportFileTypes);
            this.PreExtractSolidSize = memento.PreExtractSolidSize;

            // compatible before ver.25
            if (memento._Version < Config.GenerateProductVersionNumber(1, 25, 0))
            {
                this.SupportFileTypes.AddString(".cbr");
                this.SupportFileTypes.AddString(".cbz");
            }
        }
        #endregion

    }
}
