// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Win32;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
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

        #region Fields

        // パネル表示ロック
        private bool _isPanelVisibleLocked;

        // 古いパネル表示ロック。コマンドでロックのトグルをできるようにするため
        private bool _isPanelVisibleLockedOld;


        private PanelColor _panelColor = PanelColor.Dark;
        private ContextMenuSetting _contextMenuSetting = new ContextMenuSetting();
        private bool _isHideMenu;
        private bool _isIsHidePageSlider;
        private bool _isHidePanel; // = true;

        //
        private bool _IsHidePanelInFullscreen = true;
        private bool _IsVisibleWindowTitle = true;
        private bool _isVisibleAddressBar;
        private bool _isVisibleBusy = true;


        #endregion

        #region Constructors

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

        #endregion

        #region Properties

        // 「ブックを開く」ダイアログを現在の場所を基準にして開く
        [PropertyMember("「ファイルを開く」でのファイル選択ダイアログの開始場所を現在開いているブックの場所にする")]
        public bool IsOpenbookAtCurrentPlace { get; set; }

        //
        [PropertyMember("テーマカラー")]
        public PanelColor PanelColor
        {
            get { return _panelColor; }
            set { if (_panelColor != value) { _panelColor = value; RaisePropertyChanged(); } }
        }

        //
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
        public bool IsHideMenu
        {
            get { return _isHideMenu; }
            set
            {
                _isHideMenu = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHideMenu));
            }
        }

        //
        public bool CanHideMenu => IsHideMenu || WindowShape.Current.IsFullScreen;

        // スライダーを自動的に隠す
        public bool IsHidePageSlider
        {
            get { return _isIsHidePageSlider; }
            set
            {
                _isIsHidePageSlider = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanHidePageSlider));
            }
        }

        //
        public bool CanHidePageSlider => IsHidePageSlider || WindowShape.Current.IsFullScreen;

        // パネルを自動的に隠す
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

        /// <summary>
        /// フルスクリーン時にパネルを隠す
        /// </summary>
        [PropertyMember("フルスクリーンのときにパネルを自動的に隠す")]
        public bool IsHidePanelInFullscreen
        {
            get { return _IsHidePanelInFullscreen; }
            set { if (_IsHidePanelInFullscreen != value) { _IsHidePanelInFullscreen = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(CanHidePanel)); } }
        }

        // パネルを自動的に隠せるか
        public bool CanHidePanel => IsHidePanel || (IsHidePanelInFullscreen && WindowShape.Current.IsFullScreen);

        /// <summary>
        /// IsVisibleWindowTitle property.
        /// タイトルバーが表示されておらず、スライダーにフォーカスがある場合等にキャンバスにタイトルを表示する
        /// </summary>
        [PropertyMember("タイトルバーが表示されない時に、表示エリアにウィンドウタイトルを表示する")]
        public bool IsVisibleWindowTitle
        {
            get { return _IsVisibleWindowTitle; }
            set { if (_IsVisibleWindowTitle != value) { _IsVisibleWindowTitle = value; RaisePropertyChanged(); } }
        }

        // アドレスバーON/OFF
        public bool IsVisibleAddressBar
        {
            get { return _isVisibleAddressBar; }
            set { _isVisibleAddressBar = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// パネル表示状態をロックする
        /// </summary>
        public bool IsPanelVisibleLocked
        {
            get { return _isPanelVisibleLocked; }
            set
            {
                if (_isPanelVisibleLocked != value)
                {
                    _isPanelVisibleLocked = value;
                    RaisePropertyChanged();
                    SidePanel.Current.IsVisibleLocked = _isPanelVisibleLocked;
                }
            }
        }

        /// <summary>
        /// IsVisibleBusy property.
        /// </summary>
        [PropertyMember("画像読み込み処理中マークを画面左上に表示する")]
        public bool IsVisibleBusy
        {
            get { return _isVisibleBusy; }
            set { if (_isVisibleBusy != value) { _isVisibleBusy = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// メニューエリアマウスオーバー
        /// Viewから更新される
        /// </summary>
        public bool IsMenuAreaMouseOver { get; set; }

        /// <summary>
        /// ステータスエリアマウスオーバー
        /// Viewから更新される
        /// </summary>
        public bool IsStatusAreaMouseOver { get; set; }

        // メニューエリア、ステータスエリアどちらかの上にマウスがある
        public bool IsFontAreaMouseOver => IsMenuAreaMouseOver || IsStatusAreaMouseOver;

        // 何かキーが押されているか
        public AnyKey AnyKey { get; } = new AnyKey();

        #endregion

        #region Methods

        //
        public bool ToggleHideMenu()
        {
            IsHideMenu = !IsHideMenu;
            return IsHideMenu;
        }

        //
        public bool ToggleHidePageSlider()
        {
            IsHidePageSlider = !IsHidePageSlider;
            return IsHidePageSlider;
        }

        public bool ToggleHidePanel()
        {
            IsHidePanel = !IsHidePanel;
            return IsHidePanel;
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
            WindowShape.Current.WindowChromeFrame = App.Current.WindowChromeFrame;

            var setting = SaveData.Current.UserSetting;

            // 設定反映
            SaveData.Current.RestoreSetting(setting, true);
            SaveData.Current.RestoreSettingCompatible(setting, true);

            // 履歴読み込み
            SaveData.Current.LoadHistory(setting);

            // ブックマーク読み込み
            SaveData.Current.LoadBookmark(setting);

            // ページマーク読込
            SaveData.Current.LoadPagemark(setting);

            SaveData.Current.UserSetting = null; // ロード設定破棄


            // フォルダーを開く
            if (App.Current.Option.IsBlank != SwitchOption.on)
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
            if (!App.Current.IsOpenLastBook) return;

            string place = BookHistory.Current.LastAddress;
            if (place != null || System.IO.Directory.Exists(place) || System.IO.File.Exists(place))
            {
                BookHub.Current.RequestLoad(place, null, BookLoadOption.Resume, true);
            }
        }


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
            if (this.IsOpenbookAtCurrentPlace && BookHub.Current.Book != null)
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
            bool isScrolled = DragTransformControl.Current.ScrollN(-1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation, parameter.Scroll / 100.0);

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
            bool isScrolled = DragTransformControl.Current.ScrollN(+1, bookReadDirection, parameter.IsNScroll, parameter.Margin, parameter.IsAnimation, parameter.Scroll / 100.0);

            if (!isScrolled)
            {
                ContentCanvas.Current.NextViewOrigin = (BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.RightToLeft) ? DragViewOrigin.RightTop : DragViewOrigin.LeftTop;
                BookOperation.Current.NextPage();
            }
        }


        // 設定ウィンドウを開く
        public void OpenSettingWindow()
        {
            if (Setting.SettingWindow.Current != null) return;

            var dialog = new Setting.SettingWindow(new Setting.SettingWindowModel());
            dialog.Owner = App.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Show();
        }

        // 設定ウィンドウを閉じる
        public bool CloseSettingWindow()
        {
            if (Setting.SettingWindow.Current != null)
            {
                Setting.SettingWindow.Current.Close();
                return true;
            }
            else
            {
                return false;
            }
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
            if (Config.Current.IsAppxPackage)
            {
                new MessageDialog($"ストアアプリでは設定ファイルの場所を開くことができません", "このコマンドは使用できません").ShowDialog();
                return;
            }

            Process.Start("explorer.exe", $"\"{Config.Current.LocalApplicationDataPath}\"");
        }

        // オンラインヘルプ
        public void OpenOnlineHelp()
        {
            System.Diagnostics.Process.Start("https://bitbucket.org/neelabo/neeview/wiki/");
        }


        // 履歴削除
        // TODO: 直接変更し、最近使ったファイルはイベントで更新すべき
        public void ClearHistory()
        {
            BookHistory.Current.Clear();
            MenuBar.Current.UpdateLastFiles();
        }

        /// <summary>
        /// パネル表示ロック開始
        /// コマンドから呼ばれる
        /// </summary>
        public void EnterVisibleLocked()
        {
            this.IsPanelVisibleLocked = !_isPanelVisibleLockedOld;
            _isPanelVisibleLockedOld = _isPanelVisibleLocked;
        }

        /// <summary>
        /// パネル表示ロック解除
        /// 他の操作をした場所から呼ばれる
        /// </summary>
        public void LeaveVisibleLocked()
        {
            if (_isPanelVisibleLocked)
            {
                _isPanelVisibleLockedOld = true;
                this.IsPanelVisibleLocked = false;
            }
            else
            {
                _isPanelVisibleLockedOld = false;
            }
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelColor PanelColor { get; set; }
            [DataMember]
            public ContextMenuSetting ContextMenuSetting { get; set; }
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

            [DataMember, DefaultValue(true)]
            public bool IsVisibleBusy { get; set; }

            [DataMember, DefaultValue(false)]
            public bool IsOpenbookAtCurrentPlace { get; set; }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                IsVisibleBusy = true;
            }
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
            memento.IsVisibleBusy = this.IsVisibleBusy;
            memento.IsOpenbookAtCurrentPlace = this.IsOpenbookAtCurrentPlace;

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
            this.IsVisibleBusy = memento.IsVisibleBusy;
            this.IsOpenbookAtCurrentPlace = memento.IsOpenbookAtCurrentPlace;
        }

        #endregion
    }
}
