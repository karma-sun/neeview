using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class HistoryList : BindableBase
    {
        static HistoryList() => Current = new HistoryList();
        public static HistoryList Current { get; }


        private string _filterPath;
        private List<BookHistory> _items;


        private HistoryList()
        {
            BookOperation.Current.BookChanged += BookOperation_BookChanged;

            Config.Current.History.AddPropertyChanged(nameof(HistoryConfig.IsCurrentFolder), (s, e) => UpdateFilterPath());

            UpdateFilterPath();
        }


        public bool IsThumbnailVisibled
        {
            get
            {
                switch (Config.Current.History.PanelListItemStyle)
                {
                    default:
                        return false;
                    case PanelListItemStyle.Content:
                        return Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0;
                    case PanelListItemStyle.Banner:
                        return Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0;
                }
            }
        }

        public string FilterPath
        {
            get { return _filterPath; }
            set { SetProperty(ref _filterPath, value); }
        }

        public List<BookHistory> Items
        {
            get { return _items; }
            set { _items = value; RaisePropertyChanged(); }
        }


        private void BookOperation_BookChanged(object sender, BookChangedEventArgs e)
        {
            UpdateFilterPath();
        }

        private void UpdateFilterPath()
        {
            FilterPath = Config.Current.History.IsCurrentFolder ? LoosePath.GetDirectoryName(BookOperation.Current.Address) : "";
        }

        public void UpdateItems()
        {
            Items = BookHistoryCollection.Current.Items
                .Where(e => string.IsNullOrEmpty(FilterPath) || FilterPath == LoosePath.GetDirectoryName(e.Path))
                .ToList();
        }

        // 履歴を戻ることができる？
        public bool CanPrevHistory()
        {
            var index = Items.FindIndex(e => e.Path == BookHub.Current.Address);

            if (index < 0)
            {
                return Items.Any();
            }
            else
            {
                return index < Items.Count - 1;
            }
        }

        // 履歴を戻る
        public void PrevHistory()
        {
            if (BookHub.Current.IsLoading || Items.Count <= 0) return;

            var index = Items.FindIndex(e => e.Path == BookHub.Current.Address);

            var prev = index < 0
                ? Items.First()
                : index < Items.Count - 1 ? Items[index + 1] : null;

            if (prev != null)
            {
                BookHub.Current.RequestLoad(prev.Path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyHistoryTerminal);
            }
        }

        // 履歴を進めることができる？
        public bool CanNextHistory()
        {
            var index = Items.FindIndex(e => e.Path == BookHub.Current.Address);
            return index > 0;
        }

        // 履歴を進める
        public void NextHistory()
        {
            if (BookHub.Current.IsLoading) return;

            var index = Items.FindIndex(e => e.Path == BookHub.Current.Address);
            if (index > 0)
            {
                var next = Items[index - 1];
                BookHub.Current.RequestLoad(next.Path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyHistoryLastest);
            }
        }


        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            public void RestoreConfig(Config config)
            {
                config.History.PanelListItemStyle = PanelListItemStyle;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = Config.Current.History.PanelListItemStyle;
            return memento;
        }

        #endregion
    }
}
