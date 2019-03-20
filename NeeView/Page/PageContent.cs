using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 情報コンテンツ表示用
    /// TODO: FilePageContext
    /// </summary>
    public class PageMessage
    {
        /// <summary>
        /// アイコン
        /// </summary>
        public FilePageIcon Icon { get; set; }

        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; }
    }


    /// <summary>
    /// ページコンテンツ基底
    /// </summary>
    public abstract class PageContent : BindableBase
    {
        #region 開発用

        [Conditional("DEBUG")]
        private void InitializeDev()
        {
            Changed += (s, e) => UpdateDebStatus();
            Thumbnail.Changed += (s, e) => UpdateDebStatus();
        }

        private void UpdateDebStatus()
        {
            DevStatus = (Thumbnail.IsValid ? "T" : "") + (IsLoaded ? "C" : "");
        }

        /// <summary>
        /// DevStatus property.
        /// </summary>
        private string _DevStatus;
        public string DevStatus
        {
            get { return _DevStatus; }
            set { if (_DevStatus != value) { _DevStatus = value; RaisePropertyChanged(); } }
        }

        #endregion

        /// <summary>
        /// コンテンツ変更イベント
        /// </summary>
        public event EventHandler Changed;

        protected void RaiseChanged()
        {
            Changed?.Invoke(this, null);
        }

        /// <summary>
        /// コンテンツ準備完了イベント
        /// </summary>
        public event EventHandler Loaded;

        protected void RaiseLoaded()
        {
            Loaded?.Invoke(this, null);
        }

        /// <summary>
        /// アーカイブエントリー
        /// </summary>
        private ArchiveEntry _entry;
        public virtual ArchiveEntry Entry
        {
            get { return _entry; }
            protected set { if (_entry != value) { _entry = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// コンテンツサイズ
        /// </summary>
        public virtual Size Size { get; protected set; } // = new Size(480, 680);


        /// <summary>
        /// 情報表示用
        /// </summary>
        public PageMessage PageMessage { get; protected set; }

        /// <summary>
        /// サムネイル
        /// </summary>
        public Thumbnail Thumbnail { get; protected set; } = new Thumbnail();


        /// <summary>
        /// IsLoaded property.
        /// </summary>
        public virtual bool IsLoaded => true;

        /// <summary>
        /// アニメーション？
        /// </summary>
        public bool IsAnimated { get; protected set; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entry"></param>
        public PageContent(ArchiveEntry entry)
        {
            _entry = entry;

            // 開発用：
            InitializeDev();
        }


        /// <summary>
        /// コンテンツロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task LoadAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// コンテンツ開放
        /// </summary>
        public virtual void Unload()
        {
        }

        /// <summary>
        /// エントリ初期化。
        /// 未定義の場合に生成する
        /// </summary>
        public virtual async Task InitializeEntryAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// サムネイル初期化
        /// </summary>
        public virtual void InitializeThumbnail()
        {
            // 識別名設定
            Thumbnail.Initialize(Entry, null);
        }

        /// <summary>
        /// サムネイルロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task LoadThumbnailAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// テンポラリファイル
        /// </summary>
        public FileProxy FileProxy { get; private set; }

        /// <summary>
        /// テンポラリファイルの作成
        /// </summary>
        /// <param name="isKeepFileName">エントリ名準拠のテンポラリファイルを作成</param>
        /// <returns></returns>
        public FileProxy CreateTempFile(bool isKeepFileName)
        {
            FileProxy = FileProxy ?? Entry.ExtractToTemp(isKeepFileName);
            return FileProxy;
        }

        public override string ToString()
        {
            return _entry.EntryLastName ?? base.ToString();
        }
    }









}
