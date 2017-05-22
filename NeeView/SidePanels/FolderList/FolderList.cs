// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    //
    public class FolderPlaceChangedEventArgs : EventArgs
    {
        public string Place { get; set; }
        public string Select { get; set; }
        public bool IsFocus { get; set; }
    }


    //
    public class FolderList : BindableBase
    {
        /// <summary>
        /// PanelListItemStyle property.
        /// </summary>
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _panelListItemStyle; }
            set { if (_panelListItemStyle != value) { _panelListItemStyle = value; RaisePropertyChanged(); } }
        }

        //
        private PanelListItemStyle _panelListItemStyle;


        /// <summary>
        /// フォルダーアイコン表示位置
        /// </summary>
        public FolderIconLayout FolderIconLayout
        {
            get { return _folderIconLayout; }
            set { if (_folderIconLayout != value) { _folderIconLayout = value; RaisePropertyChanged(); } }
        }

        private FolderIconLayout _folderIconLayout = FolderIconLayout.Default;


        /// <summary>
        /// IsVisibleHistoryMark property.
        /// </summary>
        public bool IsVisibleHistoryMark
        {
            get { return _isVisibleHistoryMark; }
            set { if (_isVisibleHistoryMark != value) { _isVisibleHistoryMark = value; RaisePropertyChanged(); } }
        }

        private bool _isVisibleHistoryMark = true;


        /// <summary>
        /// IsVisibleBookmarkMark property.
        /// </summary>
        public bool IsVisibleBookmarkMark
        {
            get { return _isVisibleBookmarkMark; }
            set { if (_isVisibleBookmarkMark != value) { _isVisibleBookmarkMark = value; RaisePropertyChanged(); } }
        }

        private bool _isVisibleBookmarkMark = true;


        //
        public BookHub BookHub { get; private set; }

        //
        public FolderPanelModel FolderPanel { get; private set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bookHub"></param>
        /// <param name="folderPanel"></param>
        public FolderList(BookHub bookHub, FolderPanelModel folderPanel)
        {
            this.FolderPanel = folderPanel;
            this.BookHub = bookHub;
        }

        public event EventHandler<FolderPlaceChangedEventArgs> PlaceChanged;

        //
        public void SetPlace(string place, string select, bool isFocus)
        {
            var args = new FolderPlaceChangedEventArgs()
            {
                Place = place,
                Select = select,
                IsFocus = isFocus
            };
            PlaceChanged?.Invoke(this, args);
        }

        /// <summary>
        /// 場所の初期化。
        /// nullを指定した場合、HOMEフォルダに移動。
        /// </summary>
        /// <param name="place"></param>
        public void ResetPlace(string place)
        {
            SetPlace(place ?? this.BookHub.GetFixedHome(), null, false);
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            [DataMember]
            public FolderIconLayout FolderIconLayout { get; set; }

            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            memento.FolderIconLayout = this.FolderIconLayout;
            memento.IsVisibleHistoryMark = this.IsVisibleHistoryMark;
            memento.IsVisibleBookmarkMark = this.IsVisibleBookmarkMark;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.PanelListItemStyle = memento.PanelListItemStyle;
            this.FolderIconLayout = memento.FolderIconLayout;
            this.IsVisibleHistoryMark = memento.IsVisibleHistoryMark;
            this.IsVisibleBookmarkMark = memento.IsVisibleBookmarkMark;

            // Preference反映
            ///RaisePropertyChanged(nameof(FolderIconLayout));
        }

        #endregion
    }




    /// <summary>
    /// 旧フォルダーリスト設定。
    /// 互換性のために残してあります
    /// </summary>
    [DataContract]
    public class FolderListSetting
    {
        [DataMember]
        public bool IsVisibleHistoryMark { get; set; }

        [DataMember]
        public bool IsVisibleBookmarkMark { get; set; }
    }
}
