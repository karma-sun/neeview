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

namespace NeeView
{
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

        // 所属アーカイバ
        protected Archiver _Archiver;

        // 場所
        public string Place { get; protected set; }

        // ページ名
        public string FileName { get; protected set; }

        // ページ名：フルパス
        public string FullPath { get { return LoosePath.Combine(Place, FileName); } }

        // 更新日時
        public DateTime UpdateTime { get; protected set; }

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
            return (Content as GifResource)?.BitmapSource ?? (Content as BitmapSource);
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

        // ジョブリクエスト
        private JobRequest _JobRequest;

        // Openフラグ
        [Flags]
        public enum OpenOption
        {
            None = 0,
            WeakPriority = (1 << 0), // 高優先度の場合のみ上書き
        };


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
            _JobRequest = ModelContext.JobEngine.Add(OnExecute, OnCancel, priority);
        }


        // JOB: メイン処理
        private void OnExecute(CancellationToken cancel)
        {
            //Debug.WriteLine($"Job.{_JobRequest?.Priority.ToString()}: {FileName}..");

            var content = LoadContent();

            if (!cancel.IsCancellationRequested)
            {
                Message = "Valid.";
                Content = content;
            }
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
            Debug.Assert(_Archiver != null);

            if (_Archiver is FolderFiles)
            {
                return LoosePath.Combine(_Archiver.FileName, FileName);
            }
            else
            {
                return _Archiver.GetPlace();
            }
        }

        // テンポラリファイル名
        private string _TempFile;

        // テンポラリファイルの作成
        public virtual string CreateTempFile()
        {
            if (_TempFile != null) return _TempFile;

            if (_Archiver.IsFileSystem)
            {
                _TempFile = _Archiver.GetFileSystemPath(FileName);
            }
            else
            {
                var tempFile = Temporary.CreateTempFileName(FileName);
                _Archiver.ExtractToFile(FileName, tempFile, false);
                _Archiver.TrashBox.Add(new TrashFile(tempFile)); // ブックの消失とともに消す
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
            return _Archiver != null && _Archiver is FolderFiles;
        }

        // コンテンツローダー名
        protected string LoaderName;

        // 詳細
        public string ContentProperty
        {
            get
            {
                return $"Archiver: {_Archiver?.ToString()}\nLoader: {LoaderName}";
            }
        } 
    }
}
