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
    /// サムネイル有効ページ管理
    /// </summary>
    public class AliveThumbnailList : IDisposable
    {
        private LinkedList<Page> _List = new LinkedList<Page>();

        // サムネイル有効ページを追加
        public void Add(Page page)
        {
            if (page.Thumbnail != null) _List.AddFirst(page);
        }

        // サムネイル全開放
        public void Clear()
        {
            foreach (var page in _List)
            {
                page.CloseThumbnail();
            }
            _List.Clear();
        }

        // 終了処理
        public void Dispose()
        {
            Clear();
        }

        // 有効数を超えるサムネイルは古いものから無効にする
        public void Limited(int limit)
        {
            while (_List.Count > limit)
            {
                var page = _List.Last();
                page.CloseThumbnail();

                _List.RemoveLast();
            }
        }
    }


    /// <summary>
    /// ページ
    /// </summary>
    public abstract class Page : INotifyPropertyChanged
    {
        #region 開発用

        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        [Conditional("DEBUG")]
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        // 開発用メッセージ
        #region Property: Message
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { _Message = value; OnPropertyChanged(); }
        }
        #endregion

        #endregion

        // コンテンツ更新イベント
        public static event EventHandler ContentChanged;

        // コンテンツ更新イベント
        public event EventHandler<bool> Loaded;

        // サムネイル更新イベント
        public event EventHandler<BitmapSource> ThumbnailChanged;

        // アーカイブエントリ
        public ArchiveEntry Entry { get; protected set; }

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


        // サムネイル
        private BitmapSource _Thumbnail;
        public BitmapSource Thumbnail
        {
            get { return _Thumbnail; }

            private set
            {
                if (_Thumbnail != value)
                {
                    _Thumbnail = value;
                    ThumbnailChanged?.Invoke(this, _Thumbnail);
                    OnPropertyChanged();
                }
            }
        }

        // サムネイル更新
        private void UpdateThumbnail(BitmapSource source)
        {
            if (Thumbnail == null)
            {
                Thumbnail = CreateThumbnail(source, new Size(_ThumbnailSize, _ThumbnailSize));
            }
        }

        // サムネイル作成
        private static BitmapSource CreateThumbnail(BitmapSource source, Size maxSize)
        {
            if (source == null) return null;

            double width = source.PixelWidth;
            double height = source.PixelHeight;

            var image = new Image();
            image.Source = source;
            var scaleX = width > maxSize.Width ? maxSize.Width / width : 1.0;
            var scaleY = height > maxSize.Height ? maxSize.Height / height : 1.0;
            var scale = scaleX > scaleY ? scaleY : scaleX;
            if (scale > 1.0) scale = 1.0;
            image.Width = (int)(width * scale + 0.5) / 2 * 2;
            image.Height = (int)(height * scale + 0.5) / 2 * 2;
            if (image.Width < 2.0) image.Width = 2.0;
            if (image.Height < 2.0) image.Height = 2.0;
            image.Stretch = Stretch.Fill;
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            // 拡大はしない
            if (scale > 0.9999) return source;

            // レンダリング
            var grid = new Grid();
            grid.Width = image.Width;
            grid.Height = image.Height;
            grid.Children.Add(image);
            if (scale >= 1.0)
            {
                grid.Width = maxSize.Width;
                grid.Height = maxSize.Height;
                image.HorizontalAlignment = HorizontalAlignment.Center;
                image.VerticalAlignment = VerticalAlignment.Center;
                image.SnapsToDevicePixels = true;
            }

            // ビューツリー外でも正常にレンダリングするようにする処理
            grid.Measure(new Size(grid.Width, grid.Height));
            grid.Arrange(new Rect(new Size(grid.Width, grid.Height)));
            grid.UpdateLayout();

            double dpi = 96.0;
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)grid.Width, (int)grid.Height, dpi, dpi, PixelFormats.Pbgra32);
            bmp.Render(grid);

            grid.Children.Clear();

            return bmp;
        }


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
        protected object _Content;
        public object Content
        {
            get { return _Content; }
            set
            {
                if (_Content != value)
                {
                    _Content = value;
                    ContentChanged?.Invoke(this, null);
                    Loaded?.Invoke(this, _Content != null);
                }
            }
        }

        // 待つ
        public async Task LoadAsync(QueueElementPriority priority)
        {
            try
            {
                if (_Content != null) return;

                var waitEvent = new TaskCompletionSource<bool>();
                EventHandler<bool> a = (s, e) => waitEvent.SetResult(e);

                Loaded += a;

                Open(priority);
                await waitEvent.Task;

                Loaded -= a;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw e;
            }
        }


        // アニメーションGIF有効/無効フラグ
        public static bool IsEnableAnimatedGif { get; set; }

        // EXIF情報有効/無効フラグ
        public static bool IsEnableExif { get; set; }

        // コンテンツロード
        protected abstract object LoadContent();

        // ジョブの同時実行を回避
        private object _Lock = new object();

        // ジョブリクエスト
        private JobRequest _JobRequest;

        // Openフラグ
        [Flags]
        public enum OpenOption
        {
            None = 0,
            WeakPriority = (1 << 0), // 高優先度の場合のみ上書き
        };

        // サムネイル作成ジョブリクエスト
        private JobRequest _ThumbnailJobRequest;

        // サムネイルサイズ
        private double _ThumbnailSize;


        // サムネイルを要求
        public void OpenThumbnail(double size)
        {
            // 既にサムネイルが存在する場合、何もしない
            if (_Thumbnail != null) return;

            // ジョブ登録済の場合も何もしない
            if (_ThumbnailJobRequest != null && !_ThumbnailJobRequest.IsCancellationRequested) return;

            // ジョブ登録
            _ThumbnailSize = size;
            _ThumbnailJobRequest = ModelContext.JobEngine.Add(this, OnExecuteThumbnail, OnCancelThumbnail, QueueElementPriority.Low);
        }

        // サムネイル無効化
        public void CloseThumbnail()
        {
            _Thumbnail = null;
        }

        // JOB: メイン処理
        private void OnExecuteThumbnail(CancellationToken cancel)
        {
            //Debug.WriteLine($"OnExecuteTb({LastName})");
            lock (_Lock)
            {
                if (_Thumbnail == null)
                {
                    BitmapSource source = null;

                    if (Content != null)
                    {
                        source = GetBitmapSourceContent(Content);
                        //Debug.WriteLine("TB: From Content.");
                    }
                    else
                    {
                        var content = LoadContent();

                        if (!cancel.IsCancellationRequested)
                        {
                            source = GetBitmapSourceContent(content);

                            if (_JobRequest != null)
                            {
                                Content = content;
                                //Debug.WriteLine("TB: Keep Content.");
                            }
                        }
                    }

                    if (source != null)
                    {
                        App.Current.Dispatcher.Invoke(() => UpdateThumbnail(source));

                        source = null;
                        GC.Collect();
                    }
                }

                _ThumbnailJobRequest = null;
            }
            // Debug.WriteLine($"OnExecuteTb({LastName}) done.");
        }


        // JOB: キャンセル処理
        private void OnCancelThumbnail()
        {
            Message = $"Canceled.";
            _ThumbnailJobRequest = null;
        }


        // コンテンツを開く(非同期)
        public void Open(QueueElementPriority priority, OpenOption option = OpenOption.None)
        {
            // 既にロード済の場合は何もしない
            if (_Content != null) return;

            // ジョブ登録済の場合、優先度変更
            if (_JobRequest != null && !_JobRequest.IsCancellationRequested)
            {
                if ((option & OpenOption.WeakPriority) != OpenOption.WeakPriority || priority < _JobRequest.Priority)
                {
                    Message = $"ReOpen... ({priority})";
                    _JobRequest.ChangePriority(priority);
                }
                return;
            }

            Message = $"Open... ({priority})";
            _JobRequest = ModelContext.JobEngine.Add(this, OnExecute, OnCancel, priority);
        }


        // JOB: メイン処理
        private void OnExecute(CancellationToken cancel)
        {
            //Debug.WriteLine($"*OnExecute({LastName})");
            lock (_Lock)
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
                }
                else
                {
                    //Debug.WriteLine("CT: AlreadyExist");
                }
            }
            //Debug.WriteLine($"*OnExecute({LastName}) done.");
        }


        // JOB: キャンセル処理
        private void OnCancel()
        {
            Message = $"Canceled.";
        }


        // コンテンツを閉じる
        public void Close()
        {
            Message = "Closing...";

            if (_JobRequest != null)
            {
                _JobRequest.Cancel();
                _JobRequest = null;
            }

            if (_Content != null)
            {
                _Content = null;
                GC.Collect();
            }

            Message = "Closed.";
        }


        // ファイルの場所を取得
        public string GetFilePlace()
        {
            Debug.Assert(Entry?.Archiver != null);
            return Entry.GetFileSystemPath() ?? Entry.Archiver.GetPlace();
        }

        // テンポラリファイル名
        private string _TempFile;

        // テンポラリファイルの作成
        public virtual string CreateTempFile()
        {
            if (_TempFile != null) return _TempFile;

            if (Entry.IsFileSystem)
            {
                _TempFile = Entry.GetFileSystemPath();
            }
            else
            {
                var tempFile = Temporary.CreateTempFileName(FileName);
                Entry.ExtractToFile(tempFile, false);
                Entry.Archiver.TrashBox.Add(new TrashFile(tempFile)); // ブックの消失とともに消す
                _TempFile = tempFile;
            }

            return _TempFile;
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
    }
}
