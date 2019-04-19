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
            LatestSetting.PageMode = mode;
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.PageMode));
        }

        // 単ページ/見開き表示トグル
        public void TogglePageMode()
        {
            if (IsLocked) return;
            LatestSetting.PageMode = LatestSetting.PageMode.GetToggle();
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.PageMode));
        }

        // 見開き方向設定
        public void SetBookReadOrder(PageReadOrder order)
        {
            if (IsLocked) return;
            LatestSetting.BookReadOrder = order;
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.BookReadOrder));
        }

        // 見開き方向変更
        public void ToggleBookReadOrder()
        {
            if (IsLocked) return;
            LatestSetting.BookReadOrder = LatestSetting.BookReadOrder.GetToggle();
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.BookReadOrder));
        }

        // 先頭ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleFirstPage()
        {
            if (IsLocked) return;
            LatestSetting.IsSupportedSingleFirstPage = !LatestSetting.IsSupportedSingleFirstPage;
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedSingleFirstPage));
        }

        // 最終ページの単ページ表示ON/OFF 
        public void ToggleIsSupportedSingleLastPage()
        {
            if (IsLocked) return;
            LatestSetting.IsSupportedSingleLastPage = !LatestSetting.IsSupportedSingleLastPage;
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedSingleLastPage));

        }

        // 横長ページの分割ON/OFF
        public void ToggleIsSupportedDividePage()
        {
            if (IsLocked) return;
            LatestSetting.IsSupportedDividePage = !LatestSetting.IsSupportedDividePage;
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedDividePage));
        }

        // 横長ページの見開き判定ON/OFF
        public void ToggleIsSupportedWidePage()
        {
            if (IsLocked) return;
            LatestSetting.IsSupportedWidePage = !LatestSetting.IsSupportedWidePage;
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsSupportedWidePage));
        }

        // フォルダー再帰読み込みON/OFF
        public void ToggleIsRecursiveFolder()
        {
            if (IsLocked) return;
            LatestSetting.IsRecursiveFolder = !LatestSetting.IsRecursiveFolder;
            SettingChanged?.Invoke(this, new BookSettingEventArgs(BookSettingKey.IsRecursiveFolder));
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
