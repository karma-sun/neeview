using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    public class BookSettingPresenter : BindableBase
    {
        static BookSettingPresenter() => Current = new BookSettingPresenter();
        public static BookSettingPresenter Current { get; }


        private BookSettingPresenter()
        {
            SettingChanged += (s, e) => RaisePropertyChanged(nameof(LatestSetting));
        }


        // 設定の変更通知
        public event EventHandler<BookSettingEventArgs> SettingChanged;


        public BookSetting DefaultSetting { get; private set; } = new BookSetting();

        public BookSetting LatestSetting { get; private set; } = new BookSetting();

        public BookSettingGenerater Generater { get; set; } = new BookSettingGenerater();

        public bool IsLocked { get; set; }

        public void SetDefaultSetting(BookSetting setting)
        {
            if (setting == null) return;

            DefaultSetting = setting.Clone();
            DefaultSetting.Page = null;
        }


        public void SetLatestSetting(BookSetting setting)
        {
            if (setting == null) return;

            LatestSetting = setting.Clone();
            LatestSetting.Page = null;
            SettingChanged?.Invoke(this, null);
        }

        // 新しい本の設定
        public BookSetting GetSetting(BookSetting restore, bool isDefaultRecursive)
        {
            // TODO: isRecursived
            return Generater.Mix(DefaultSetting, LatestSetting, restore, isDefaultRecursive);
        }

        #region BookSetting Operation

        // 単ページ/見開き表示設定の可否
        public bool CanPageModeSubSetting(PageMode mode)
        {
            return !IsLocked && LatestSetting.PageMode == mode;
        }

        // 単ページ/見開き表示設定
        public void SetPageMode(PageMode mode)
        {
            if (IsLocked) return;
            if (LatestSetting.PageMode != mode)
            {
                LatestSetting.PageMode = mode;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.PageMode));
            }
        }

        public void TogglePageMode()
        {
            SetPageMode(LatestSetting.PageMode.GetToggle());
        }

        // 見開き方向設定
        public void SetBookReadOrder(PageReadOrder order)
        {
            if (IsLocked) return;
            if (LatestSetting.BookReadOrder != order)
            {
                LatestSetting.BookReadOrder = order;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.BookReadOrder));
            }
        }

        public void ToggleBookReadOrder()
        {
            SetBookReadOrder(LatestSetting.BookReadOrder.GetToggle());
        }

        // 先頭ページの単ページ表示ON/OFF 
        public void SetIsSupportedSingleFirstPage(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsSupportedSingleFirstPage != value)
            {
                LatestSetting.IsSupportedSingleFirstPage = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedSingleFirstPage));
            }
        }

        public void ToggleIsSupportedSingleFirstPage()
        {
            SetIsSupportedSingleFirstPage(!LatestSetting.IsSupportedSingleFirstPage);
        }

        // 最終ページの単ページ表示ON/OFF 
        public void SetIsSupportedSingleLastPage(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsSupportedSingleLastPage != value)
            {
                LatestSetting.IsSupportedSingleLastPage = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedSingleLastPage));
            }
        }

        public void ToggleIsSupportedSingleLastPage()
        {
            SetIsSupportedSingleLastPage(!LatestSetting.IsSupportedSingleLastPage);
        }

        // 横長ページの分割ON/OFF
        public void SetIsSupportedDividePage(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsSupportedDividePage != value)
            {
                LatestSetting.IsSupportedDividePage = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedDividePage));
            }
        }

        public void ToggleIsSupportedDividePage()
        {
            SetIsSupportedDividePage(!LatestSetting.IsSupportedDividePage);
        }

        // 横長ページの見開き判定ON/OFF
        public void SetIsSupportedWidePage(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsSupportedWidePage != value)
            {
                LatestSetting.IsSupportedWidePage = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedWidePage));
            }
        }

        public void ToggleIsSupportedWidePage()
        {
            SetIsSupportedWidePage(!LatestSetting.IsSupportedWidePage);
        }

        // フォルダー再帰読み込みON/OFF
        public void SetIsRecursiveFolder(bool value)
        {
            if (IsLocked) return;
            if (LatestSetting.IsRecursiveFolder != value)
            {
                LatestSetting.IsRecursiveFolder = value;
                SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsRecursiveFolder));
            }
        }

        public void ToggleIsRecursiveFolder()
        {
            SetIsRecursiveFolder(!LatestSetting.IsRecursiveFolder);
        }

        // ページ並び設定切り替え
        public void ToggleSortMode()
        {
            if (IsLocked) return;
            LatestSetting.SortMode = LatestSetting.SortMode.GetToggle();
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.SortMode));
        }

        // ページ並び設定
        public void SetSortMode(PageSortMode mode)
        {
            if (IsLocked) return;
            LatestSetting.SortMode = mode;
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.SortMode));
        }

        // 既定設定を適用
        public void SetDefaultPageSetting()
        {
            if (IsLocked) return;
            SetLatestSetting(DefaultSetting);
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public BookSetting DefaultSetting { get; set; }

            [DataMember]
            public BookSetting LatestSetting { get; set; }

            [DataMember]
            public BookSettingGenerater Generater { get; set; }


            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void Deserialized(StreamingContext c)
            {
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.DefaultSetting = this.DefaultSetting.Clone();
            memento.LatestSetting = this.LatestSetting.Clone();
            memento.Generater = this.Generater.Clone();
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.DefaultSetting = memento.DefaultSetting?.Clone() ?? new BookSetting();
            this.LatestSetting = memento.LatestSetting?.Clone() ?? new BookSetting();
            this.Generater = memento.Generater?.Clone() ?? new BookSettingGenerater();
        }

        #endregion

    }
}
