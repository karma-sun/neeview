using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookSetting : BindableBase
    {
        public static BookSetting Current { get; private set; }

        public BookSetting()
        {
            Current = this;

            this.SettingChanged +=
                (s, e) => RaisePropertyChanged(nameof(BookMemento));
        }


        // 設定の変更通知
        public event EventHandler SettingChanged;

        // TODO: 応急処置
        public void RaiseSettingChanged()
        {
            SettingChanged?.Invoke(this, null);
        }


        /// <summary>
        /// 本の設定、引き継ぎ用
        /// </summary>
        public Book.Memento BookMemento
        {
            get { return _BookMemento; }
            set
            {
                if (_BookMemento != value)
                {
                    _BookMemento = value.Clone();
                    _BookMemento.ValidateForDefault();
                    RaisePropertyChanged();
                    ////SettingChanged?.Invoke(this, null);
                }
            }
        }

        private Book.Memento _BookMemento = new Book.Memento();



        // 本の設定、標準
        public Book.Memento BookMementoDefault { get; set; } = new Book.Memento();


        // 履歴から復元する設定のフィルタ
        public BookMementoFilter HistoryMementoFilter { get; set; } = new BookMementoFilter(true);

        // 新しい本を開くときに標準設定にする？
        [PropertyMember("ブックを開くときに復元されない項目には既定のページ設定を適用する", Tips ="OFFにすると直前の設定値を引き継ぎます。")]
        public bool IsUseBookMementoDefault { get; set; }


        // 新しい本を開くときの設定取得
        private Book.Memento GetBookMementoDefault()
        {
            if (IsUseBookMementoDefault)
            {
                // 既定の設定
                return BookMementoDefault;
            }
            else
            {
                // 現在の設定の引き継ぎ。フォルダー再帰フラグはクリア
                var memento =  BookMemento.Clone();
                memento.IsRecursiveFolder = false;
                return memento;
            }
        }


        /// <summary>
        /// 最新の設定を取得
        /// </summary>
        /// <param name="place">場所</param>
        /// <param name="lastest">現在の情報</param>
        /// <param name="unit">過去の情報</param>
        /// <returns></returns>
        public Book.Memento CreateLastestBookMemento(string place, Book.Memento lastest, BookMementoUnit unit)
        {
            Book.Memento memento = null;

            // 現在開いているブック
            if (lastest?.Place == place)
            {
                memento = lastest.Clone();
            }
            else
            {
                // 過去の情報
                unit = unit ?? BookMementoCollection.Current.Find(place);
                if (unit != null && unit.Memento.Place == place)
                {
                    // ブックマーク
                    if (unit.BookmarkNode != null)
                    {
                        memento = unit.Memento.Clone();
                    }
                    // 履歴
                    else if (unit.HistoryNode != null)
                    {
                        memento = unit.Memento.Clone();
                    }
                }
            }

            return memento;
        }

        // 設定をブックマーク、履歴から取得する
        public Book.Memento GetSetting(string place, Book.Memento memory, BookLoadOption option)
        {
            // 既定の設定
            var memento = GetBookMementoDefault().Clone();
            if (option.HasFlag(BookLoadOption.DefaultRecursive))
            {
                memento.IsRecursiveFolder = true;
            }
            memento.Page = null;

            // 過去の情報の反映
            if (memory != null && memory.Place == place)
            {
                if ((option & BookLoadOption.Resume) == BookLoadOption.Resume)
                {
                    memento = memory.Clone();
                }
                else
                {
                    memento.Write(HistoryMementoFilter, memory);
                }

                return memento;
            }

            // 履歴なし
            return memento;
        }



        #region BookSetting


        // 本の設定を更新
        // TODO: BookHubアクセスは逆参照になっている。イベントで処理すべき？
        private void RefleshBookSetting()
        {
            BookHub.Current.Book?.Restore(BookMemento);
            SettingChanged?.Invoke(this, null);
        }

        // TODO: この実装どうなのか？
        private bool IsLoading()
        {
            return BookHub.Current.IsLoading;
        }


        // ページモードごとの設定の可否
        public bool CanPageModeSubSetting(PageMode mode)
        {
            return !IsLoading() && BookMemento.PageMode == mode;
        }

        // 先頭ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleFirstPage()
        {
            if (IsLoading()) return;
            BookMemento.IsSupportedSingleFirstPage = !BookMemento.IsSupportedSingleFirstPage;
            RefleshBookSetting();
        }

        // 最終ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleLastPage()
        {
            if (IsLoading()) return;
            BookMemento.IsSupportedSingleLastPage = !BookMemento.IsSupportedSingleLastPage;
            RefleshBookSetting();
        }

        // 横長ページの分割ON/OFF
        public void ToggleIsSupportedDividePage()
        {
            if (IsLoading()) return;
            BookMemento.IsSupportedDividePage = !BookMemento.IsSupportedDividePage;
            RefleshBookSetting();
        }

        // 横長ページの見開き判定ON/OFF
        public void ToggleIsSupportedWidePage()
        {
            if (IsLoading()) return;
            BookMemento.IsSupportedWidePage = !BookMemento.IsSupportedWidePage;
            RefleshBookSetting();
        }

        // フォルダー再帰読み込みON/OFF
        public void ToggleIsRecursiveFolder()
        {
            if (IsLoading()) return;
            BookMemento.IsRecursiveFolder = !BookMemento.IsRecursiveFolder;
            RefleshBookSetting();
        }

        // 見開き方向設定
        public void SetBookReadOrder(PageReadOrder order)
        {
            if (IsLoading()) return;
            BookMemento.BookReadOrder = order;
            RefleshBookSetting();
        }

        // 見開き方向変更
        public void ToggleBookReadOrder()
        {
            if (IsLoading()) return;
            BookMemento.BookReadOrder = BookMemento.BookReadOrder.GetToggle();
            RefleshBookSetting();
        }

        // ページモード設定
        public void SetPageMode(PageMode mode)
        {
            if (IsLoading()) return;
            BookMemento.PageMode = mode;
            RefleshBookSetting();
        }


        // 単ページ/見開き表示トグル
        public void TogglePageMode()
        {
            if (IsLoading()) return;
            BookMemento.PageMode = BookMemento.PageMode.GetToggle();
            RefleshBookSetting();
        }

        // ページ並び変更
        public void ToggleSortMode()
        {
            if (IsLoading()) return;
            var mode = BookMemento.SortMode.GetToggle();
            ////_bookHub.Book?.SetSortMode(mode);
            BookMemento.SortMode = mode;
            RefleshBookSetting();
        }

        // ページ並び設定
        public void SetSortMode(PageSortMode mode)
        {
            if (IsLoading()) return;
            ////_bookHub.Book?.SetSortMode(mode);
            BookMemento.SortMode = mode;
            RefleshBookSetting();
        }

        // 既定設定を適用
        public void SetDefaultPageSetting()
        {
            if (IsLoading()) return;
            BookMemento = BookMementoDefault.Clone();
            RefleshBookSetting();
        }

        #endregion


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public Book.Memento BookMemento { get; set; }
            [DataMember]
            public Book.Memento BookMementoDefault { get; set; }
            [DataMember]
            public bool IsUseBookMementoDefault { get; set; }
            [DataMember]
            public BookMementoFilter HistoryMementoFilter { get; set; }

        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.BookMemento = BookMemento.ValidatedClone();
            memento.BookMementoDefault = BookMementoDefault.ValidatedClone();
            memento.IsUseBookMementoDefault = IsUseBookMementoDefault;
            memento.HistoryMementoFilter = HistoryMementoFilter;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            BookMemento = memento.BookMemento.Clone();
            BookMementoDefault = memento.BookMementoDefault.Clone();
            IsUseBookMementoDefault = memento.IsUseBookMementoDefault;
            HistoryMementoFilter = memento.HistoryMementoFilter;
        }
        #endregion

    }
}
