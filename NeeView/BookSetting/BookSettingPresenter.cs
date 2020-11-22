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


        public BookSettingConfig DefaultSetting => Config.Current.BookSettingDefault;

        public BookSettingConfig LatestSetting => Config.Current.BookSetting;

        public BookSettingPolicyConfig Generater => Config.Current.BookSettingPolicy;

        public bool IsLocked { get; set; }


        public void SetLatestSetting(BookSettingConfig setting)
        {
            if (setting == null) return;
            if (!LatestSetting.Equals(setting))
            {
                setting.CopyTo(LatestSetting);
                LatestSetting.Page = null;
                SettingChanged?.Invoke(this, null);
            }
        }

        // 新しい本の設定
        public BookSettingConfig GetSetting(BookSettingConfig restore, bool isDefaultRecursive)
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
        public class Memento : IMemento
        {
            [DataMember]
            public BookSettingConfig DefaultSetting { get; set; }

            [DataMember]
            public BookSettingConfig LatestSetting { get; set; }

            [DataMember]
            public BookSettingPolicyConfig Generater { get; set; }


            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
            }

            public void RestoreConfig(Config config)
            {
                if (DefaultSetting != null)
                {
                    config.BookSettingDefault.Page = DefaultSetting.Page;
                    config.BookSettingDefault.PageMode = DefaultSetting.PageMode;
                    config.BookSettingDefault.BookReadOrder = DefaultSetting.BookReadOrder;
                    config.BookSettingDefault.IsSupportedDividePage = DefaultSetting.IsSupportedDividePage;
                    config.BookSettingDefault.IsSupportedSingleFirstPage = DefaultSetting.IsSupportedSingleFirstPage;
                    config.BookSettingDefault.IsSupportedSingleLastPage = DefaultSetting.IsSupportedSingleLastPage;
                    config.BookSettingDefault.IsSupportedWidePage = DefaultSetting.IsSupportedWidePage;
                    config.BookSettingDefault.IsRecursiveFolder = DefaultSetting.IsRecursiveFolder;
                    config.BookSettingDefault.SortMode = DefaultSetting.SortMode;
                }
                if (LatestSetting != null)
                {
                    config.BookSetting.Page = LatestSetting.Page;
                    config.BookSetting.PageMode = LatestSetting.PageMode;
                    config.BookSetting.BookReadOrder = LatestSetting.BookReadOrder;
                    config.BookSetting.IsSupportedDividePage = LatestSetting.IsSupportedDividePage;
                    config.BookSetting.IsSupportedSingleFirstPage = LatestSetting.IsSupportedSingleFirstPage;
                    config.BookSetting.IsSupportedSingleLastPage = LatestSetting.IsSupportedSingleLastPage;
                    config.BookSetting.IsSupportedWidePage = LatestSetting.IsSupportedWidePage;
                    config.BookSetting.IsRecursiveFolder = LatestSetting.IsRecursiveFolder;
                    config.BookSetting.SortMode = LatestSetting.SortMode;
                }
                if (Generater != null)
                {
                    config.BookSettingPolicy.Page = Generater.Page;
                    config.BookSettingPolicy.PageMode = Generater.PageMode;
                    config.BookSettingPolicy.BookReadOrder = Generater.BookReadOrder;
                    config.BookSettingPolicy.IsSupportedDividePage = Generater.IsSupportedDividePage;
                    config.BookSettingPolicy.IsSupportedSingleFirstPage = Generater.IsSupportedSingleFirstPage;
                    config.BookSettingPolicy.IsSupportedSingleLastPage = Generater.IsSupportedSingleLastPage;
                    config.BookSettingPolicy.IsSupportedWidePage = Generater.IsSupportedWidePage;
                    config.BookSettingPolicy.IsRecursiveFolder = Generater.IsRecursiveFolder;
                    config.BookSettingPolicy.SortMode = Generater.SortMode;
                }
            }
        }

        #endregion

    }
}
