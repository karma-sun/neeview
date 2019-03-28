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
using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public enum PageType
    {
        Folder,
        File,
    }

    public interface IHasPage
    {
        Page GetPage();
    }



    /// <summary>
    /// ページ
    /// </summary>
    public abstract class Page : BindableBase, IHasPage, IHasPageContent, IDisposable
    {
        #region 開発用

        protected bool SetPropertyDebug<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChangedDebug(propertyName);
            return true;
        }

        [Conditional("DEBUG")]
        protected void RaisePropertyChangedDebug([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
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
        private ArchiveEntry _entry;
        public ArchiveEntry Entry
        {
            get { return _entry; }
            private set { SetProperty(ref _entry, value); }
        }

        // ページ番号
        public int Index { get; set; }

        // TODO: 表示番号と内部番号のずれ
        public int IndexPlusOne => Index + 1;

        // 場所
        public string Place { get; protected set; }

        // ページ名 : エントリ名
        public string EntryName => Entry?.EntryName;

        // ページ名：ファイル名のみ
        public string EntryLastName => Entry?.EntryLastName;

        // ページ名：フルネーム
        public string EntryFullName => Entry?.SystemPath.Substring(BookPrefix.Length);

        // ページ名：システムパス
        public string SystemPath => Entry?.SystemPath;

        // ページ名：ブックプレフィックス
        public string BookPrefix { get; private set; }

        // ページ名：スマート名用プレフィックス
        public string Prefix { get; set; }

        // ファイル情報：最終更新日
        public DateTime LastWriteTime => Entry != null ? Entry.LastWriteTime : default;

        // ファイル情報：ファイルサイズ
        public long Length => Entry.Length;

        // コンテンツ幅
        public double Width => Size.Width;

        // コンテンツ高
        public double Height => Size.Height;

        // コンテンツサイズ
        public Size Size
        {
            get
            {
                // サイズ指定を反映
                var customSize = PictureProfile.Current.CustomSize;
                if (customSize.IsEnabled && !Content.Size.IsEmptyOrZero())
                {
                    return customSize.IsUniformed ? Content.Size.Uniformed(customSize.Size) : customSize.Size;
                }
                else
                {
                    return Content.Size;
                }
            }
        }


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
        /// ページの種類
        /// </summary>
        public PageType PageType => Content is ArchiveContent ? PageType.Folder : PageType.File;

        /// <summary>
        /// サムネイル
        /// </summary>
        public Thumbnail Thumbnail => Content?.Thumbnail;

        // 表示中?
        private bool _isVisibled;
        public bool IsVisibled
        {
            get { return _isVisibled; }
            set { SetProperty(ref _isVisibled, value); }
        }

        private bool _isPagemark;
        public bool IsPagemark
        {
            get { return _isPagemark; }
            set { SetProperty(ref _isPagemark, value); }
        }


        /// <summary>
        /// 要求状態
        /// </summary>
        public PageContentState State
        {
            get { return Content.State; }
            set { Content.State = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Page(string bookPrefix, ArchiveEntry entry)
        {
            BookPrefix = bookPrefix;
            Entry = entry;
        }


        #region コンテンツ

        /// <summary>
        /// コンテンツ読み込み
        /// </summary>
        public async Task LoadContentAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Message = "Load...";
            await Content.LoadContentAsync(token);
            Message = "Loaded.";

            RaisePropertyChanged(nameof(Content));
        }


        /// <summary>
        /// コンテンツを閉じる
        /// </summary>
        public void UnloadContent()
        {
            Debug.Assert(State == PageContentState.None);
            Content.UnloadContent();
            Message = ".";

            RaisePropertyChanged(nameof(Content));
        }

        #endregion

        #region サムネイル

        /// <summary>
        /// サムネイル読み込み
        /// </summary>
        public async Task LoadThumbnailAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await Content.InitializeEntryAsync(token);
            Content.InitializeThumbnail();
            if (Thumbnail.IsValid) return;
            if (token.IsCancellationRequested) return;
            await Content.LoadThumbnailAsync(token);
        }

        #endregion


        // ToString
        public override string ToString()
        {
            var name = Content?.ToString();
            if (name == null) return base.ToString();
            return $"{name}: State={State}";
        }


        // ページ名：ソート用分割
        public string[] GetEntryFullNameTokens()
        {
            return LoosePath.Split(EntryFullName);
        }

        // ページ名：プレフィックスを除いたフルパス
        public string GetSmartFullName()
        {
            return (Prefix == null ? EntryFullName : EntryFullName.Substring(Prefix.Length)).Replace('\\', '/').Replace("/", " > ");
        }

        // ファイルの場所を取得
        public string GetFilePlace()
        {
            Debug.Assert(Entry?.Archiver != null);
            return Entry.GetFileSystemPath() ?? Entry.Archiver.GetPlace();
        }

        // フォルダーを開く、で取得するパス
        public string GetFolderOpenPlace()
        {
            Debug.Assert(Entry?.Archiver != null);
            if (Entry.Archiver is PagemarkArchiver)
            {
                return Entry.GetFileSystemPath();
            }
            else if (Entry.Archiver is FolderArchive)
            {
                return GetFilePlace();
            }
            else
            {
                return GetFolderPlace();
            }
        }

        // フォルダーの場所を取得
        public string GetFolderPlace()
        {
            Debug.Assert(Entry?.Archiver != null);
            return Entry.Archiver.GetSourceFileSystemPath();
        }

        public Page GetPage()
        {
            return this;
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Loaded = null;
                    Content.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
