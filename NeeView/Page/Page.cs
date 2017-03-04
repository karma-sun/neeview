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

namespace NeeView
{
    /// <summary>
    /// ページ
    /// </summary>
    public abstract class Page : INotifyPropertyChanged, IDisposable
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion


        #region 開発用

        [Conditional("DEBUG")]
        protected void OnPropertyChangedDebug([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        // 開発用メッセージ
        #region Property: Message
        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChangedDebug(); }
        }
        #endregion

        #endregion

        // コンテンツ更新イベント
        public EventHandler Loaded;

        // アーカイブエントリ
        public ArchiveEntry Entry { get; protected set; }

        // ページ番号
        public int Index { get; set; }

        public int IndexPlusOne => Index + 1;

        // 場所
        public string Place { get; protected set; }

        // ページ名 : エントリ名
        public string FileName => Entry?.EntryName;

        // ページ名：ファイル名のみ
        public string LastName => LoosePath.GetFileName(FileName);

        // ページ名：フルパス
        public string FullPath { get { return LoosePath.Combine(Place, FileName); } }

        // ページ名：プレフィックス
        public string Prefix { get; set; }

        // ページ名：プレフィックスを除いたフルパス
        public string SmartFullPath => (Prefix == null ? FullPath : FullPath.Substring(Prefix.Length)).Replace('\\', '/').Replace("/", " > ");

        // ページ名：プレフィックスを除いたフルパス のディレクトリ名(整形済)
        public string SmartDirectoryName => LoosePath.GetDirectoryName(SmartFullPath).Replace('\\', '/');

        // ページ名 : サムネイル識別名
        public virtual string ThumbnailName => FullPath;

        // ファイル情報：最終更新日
        public DateTime? LastWriteTime => Entry.LastWriteTime;

        // ファイル情報：ファイルサイズ
        public long FileSize => Entry.FileSize;


        // コンテンツ幅
        public double Width { get; protected set; }

        // コンテンツ高
        public double Height { get; protected set; }

        // ワイド判定用縦横比
        public static double WideRatio { get; set; }

        // ワイド判定
        public bool IsWide => Width > Height * WideRatio;

        // コンテンツ色
        public Color Color { get; protected set; }

        // コンテンツのBitmapSourceを取得
        public BitmapSource GetBitmapSourceContent()
        {
            return GetBitmapSourceContent(Content);
        }

        // コンテンツのBitmapSourceを取得
        public BitmapSource GetBitmapSourceContent(object content)
        {
            return (content as AnimatedGifContent)?.BitmapContent?.Source ?? (content as BitmapContent)?.Source;
        }

        // コンテンツ
        protected object _content;
        public object Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    if (_content != null)
                    {
                        Loaded?.Invoke(this, null);
                    }
                    OnPropertyChangedDebug(nameof(IsContentAlived));
                }
            }
        }

        /// <summary>
        /// Content有効判定
        /// </summary>
        public bool IsContentAlived => _content != null;

        /// <summary>
        /// Content情報有効判定
        /// </summary>
        public bool IsContentInfoAlive => IsContentAlived || Width != 0 || Height != 0;



        // 最小限のクローン
        public abstract Page TinyClone();




        // アニメーションGIF有効/無効フラグ
        public static bool IsEnableAnimatedGif { get; set; }

        // EXIF情報有効/無効フラグ
        public static bool IsEnableExif { get; set; }


        // コンテンツロード
        protected abstract object LoadContent();

        // ジョブの同時実行を回避
        private object _lock = new object();


        // サムネイル
        public Thumbnail Thumbnail { get; } = new Thumbnail();


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
            if (_content != null) return;
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
            if (_content != null) return null;

            Message = $"Open... ({priority})";
            return _contentJob.Request(priority, option);
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

            public void Cancel()
            {
                _page.OnCancel();
            }

            public void Execute(ManualResetEventSlim completed, CancellationToken token)
            {
                _page.OnExecute(completed, token);
            }
        }

        /// <summary>
        /// JOB: メイン処理
        /// </summary>
        /// <param name="completed">処理の途中でJOB完了設定されることがある</param>
        /// <param name="cancel"></param>
        private void OnExecute(ManualResetEventSlim completed, CancellationToken cancel)
        {
            //Debug.WriteLine($"*OnExecute({LastName})");
            lock (_lock)
            {
                var content = Content ?? LoadContent();

                if (cancel.IsCancellationRequested) return;

                if (Content == null && _contentJob.IsActive)
                {
                    Message = "Valid.";
                    Content = content;
                }

                // ここでJOB完了信号発行。サムネイル化処理と分ける
                completed?.Set();

                // Thumbnail !!
                if (!this.Thumbnail.IsValid)
                {
                    var source = GetBitmapSourceContent(content);
                    if (source != null)
                    {
                        this.Thumbnail.Initialize(source);
                    }
                }
            }
            //Debug.WriteLine($"*OnExecute({LastName}) done.");
        }


        /// <summary>
        /// JOB: キャンセル処理
        /// </summary>
        private void OnCancel()
        {
            Message = $"Canceled.";
        }


        /// <summary>
        /// コンテンツを閉じる
        /// </summary>
        public void Unload()
        {
            Message = "Closing...";

            _contentJob.Cancel();

            if (Content != null)
            {
                Content = null;
                MemoryControl.Current.GarbageCollect();
            }

            Message = "Closed.";
        }

        #endregion


        #region 新サムネイル

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

            public void Cancel()
            {
            }

            public void Execute(ManualResetEventSlim completed, CancellationToken token)
            {
                _page.OnExcludeThumbnail(null, token);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="completed"></param>
        /// <param name="token"></param>
        private void OnExcludeThumbnail(ManualResetEventSlim completed, CancellationToken token)
        {
            // キャッシュチェック
            Thumbnail.Initialize(ThumbnailName, FileSize, LastWriteTime);
            if (Thumbnail.IsValid) return;

            if (token.IsCancellationRequested) return;

            OnExecute(null, token);
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

            return _thumbnailJob.Request(priority, PageJobOption.None);
        }

        // サムネイル要求キャンセル
        //public void UnloadThumbnail()
        //{
        //    _thumbnailJob.Cancel();
        //}

        #endregion


        // ファイルの場所を取得
        public string GetFilePlace()
        {
            Debug.Assert(Entry?.Archiver != null);
            return Entry.GetFileSystemPath() ?? Entry.Archiver.GetPlace();
        }

        // フォルダの場所を取得
        public string GetFolderPlace()
        {
            Debug.Assert(Entry?.Archiver != null);
            string path = Entry.GetFileSystemPath();
            return path != null ? System.IO.Path.GetDirectoryName(path) : Entry.Archiver.GetPlace();
        }

        // テンポラリファイル名
        public FileProxy FileProxy { get; private set; }

        /// <summary>
        /// テンポラリファイルの作成
        /// </summary>
        /// <param name="isKeepFileName">エントリ名準拠のテンポラリファイルを作成</param>
        /// <returns></returns>
        public virtual FileProxy CreateTempFile(bool isKeepFileName)
        {
            FileProxy = FileProxy ?? Entry.ExtractToTemp(isKeepFileName);
            return FileProxy;
        }


        // ファイルを保存する
        public virtual void Export(string path)
        {
            throw new NotImplementedException();
        }

        // ファイルの存在確認
        public bool IsFile()
        {
            return Entry?.Archiver != null && Entry.Archiver is FolderArchive;
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
    }

}
