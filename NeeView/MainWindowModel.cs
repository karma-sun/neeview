// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // パネルカラー
    public enum PanelColor
    {
        Dark,
        Light,
    }


    public class MainWindowModel : BindableBase
    {
        public static MainWindowModel Current { get; private set; }

        public Models Models { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        public MainWindowModel()
        {
            Current = this;
        }

        //
        private PanelColor _panelColor = PanelColor.Dark;
        public PanelColor PanelColor
        {
            get { return _panelColor; }
            set { if (_panelColor != value) { _panelColor = value; RaisePropertyChanged(); } }
        }


        //
        private ContextMenuSetting _contextMenuSetting = new ContextMenuSetting();
        public ContextMenuSetting ContextMenuSetting
        {
            get { return _contextMenuSetting; }
            set
            {
                _contextMenuSetting = value;
                _contextMenuSetting.Validate();
                RaisePropertyChanged();
            }
        }

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelColor PanelColor { get; set; }

            [DataMember]
            public ContextMenuSetting ContextMenuSetting { get; set; }


            //
            [DataMember]
            public bool IsSaveWindowPlacement { get; set; }

            [DataMember]
            public bool IsHideMenu { get; set; }

            [DataMember]
            public bool IsSaveFullScreen { get; set; }

            [DataMember]
            public string UserDownloadPath { get; set; }


            [DataMember]
            public bool IsVisibleAddressBar { get; set; }

            [DataMember]
            public bool IsHidePanel { get; set; }

            [DataMember]
            public bool IsHidePanelInFullscreen { get; set; }


            [DataMember]
            public bool IsHidePageSlider { get; set; }

            [DataMember]
            public bool IsVisibleWindowTitle { get; set; }


            //
            private void Constructor()
            {
                IsSaveWindowPlacement = true;
                IsHidePanelInFullscreen = true;
                IsVisibleWindowTitle = true;
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.PanelColor = this.PanelColor;
            memento.ContextMenuSetting = this.ContextMenuSetting.Clone();

            /*
            memento.IsSaveWindowPlacement = this.IsSaveWindowPlacement;
            memento.IsHideMenu = this.IsHideMenu;
            memento.IsHidePageSlider = this.IsHidePageSlider;
            memento.IsSaveFullScreen = this.IsSaveFullScreen;
            memento.UserDownloadPath = this.UserDownloadPath;
            memento.IsVisibleAddressBar = this.IsVisibleAddressBar;
            memento.IsHidePanel = this.IsHidePanel;
            memento.IsHidePanelInFullscreen = this.IsHidePanelInFullscreen;
            memento.IsVisibleWindowTitle = this.IsVisibleWindowTitle;
            */

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.PanelColor = memento.PanelColor;
            this.ContextMenuSetting = memento.ContextMenuSetting.Clone();

            /*
            this.IsSaveWindowPlacement = memento.IsSaveWindowPlacement;
            this.IsHideMenu = memento.IsHideMenu;
            this.IsHidePageSlider = memento.IsHidePageSlider;
            this.IsSaveFullScreen = memento.IsSaveFullScreen;
            this.UserDownloadPath = memento.UserDownloadPath;
            this.IsHidePanel = memento.IsHidePanel;
            this.IsVisibleAddressBar = memento.IsVisibleAddressBar;
            this.IsHidePanelInFullscreen = memento.IsHidePanelInFullscreen;
            this.IsVisibleWindowTitle = memento.IsVisibleWindowTitle;
            */
        }

        #endregion

    }
}
