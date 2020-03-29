using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class FolderPanelModel : BindableBase
    {
        static FolderPanelModel() => Current = new FolderPanelModel();
        public static FolderPanelModel Current { get; }


        private FrameworkElement _visual;


        private FolderPanelModel()
        {
            Config.Current.Bookshelf.AddPropertyChanged(nameof(BookshelfConfig.IsPageListVisible), (s, e) =>
            {
                RaisePropertyChanged(nameof(IsPageListVisible));
                RaisePropertyChanged(nameof(FolderListGridLength0));
                RaisePropertyChanged(nameof(FolderListGridLength2));
            });
        }


        public bool IsPageListVisible
        {
            get { return Config.Current.Bookshelf.IsPageListVisible && _visual != null; }
            set
            {
                if (Config.Current.Bookshelf.IsPageListVisible != value)
                {
                    Config.Current.Bookshelf.IsPageListVisible = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(FolderListGridLength0));
                    RaisePropertyChanged(nameof(FolderListGridLength2));
                }
            }
        }

        public GridLength FolderListGridLength0
        {
            get { return IsPageListVisible ? Config.Current.Bookshelf.GridLength0 : new GridLength(1, GridUnitType.Star); }
            set
            {
                if (IsPageListVisible)
                {
                    Config.Current.Bookshelf.GridLength0 = value;
                }
                RaisePropertyChanged();
            }
        }

        public GridLength FolderListGridLength2
        {
            get { return IsPageListVisible ? Config.Current.Bookshelf.GridLength2 : new GridLength(0); }
            set
            {
                if (IsPageListVisible)
                {
                    Config.Current.Bookshelf.GridLength2 = value;
                }
                RaisePropertyChanged();
            }
        }

        public FrameworkElement Visual
        {
            get { return _visual; }
            set
            {
                if (_visual != value)
                {
                    _visual = value;
                    RaisePropertyChanged(null);
                }
            }
        }


        public void SetVisual(FrameworkElement visual)
        {
            Visual = visual;
        }

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsVisiblePanelList { get; set; }

            [DataMember]
            public string GridLength0 { get; set; }
            [DataMember]
            public string GridLength2 { get; set; }


            public void RestoreConfig(Config config)
            {
                config.Bookshelf.IsPageListVisible = IsVisiblePanelList;

                var converter = new GridLengthConverter();
                config.Bookshelf.GridLength0 = (GridLength)converter.ConvertFromString(GridLength0);
                config.Bookshelf.GridLength2 = (GridLength)converter.ConvertFromString(GridLength2);
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsVisiblePanelList = Config.Current.Bookshelf.IsPageListVisible;

            memento.GridLength0 = Config.Current.Bookshelf.GridLength0.ToString();
            memento.GridLength2 = Config.Current.Bookshelf.GridLength2.ToString();

            return memento;
        }

        #endregion
    }
}
