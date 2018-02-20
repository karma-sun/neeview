// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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

        [PropertyPath("7z.dll(32bit)の場所", Tips = "別の7z.dllを使用したい場合に設定します。反映にはアプリを開き直す必要があります。", Filter = "DLL|*.dll")]
        public string X86DllPath { get; set; } = "";

        [PropertyPath("7z.dll(64bit)の場所", Tips = "別の7z.dllを使用したい場合に設定します。反映にはアプリを開き直す必要があります。", Filter = "DLL|*.dll")]
        public string X64DllPath { get; set; } = "";

        [PropertyMember("7z.dllで展開する圧縮ファイルの拡張子", Tips =".zip を追加すると、zipファイルも7z.dllを使用して展開されるようになります。")]
        public FileTypeCollection SupportFileTypes { get; set; } = new FileTypeCollection(".7z;.rar;.lzh;.cbr;.cbz");

        [PropertyMember("7z.dllがファイルをロックする時間(秒)", Tips = "この時間アクセスがなければロックを解除します。-1でロックを保持したままになります。")]
        public double LockTime { get; set; } = -1.0;

        // 強制アンロックモード
        public bool IsUnlockMode { get; set; }

        // 事前展開
        [PropertyMember("7z.dllでは全て事前展開する", Tips = "ブックを閲覧する時に一時フォルダーに全て事前展開してページ送りを高速化します。OFFにするとソリッド圧縮ファイルの場合のみ事前展開を行います。")]
        public bool IsPreExtract { get; set; }

        // 事前展開サイズ上限
        [PropertyMember("7z.dllで事前展開する最大ファイルサイズ(MB)", Tips = "このサイズを超える圧縮ファイルは事前展開を行いません。全ての事前展開を禁止する場合には0を設定します。")]
        public int PreExtractSolidSize { get; set; } = 1000;

        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember, DefaultValue("")]
            public string X86DllPath { get; set; }

            [DataMember, DefaultValue("")]
            public string X64DllPath { get; set; }

            [DataMember, DefaultValue(".7z;.rar;.lzh;.cbr;.cbz")]
            public string SupportFileTypes { get; set; }

            [DataMember, DefaultValue(-1.0)]
            public double LockTime { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsPreExtract { get; set; }

            [DataMember, DefaultValue(1000)]
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
            memento.SupportFileTypes = this.SupportFileTypes.ToString();
            memento.IsPreExtract = this.IsPreExtract;
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
            this.IsPreExtract = memento.IsPreExtract;
            this.PreExtractSolidSize = memento.PreExtractSolidSize;

            // compatible before ver.25
            if (memento._Version < Config.GenerateProductVersionNumber(1, 25, 0))
            {
                this.SupportFileTypes.Add(".cbr");
                this.SupportFileTypes.Add(".cbz");
            }
        }
        #endregion

    }
}
