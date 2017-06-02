// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using NeeView.ComponentModel;

namespace NeeView
{

    /// <summary>
    /// ページ
    /// </summary>
    public abstract class Page : BindableBase, IDisposable, IHasPage
    {
        #region 開発用

        [Conditional("DEBUG")]
        protected void RaisePropertyChangedDebug([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            ////PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            RaisePropertyChanged(name);
        }

        // 開発用メッセージ
        #region Property: Message
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                ////if (Index == 9) Debug.WriteLine($">> {value}");
                _message = value;
                RaisePropertyChangedDebug();
            }
        }
        #endregion

        #endregion

        // コンテンツ更新イベント
        public EventHandler Loaded;


        // アーカイブエントリ
        public ArchiveEntry Entry { get; protected set; }

        // ページ番号
        public int Index { get; set; }

        // TODO: 表示番号と内部番号のずれ
        public int IndexPlusOne => Index + 1;

        // 場所
        public string Place { get; protected set; }

        // ページ名 : エントリ名
        public string FileName => Entry?.EntryName;

        // ページ名：ファイル名のみ
        public string LastName => Entry?.EntryLastName;

        // ページ名：フルパス
        public string FullPath => Entry?.EntryFullName;

        // ページ名：プレフィックス
        public string Prefix { get; set; }

        // ページ名：プレフィックスを除いたフルパス
        public string SmartFullPath => (Prefix == null ? FullPath : FullPath.Substring(Prefix.Length)).Replace('\\', '/').Replace("/", " > ");

        // ページ名：プレフィックスを除いたフルパス のディレクトリ名(整形済)
        public string SmartDirectoryName => LoosePath.GetDirectoryName(SmartFullPath).Replace('\\', '/');

        // ファイル情報：最終更新日
        public DateTime? LastWriteTime => Entry.LastWriteTime;

        // ファイル情報：ファイルサイズ
        public long Length => Entry.Length;

        // コンテンツ幅
        public double Width => Content.Size.Width;

        // コンテンツ高
        public double Height => Content.Size.Height;


        /// <summary>
        /// Content有効判定
        /// </summary>
        public bool IsContentAlived => Content.IsLoaded;

        /// <summary>
        /// Content情報有効判定
        /// </summary>
        public bool IsContentInfoAlive => IsContentAlived || Width != 0 || Height != 0;

        /// <summary>
        /// コンテンツ
        /// </summary>
        public PageContent Content { get; protected set; }

        /// <summary>
        /// サムネイル
        /// </summary>
        public Thumbnail Thumbnail => Content?.Thumbnail;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Page()
        {
            InitializeContentJob();
            InitializeThumbnailJob();
        }


        #region コンテンツ

        private PageJob _contentJob;

        /// <summary>
        /// 初期化
        /// </summary>
        private void InitializeContentJob()
        {
            _contentJob = new PageJob(this, new ContentLoadJobCommand(this));
        }

        /// <summary>
        /// 読込
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public async Task LoadAsync(QueueElementPriority priority)
        {
            await LoadAsync(priority, CancellationToken.None);
        }

        /// <summary>
        /// 読込
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LoadAsync(QueueElementPriority priority, CancellationToken token)
        {
            if (Content.IsLoaded) return;

            Message = $"Open... ({priority})";
            await _contentJob.RequestAsync(priority, PageJobOption.None, token);
        }

        /// <summary>
        ///  コンテンツを開く(非同期)
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public JobRequest Load(QueueElementPriority priority, PageJobOption option = PageJobOption.None)
        {
            // 既にロード済の場合は何もしない
            if (Content.IsLoaded) return null;

            Message = $"Open... ({priority})";
            return _contentJob.Request(priority, null, option);
        }

        /// <summary>
        /// JOB: コマンド
        /// </summary>
        private class ContentLoadJobCommand : IJobCommand
        {
            Page _page;

            public ContentLoadJobCommand(Page page)
            {
                _page = page;
            }

            public async Task ExecuteAsync(ManualResetEventSlim completed, CancellationToken token)
            {
                await _page.ExecuteAsync(completed, token);
            }
        }

        /// <summary>
        /// JOB: メイン処理
        /// </summary>
        /// <param name="completed">処理の途中でJOB完了設定されることがある</param>
        /// <param name="cancel"></param>
        private async Task ExecuteAsync(ManualResetEventSlim completed, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            Message = "** Load...";
            await Content.LoadAsync(cancel);
        }


        /// <summary>
        /// コンテンツを閉じる
        /// </summary>
        public void Unload()
        {
            if (!_contentJob.IsActive)
            {
                Message = "Closed.";
            }

            _contentJob.Cancel();

            Content.Unload();
        }

        #endregion


        #region サムネイル

        private PageJob _thumbnailJob;

        /// <summary>
        /// 初期化
        /// </summary>
        private void InitializeThumbnailJob()
        {
            _thumbnailJob = new PageJob(this, new ThumbnailLoadJobCommand(this));
        }


        /// <summary>
        /// JOB: コマンド
        /// </summary>
        private class ThumbnailLoadJobCommand : IJobCommand
        {
            Page _page;

            public ThumbnailLoadJobCommand(Page page)
            {
                _page = page;
            }

            public async Task ExecuteAsync(ManualResetEventSlim completed, CancellationToken token)
            {
                await _page.ExcludeThumbnailAsync(null, token);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="completed"></param>
        /// <param name="token"></param>
        private async Task ExcludeThumbnailAsync(ManualResetEventSlim completed, CancellationToken token)
        {
            // キャッシュチェック
            Content.InitializeThumbnail();

            if (Thumbnail.IsValid) return;

            if (token.IsCancellationRequested) return;

            await Content.LoadThumbnailAsync(token);
        }



        /// <summary>
        /// 読込
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public async Task LoadThumbnailAsync(QueueElementPriority priority)
        {
            await LoadThumbnailAsync(priority, CancellationToken.None);
        }

        /// <summary>
        /// 読込
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LoadThumbnailAsync(QueueElementPriority priority, CancellationToken token)
        {
            if (Thumbnail.IsValid) return;
            await _thumbnailJob.RequestAsync(priority, PageJobOption.None, token);
        }

        /// <summary>
        /// サムネイル要求
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public JobRequest LoadThumbnail(QueueElementPriority priority)
        {
            // 既にサムネイルが存在する場合、何もしない
            if (this.Thumbnail.IsValid) return null;

            return _thumbnailJob.Request(priority, null, PageJobOption.None);
        }


        #endregion



        // ファイルの場所を取得
        public string GetFilePlace()
        {
            Debug.Assert(Entry?.Archiver != null);
            return Entry.GetFileSystemPath() ?? Entry.Archiver.GetPlace();
        }

        // フォルダーの場所を取得
        public string GetFolderPlace()
        {
            Debug.Assert(Entry?.Archiver != null);
            string path = Entry.GetFileSystemPath();
            return path != null ? System.IO.Path.GetDirectoryName(path) : Entry.Archiver.GetPlace();
        }



        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    Unload();

                    Loaded = null;
                    this.Thumbnail.Dispose(); // Changed = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~Page() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }

        #endregion


        /// <summary>
        /// IHasPage interface
        /// </summary>
        /// <returns></returns>
        public Page GetPage()
        {
            return this;
        }
    }

}
