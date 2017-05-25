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

            // Window Shape
            WindowShape.Current.AddPropertyChanged(nameof(WindowShape.Current.IsFullScreen),
                (s, e) => RaisePropertyChanged(nameof(CanHidePanel)));
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


        // メニューを自動的に隠す
        private bool _isHideMenu;
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set
            {
                _isHideMenu = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHideMenu));
                ////UpdateSidePanelMargin();
            }
        }

        //
        public bool ToggleHideMenu()
        {
            IsHideMenu = !IsHideMenu;
            return IsHideMenu;
        }

        //
        public bool CanHideMenu => IsHideMenu || WindowShape.Current.IsFullScreen;



        // スライダーを自動的に隠す
        private bool _isIsHidePageSlider;
        public bool IsHidePageSlider
        {
            get { return _isIsHidePageSlider; }
            set
            {
                _isIsHidePageSlider = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHidePageSlider));
                ////UpdateSidePanelMargin();
            }
        }

        //
        public bool ToggleHidePageSlider()
        {
            IsHidePageSlider = !IsHidePageSlider;
            return IsHidePageSlider;
        }

        //
        public bool CanHidePageSlider => IsHidePageSlider || WindowShape.Current.IsFullScreen;



        // パネルを自動的に隠す
        private bool _isHidePanel; // = true;
        public bool IsHidePanel
        {
            get { return _isHidePanel; }
            set
            {
                _isHidePanel = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHidePanel));
            }
        }

        public bool ToggleHidePanel()
        {
            IsHidePanel = !IsHidePanel;
            return IsHidePanel;
        }

        /// <summary>
        /// フルスクリーン時にパネルを隠す
        /// </summary>
        public bool IsHidePanelInFullscreen
        {
            get { return _IsHidePanelInFullscreen; }
            set { if (_IsHidePanelInFullscreen != value) { _IsHidePanelInFullscreen = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(CanHidePanel)); } }
        }

        //
        private bool _IsHidePanelInFullscreen = true;



        // パネルを自動的に隠せるか
        public bool CanHidePanel => IsHidePanel || (IsHidePanelInFullscreen && WindowShape.Current.IsFullScreen);


        /// <summary>
        /// IsVisibleWindowTitle property.
        /// タイトルバーが表示されておらず、スライダーにフォーカスがある場合等にキャンバスにタイトルを表示する
        /// </summary>
        private bool _IsVisibleWindowTitle = true;
        public bool IsVisibleWindowTitle
        {
            get { return _IsVisibleWindowTitle; }
            set { if (_IsVisibleWindowTitle != value) { _IsVisibleWindowTitle = value; RaisePropertyChanged(); } }
        }


        // アドレスバーON/OFF
        private bool _isVisibleAddressBar;
        public bool IsVisibleAddressBar
        {
            get { return _isVisibleAddressBar; }
            set { _isVisibleAddressBar = value; RaisePropertyChanged(); }
        }

        public bool ToggleVisibleAddressBar()
        {
            IsVisibleAddressBar = !IsVisibleAddressBar;
            return IsVisibleAddressBar;
        }




        // 履歴削除
        // TODO: 直接変更し、最近使ったファイルはイベントで更新すべき
        public void ClearHistory()
        {
            BookHistory.Current.Clear();
            MenuBar.Current.UpdateLastFiles();
        }

        // オンラインヘルプ
        // TODO: どこで定義すべき？
        public void OpenOnlineHelp()
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
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
            public bool IsHideMenu { get; set; }

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
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.PanelColor = this.PanelColor;
            memento.ContextMenuSetting = this.ContextMenuSetting.Clone();

            memento.IsHideMenu = this.IsHideMenu;
            memento.IsHidePageSlider = this.IsHidePageSlider;
            memento.IsVisibleAddressBar = this.IsVisibleAddressBar;
            memento.IsHidePanel = this.IsHidePanel;
            memento.IsHidePanelInFullscreen = this.IsHidePanelInFullscreen;
            memento.IsVisibleWindowTitle = this.IsVisibleWindowTitle;


            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.PanelColor = memento.PanelColor;
            this.ContextMenuSetting = memento.ContextMenuSetting.Clone();

            this.IsHideMenu = memento.IsHideMenu;
            this.IsHidePageSlider = memento.IsHidePageSlider;
            this.IsHidePanel = memento.IsHidePanel;
            this.IsVisibleAddressBar = memento.IsVisibleAddressBar;
            this.IsHidePanelInFullscreen = memento.IsHidePanelInFullscreen;
            this.IsVisibleWindowTitle = memento.IsVisibleWindowTitle;
        }

        #endregion

    }
}
