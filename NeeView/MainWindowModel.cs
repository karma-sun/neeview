// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Win32;
using NeeView.ComponentModel;
using NeeView.Windows.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    // パネルカラー
    public enum PanelColor
    {
        Dark,
        Light,
    }


    /// <summary>
    /// Load command.
    /// </summary>
    public class LoadCommand : ICommand
    {
        public static LoadCommand Command { get; } = new LoadCommand();

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return !BookHub.Current.IsLoading;
        }

        public void Execute(object parameter)
        {
            var path = parameter as string;
            if (parameter == null) return;
            BookHub.Current.RequestLoad(path, null, BookLoadOption.None, true);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }



    /// <summary>
    /// MainWindow : Model
    /// </summary>
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



        // 起動時処理
        public void Loaded()
        {
            // Chrome反映
            WindowShape.Current.WindowChromeFrame = Preference.Current.window_chrome_frame;

            // VMイベント設定
            ////InitializeViewModelEvents();

            var setting = SaveData.Current.Setting;

            // 設定反映
            SaveData.Current.RestoreSetting(setting, true);
            SaveData.Current.RestoreSettingCompatible(setting, true);

            // 履歴読み込み
            SaveData.Current.LoadHistory(setting);

            // ブックマーク読み込み
            SaveData.Current.LoadBookmark(setting);

            // ページマーク読込
            SaveData.Current.LoadPagemark(setting);

            SaveData.Current.Setting = null; // ロード設定破棄


            // フォルダーを開く
            if (App.Current.Option.IsBlank != SwitchOption.on )
            {
                if (App.Current.Option.StartupPlace != null)
                {
                    // 起動引数の場所で開く
                    BookHub.Current.RequestLoad(App.Current.Option.StartupPlace, null, BookLoadOption.None, true);
                }
                else
                {
                    // 最後に開いたフォルダーを復元する
                    LoadLastFolder();
                }
            }

            // スライドショーの自動再生
            if (App.Current.Option.IsSlideShow != null ? App.Current.Option.IsSlideShow == SwitchOption.on : SlideShow.Current.IsAutoPlaySlideShow)
            {
                SlideShow.Current.IsPlayingSlideShow = true;
            }
        }

        // 最後に開いたフォルダーを開く
        private void LoadLastFolder()
        {
            if (!Preference.Current.bootup_lastfolder) return;

            string place = BookHistory.Current.LastAddress;
            if (place != null || System.IO.Directory.Exists(place) || System.IO.File.Exists(place))
            {
                BookHub.Current.RequestLoad(place, null, BookLoadOption.Resume, true);
            }
        }


        #region Commands




        // ダイアログでファイル選択して画像を読み込む
        public void LoadAs()
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = GetDefaultFolder();

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                BookHub.Current.RequestLoad(dialog.FileName, null, BookLoadOption.None, true);
            }
            else
            {
                return;
            }
        }


        // ファイルを開く基準となるフォルダーを取得
        private string GetDefaultFolder()
        {
            // 既に開いている場合、その場所を起点とする
            if (Preference.Current.openbook_begin_current && BookHub.Current.Book != null)
            {
                return System.IO.Path.GetDirectoryName(BookHub.Current.Book.Place);
            }
            else
            {
                return "";
            }
        }


        // スクロール＋前のページに戻る
        public void PrevScrollPage()
        {
            var parameter = (ScrollPageCommandParameter)CommandTable.Current[CommandType.PrevScrollPage].Parameter;

            int bookReadDirection = (BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = MouseInput.Current.Drag.ScrollN(-1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation);

            if (!isScrolled)
            {
                ContentCanvas.Current.NextViewOrigin = (BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightBottom : DragViewOrigin.LeftBottom;
                BookOperation.Current.PrevPage();
            }
        }

        // スクロール＋次のページに進む
        public void NextScrollPage()
        {
            var parameter = (ScrollPageCommandParameter)CommandTable.Current[CommandType.NextScrollPage].Parameter;

            int bookReadDirection = (BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? 1 : -1;
            bool isScrolled = MouseInput.Current.Drag.ScrollN(+1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation);

            if (!isScrolled)
            {
                ContentCanvas.Current.NextViewOrigin = (BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightTop : DragViewOrigin.LeftTop;
                BookOperation.Current.NextPage();
            }
        }


        // 設定ウィンドウを開く
        public void OpenSettingWindow()
        {
            var setting = SaveData.Current.CreateSetting();
            var history = BookHistory.Current.CreateMemento(false);

            // スライドショー停止
            SlideShow.Current.PauseSlideShow();

            var dialog = new SettingWindow(setting, history);
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                SaveData.Current.RestoreSetting(setting, false);
                WindowShape.Current.CreateSnapMemento();
                SaveData.Current.SaveSetting();
                BookHistory.Current.Restore(history, false);

                // 現在ページ再読込
                BookHub.Current.ReLoad();
            }

            // スライドショー再開
            SlideShow.Current.ResumeSlideShow();
        }


        // バージョン情報を表示する
        public void OpenVersionWindow()
        {
            var dialog = new VersionWindow();
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }


        // 設定ファイルの場所を開く
        public void OpenSettingFilesFolder()
        {
            Process.Start("explorer.exe", $"\"{Config.Current .LocalApplicationDataPath}\"");
        }

        // オンラインヘルプ
        public void OpenOnlineHelp()
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }

        #endregion



        // 履歴削除
        // TODO: 直接変更し、最近使ったファイルはイベントで更新すべき
        public void ClearHistory()
        {
            BookHistory.Current.Clear();
            MenuBar.Current.UpdateLastFiles();
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
