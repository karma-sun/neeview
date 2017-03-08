// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
    public abstract class PageContent : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// コンテンツ準備完了イベント
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// アーカイブエントリー
        /// </summary>
        public ArchiveEntry Entry { get; private set; }
        
        /// <summary>
        /// コンテンツサイズ
        /// </summary>
        public Size Size { get; protected set; } = new Size(480, 680);


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
        private bool _IsLoaded;
        public bool IsLoaded
        {
            get { return _IsLoaded; }
            set
            {
                if (_IsLoaded != value)
                {
                    _IsLoaded = value;
                    if (_IsLoaded) Loaded?.Invoke(this, null);
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="entry"></param>
        public PageContent(ArchiveEntry entry)
        {
            this.Entry = entry;
        }


        /// <summary>
        /// コンテンツロード
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task LoadAsync(CancellationToken token)
        {
            await Task.Yield();
        }

        /// <summary>
        /// コンテンツ開放
        /// </summary>
        public virtual void Unload()
        {
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
            await Task.Yield();
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
    }









}
