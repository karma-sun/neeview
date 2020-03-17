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


        private bool _isPagelistVisible = false;
        private GridLength _gridLength0 = new GridLength(1, GridUnitType.Star);
        private GridLength _gridLength2 = new GridLength(1, GridUnitType.Star);
        private GridLength _folderListGridLength0 = new GridLength(1, GridUnitType.Star);
        private GridLength _folderListGridLength2 = new GridLength(0);
        private FrameworkElement _visual;


        private FolderPanelModel()
        {
        }


        public bool IsPageListVisible
        {
            get { return _isPagelistVisible && _visual != null; }
            set
            {
                if (_isPagelistVisible != value)
                {
                    _isPagelistVisible = value;
                    if (_isPagelistVisible)
                    {
                        RestoreGridLength();
                    }
                    else
                    {
                        StoreGridLength();
                        FolderListGridLength0 = new GridLength(1, GridUnitType.Star);
                        FolderListGridLength2 = new GridLength(0);
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public GridLength FolderListGridLength0
        {
            get { return _visual != null ? _folderListGridLength0 : new GridLength(1, GridUnitType.Star); }
            set { _folderListGridLength0 = value; RaisePropertyChanged(); }
        }

        public GridLength FolderListGridLength2
        {
            get { return _visual != null ? _folderListGridLength2 : new GridLength(0); }
            set { _folderListGridLength2 = value; RaisePropertyChanged(); }
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

        private void StoreGridLength()
        {
            _gridLength0 = FolderListGridLength0;
            _gridLength2 = FolderListGridLength2;
        }

        private void RestoreGridLength()
        {
            FolderListGridLength0 = _gridLength0;
            FolderListGridLength2 = _gridLength2;
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
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsVisiblePanelList = this.IsPageListVisible;

            if (this.IsPageListVisible) StoreGridLength();
            memento.GridLength0 = _gridLength0.ToString();
            memento.GridLength2 = _gridLength2.ToString();

            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.IsPageListVisible = memento.IsVisiblePanelList;

            var converter = new GridLengthConverter();
            _gridLength0 = (GridLength)converter.ConvertFromString(memento.GridLength0);
            _gridLength2 = (GridLength)converter.ConvertFromString(memento.GridLength2);
            if (this.IsPageListVisible) RestoreGridLength();
        }

        #endregion
    }
}
