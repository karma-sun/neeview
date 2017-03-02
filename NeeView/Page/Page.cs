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

            var job = Load(priority);

            await job.WaitAsync(token);

            if (token.IsCancellationRequested || job.IsCancellationRequested) throw new OperationCanceledException();
        }

        // アニメーションGIF有効/無効フラグ
        public static bool IsEnableAnimatedGif { get; set; }

        // EXIF情報有効/無効フラグ
        public static bool IsEnableExif { get; set; }

        // サムネイルロード
        protected virtual BitmapContent LoadThumbnail(int size)
        {
            return null;
        }

        // コンテンツロード
        protected abstract object LoadContent();

        // ジョブの同時実行を回避
        private object _lock = new object();

        // ジョブリクエスト
        private JobRequest _jobRequest;

        // Openフラグ
        [Flags]
        public enum OpenOption
        {
            None = 0,
            WeakPriority = (1 << 0), // 高優先度の場合のみ上書き
        };


        #region サムネイル

        // サムネイル更新イベント
        public event EventHandler<BitmapSource> ThumbnailChanged;

        // サムネイルがバナーであるかのフラグ
        private bool _isBanner;

        // サムネイル２
        public Thumbnail Thumbnail2 { get; } = new Thumbnail();

        // サムネイル
        private BitmapSource _thumbnail;
        public BitmapSource Thumbnail
        {
            get { return _thumbnail; }

            private set
            {
                if (_thumbnail != value)
                {
                    _thumbnail = value;
                    ThumbnailChanged?.Invoke(this, _thumbnail);
                    RaisePropertyChanged();
                }
            }
        }

        // サムネイル更新
        private void UpdateThumbnail(BitmapSource source)
        {
            if (Thumbnail == null)
            {
                Thumbnail = Utility.NVGraphics.CreateThumbnail(source, new Size(_thumbnailSize, _isBanner ? double.NaN : _thumbnailSize));
                ////Thumbnail = Utility.NVGraphics.CreateThumbnailByDrawingVisual(source, new Size(_ThumbnailSize, _ThumbnailSize));
                ////Thumbnail = Utility.NVDrawing.CreateThumbnail(source, new Size(_ThumbnailSize, _ThumbnailSize));
            }
        }


        // サムネイル作成ジョブリクエスト
        private JobRequest _thumbnailJobRequest;

        // サムネイルサイズ
        private double _thumbnailSize;


        // サムネイルを要求
        public void OpenThumbnail(QueueElementPriority priority, double size, bool isBanner)
        {
            // 既にサムネイルが存在する場合、何もしない
            if (_thumbnail != null) return;

            // ジョブ登録済の場合も何もしない
            if (_thumbnailJobRequest != null && !_thumbnailJobRequest.IsCancellationRequested) return;

            // ジョブ登録
            _thumbnailSize = size;
            _isBanner = isBanner;
            _thumbnailJobRequest = ModelContext.JobEngine.Add(this, OnExecuteThumbnail, OnCancelThumbnail, priority);
        }

        // サムネイル無効化
        public void CloseThumbnail()
        {
            _thumbnail = null;
        }

        /// <summary>
        /// JOB: メイン処理
        /// </summary>
        /// <param name="completed">JOB完了フラグ。処理の途中でセットされる？</param>
        /// <param name="cancel"></param>
        private void OnExecuteThumbnail(ManualResetEventSlim completed, CancellationToken cancel)
        {
            //Debug.WriteLine($"OnExecuteTb({LastName})");
            if (_thumbnail == null)
            {
                BitmapSource source = null;
                bool isTempSource = true;

                lock (_lock)
                {
                    if (!cancel.IsCancellationRequested)
                    {
                        if (Content != null)
                        {
                            source = GetBitmapSourceContent(Content);
                            //Debug.WriteLine("TB: From Content.");
                        }
                        else
                        {
#if false
                            // サムネイル専用読み込み
                            var thumb = LoadThumbnail((int)_ThumbnailSize);
                            if (thumb != null)
                            {
                                Debug.WriteLine($"{LastName} is TB: {thumb.Source.PixelWidth}x{thumb.Source.PixelHeight}");
                                source = thumb.Source; // GetBitmapSourceContent(thumb);
                            }
                            else
#endif
                            {
                                var content = LoadContent();
                                source = GetBitmapSourceContent(content);
                                if (_jobRequest != null)
                                {
                                    Content = content;
                                    isTempSource = false;
                                    //Debug.WriteLine("TB: Keep Content.");
                                }
                            }
                        }
                    }
                }

                if (source != null)
                {
                    UpdateThumbnail(source);
                    source = null;
                    if (isTempSource) MemoryControl.Current.GarbageCollect();
                }
            }

            _thumbnailJobRequest = null;

            // Debug.WriteLine($"OnExecuteTb({LastName}) done.");
        }


        // JOB: キャンセル処理
        private void OnCancelThumbnail()
        {
            Message = $"Canceled.";
            _thumbnailJobRequest = null;
        }


        #endregion


        /// <summary>
        ///  コンテンツを開く(非同期)
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public JobRequest Load(QueueElementPriority priority, OpenOption option = OpenOption.None)
        {
            // 既にロード済の場合は何もしない
            if (_content != null) return null;

            // ジョブ登録済の場合、優先度変更
            if (_jobRequest != null && !_jobRequest.IsCancellationRequested)
            {
                if ((option & OpenOption.WeakPriority) != OpenOption.WeakPriority || priority < _jobRequest.Priority)
                {
                    Message = $"ReOpen... ({priority})";
                    _jobRequest.ChangePriority(priority);
                }
            }
            else
            {
                Message = $"Open... ({priority})";
                _jobRequest = ModelContext.JobEngine.Add(this, OnExecute, OnCancel, priority);
            }

            return _jobRequest;
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
                if (Content == null)
                {
                    //Debug.WriteLine($"Job.{_JobRequest?.Priority.ToString()}: {FileName}..");
                    var content = LoadContent();

                    if (!cancel.IsCancellationRequested)
                    {
                        Message = "Valid.";
                        Content = content;
                    }

                    // ここでJOB完了信号発行。サムネイル化処理と分ける
                    completed.Set();

                    // Thumbnail !!
                    if (!this.Thumbnail2.IsValid)
                    {
                        var bitmapContent = content as BitmapContent;
                        if (bitmapContent?.Source != null)
                        {
                            this.Thumbnail2.Initialize(bitmapContent?.Source);
                        }
                    }
                }
                else
                {
                    //Debug.WriteLine("CT: AlreadyExist");
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

            if (_jobRequest != null)
            {
                _jobRequest.Cancel();
                _jobRequest = null;
            }

            if (Content != null)
            {
                Content = null;
                MemoryControl.Current.GarbageCollect();
            }

            Message = "Closed.";
        }


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
            return Entry?.Archiver != null && Entry.Archiver is FolderFiles;
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
                    ThumbnailChanged = null;
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
