// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Collections.ObjectModel;

namespace NeeView
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Property: SusiePluginPath
        private string _SusiePluginPath;
        public string SusiePluginPath
        {
            get { return _SusiePluginPath; }
            set
            {
                _SusiePluginPath = value;
                UpdateSusiePluginSetting(value);
            }
        }
        #endregion


        // 設定
        public Setting Setting { get; set; }
        public BookHistory.Memento History { get; set; }
        public SusieContext.Memento OldSusieSetting { get; set; }

        //
        private bool IsDartySusieSetting;

        // Susieプラグインリスト
        public ObservableCollection<Susie.SusiePlugin> AMPluginList { get; private set; }
        public ObservableCollection<Susie.SusiePlugin> INPluginList { get; private set; }

        // コマンド一覧用パラメータ
        public class CommandParam
        {
            public CommandType Key { get; set; }
            public string Group { get; set; }
            public string Header { get; set; }
            public string ShortCut { get; set; }
            public string MouseGesture { get; set; }
            public bool IsShowMessage { get; set; }
            public bool IsToggled { get; set; }
            public bool IsToggleEditable { get; set; }
            public Visibility ToggleVisibility { get; set; }
            public string Tips { get; set; }
        }

        // コマンド一覧
        public ObservableCollection<CommandParam> CommandCollection { get; set; }


        // Susieプラグイン コンフィグコマンド
        public static readonly RoutedCommand SusiePluginConfigCommand = new RoutedCommand("SusiePluginConfigCommand", typeof(SettingWindow));


        // 背景タイプリスト
        public static Dictionary<BackgroundStyle, string> BackgroundStyleList { get; } = new Dictionary<BackgroundStyle, string>
        {
            [BackgroundStyle.Black] = "黒色",
            [BackgroundStyle.White] = "白色",
            [BackgroundStyle.Auto] = "画像に合わせた色",
            [BackgroundStyle.Check] = "チェック模様",
        };

        // パネルカラーリスト
        public static Dictionary<PanelColor, string> PanelColorList { get; } = new Dictionary<PanelColor, string>
        {
            [PanelColor.Dark] = "黒色",
            [PanelColor.Light] = "白色",
        };


        // 通知表示タイプリスト
        public static Dictionary<ShowMessageStyle, string> ShowMessageTypeList { get; } = new Dictionary<ShowMessageStyle, string>
        {
            [ShowMessageStyle.None] = "表示しない",
            [ShowMessageStyle.Normal] = "表示する",
            [ShowMessageStyle.Tiny] = "小さく表示する",
        };


        // スライドショー切り替え時間リスト
        public static List<double> SlideShowIntervalList { get; } = new List<double>
        {
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            15,
            20,
            30,
            60,
        };


        // ドラッグアクション
        public static Dictionary<DragActionType, string> DragActionTypeList { get; } = new Dictionary<DragActionType, string>
        {
            [DragActionType.None] = "なし",
            [DragActionType.Move] = "移動",
            [DragActionType.MoveScale] = "移動(スケール依存)",
            [DragActionType.Angle] = "回転",
            [DragActionType.Scale] = "拡大縮小",
            [DragActionType.ScaleSlider] = "拡大縮小(スライド式)",
            [DragActionType.FlipHorizontal] = "左右反転",
            [DragActionType.FlipVertical] = "上下反転",
            [DragActionType.WindowMove] = "ウィンドウ移動",
        };

        public DragActionType DragActionNone
        {
            get { return DragKeyTable.Elements["LeftDrag"]; }
            set { DragKeyTable.Elements["LeftDrag"] = value; }
        }

        public DragActionType DragActionControl
        {
            get { return DragKeyTable.Elements["Ctrl+LeftDrag"]; }
            set { DragKeyTable.Elements["Ctrl+LeftDrag"] = value; }
        }

        public DragActionType DragActionShift
        {
            get { return DragKeyTable.Elements["Shift+LeftDrag"]; }
            set { DragKeyTable.Elements["Shift+LeftDrag"] = value; }
        }

        public DragActionType DragActionAlt
        {
            get { return DragKeyTable.Elements["Alt+LeftDrag"]; }
            set { DragKeyTable.Elements["Alt+LeftDrag"] = value; }
        }



        public DragActionType DragActionMiddleNone
        {
            get { return DragKeyTable.Elements["MiddleDrag"]; }
            set { DragKeyTable.Elements["MiddleDrag"] = value; }
        }

        public DragActionType DragActionMiddleControl
        {
            get { return DragKeyTable.Elements["Ctrl+MiddleDrag"]; }
            set { DragKeyTable.Elements["Ctrl+MiddleDrag"] = value; }
        }

        public DragActionType DragActionMiddleShift
        {
            get { return DragKeyTable.Elements["Shift+MiddleDrag"]; }
            set { DragKeyTable.Elements["Shift+MiddleDrag"] = value; }
        }

        public DragActionType DragActionMiddleAlt
        {
            get { return DragKeyTable.Elements["Alt+MiddleDrag"]; }
            set { DragKeyTable.Elements["Alt+MiddleDrag"] = value; }
        }


        #region Property: ExternalApplicationParam
        public string ExternalApplicationParam
        {
            get { return Setting.BookHubMemento.ExternalApplication.Parameter; }
            set
            {
                var validValue = ExternalApplication.ValidateApplicationParam(value);
                Setting.BookHubMemento.ExternalApplication.Parameter = validValue;
                OnPropertyChanged();
            }
        }
        #endregion

        //
        public static Dictionary<ExternalApplication.MultiPageOptionType, string> MultiPageOptionTypeList { get; } = new Dictionary<ExternalApplication.MultiPageOptionType, string>
        {
            [ExternalApplication.MultiPageOptionType.Once] = "1ページのみ実行する",
            [ExternalApplication.MultiPageOptionType.Twice] = "2ページとも実行する",
        };

        //
        public static Dictionary<ExternalApplication.ArchiveOptionType, string> ArchiveOptionTypeList { get; } = new Dictionary<ExternalApplication.ArchiveOptionType, string>
        {
            [ExternalApplication.ArchiveOptionType.None] = "実行しない",
            [ExternalApplication.ArchiveOptionType.SendArchiveFile] = "圧縮ファイルを渡す",
            [ExternalApplication.ArchiveOptionType.SendExtractFile] = "出力したファイルを渡す(一時ファイル)",
        };
        
        //
        public DragActionTable.KeyTable DragKeyTable { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="setting"></param>
        public SettingWindow(Setting setting, BookHistory.Memento history)
        {
            InitializeComponent();

            Setting = setting;
            History = history;
            OldSusieSetting = setting.SusieMemento.Clone();
            IsDartySusieSetting = false;

            // ドラッグキーテーブル作成
            DragKeyTable = new DragActionTable.KeyTable(Setting.DragActionMemento);

            // コマンド一覧作成
            CommandCollection = new ObservableCollection<CommandParam>();
            UpdateCommandList();

            // プラグイン一覧作成
            _SusiePluginPath = Setting.SusieMemento.SusiePluginPath ?? "";
            UpdateSusiePluginList();

            // 自身をコンテキストにする
            this.DataContext = this;

            // コマンド設定
            this.AMPluginListView.CommandBindings.Add(new CommandBinding(SusiePluginConfigCommand, SusiePluginConfigCommand_Executed));
            this.INPluginListView.CommandBindings.Add(new CommandBinding(SusiePluginConfigCommand, SusiePluginConfigCommand_Executed));

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));

            // デフォルトのプラグインパス設定
            this.PluginPathTextBox.DefaultDirectory = Susie.Susie.GetSusiePluginInstallPath();
        }


        // コマンド一覧 更新
        private void UpdateCommandList()
        {
            CommandCollection.Clear();
            foreach (var element in ModelContext.CommandTable)
            {
                if (element.Key.IsDisable()) continue;

                var item = new CommandParam()
                {
                    Key = element.Key,
                    Group = element.Value.Group,
                    Header = element.Value.Text,
                    ShortCut = Setting.CommandMememto[element.Key].ShortCutKey,
                    MouseGesture = Setting.CommandMememto[element.Key].MouseGesture,
                    IsShowMessage = Setting.CommandMememto[element.Key].IsShowMessage,
                    IsToggled = Setting.CommandMememto[element.Key].IsToggled,
                    ToggleVisibility = (element.Value.Attribute & CommandAttribute.ToggleEditable) == CommandAttribute.ToggleEditable ? Visibility.Visible : Visibility.Hidden,
                    IsToggleEditable = (element.Value.Attribute & CommandAttribute.ToggleLocked) != CommandAttribute.ToggleLocked,
                    Tips = element.Value.NoteToTips(),
                };
                CommandCollection.Add(item);
            }
        }

        // Susieプラグイン コンフィグ実行
        private void SusiePluginConfigCommand_Executed(object source, ExecutedRoutedEventArgs e)
        {
            var plugin = (Susie.SusiePlugin)e.Parameter;

            int result = plugin.ConfigurationDlg(this);

            // 設定ウィンドウが呼び出せなかった場合はアバウト画面でお茶を濁す
            if (result < 0)
            {
                plugin.AboutDlg(this);
            }
        }

        // Susie環境 更新
        private void UpdateSusiePluginSetting(string path)
        {
            // プラグインリスト書き戻し
            if (ModelContext.Susie != null)
            {
                ModelContext.Susie.AMPlgunList = AMPluginList.ToList();
                ModelContext.Susie.INPlgunList = INPluginList.ToList();
            }

            // 現在のSusieプラグイン情報保存
            Setting.SusieMemento.SusiePluginPath = path;
            Setting.SusieMemento.SpiFiles = SusieContext.Memento.CreateSpiFiles(ModelContext.Susie);

            ModelContext.SusieContext.Restore(Setting.SusieMemento);
            UpdateSusiePluginList();
        }

        // Susieプラグイン一覧 更新
        public void UpdateSusiePluginList()
        {
            INPluginList = new ObservableCollection<Susie.SusiePlugin>(ModelContext.Susie?.INPlgunList);
            OnPropertyChanged(nameof(INPluginList));
            this.INPluginListView.Items.Refresh();

            AMPluginList = new ObservableCollection<Susie.SusiePlugin>(ModelContext.Susie?.AMPlgunList);
            OnPropertyChanged(nameof(AMPluginList));
            this.AMPluginListView.Items.Refresh();
        }

        // 決定ボタン処理
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        // キャンセルボタン処理
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ショートカットキー設定ボタン処理
        private void ShortCutSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (CommandParam)this.CommandListView.SelectedValue;

            var dialog = new InputGestureSettingWindow(value);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                this.CommandListView.Items.Refresh();
            }
        }

        // マウスジェスチャー設定ボタン処理
        private void MouseGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (CommandParam)this.CommandListView.SelectedValue;

            var dialog = new MouseGestureSettingWindow(value);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                this.CommandListView.Items.Refresh();
            }
        }

        // キーバインド初期化ボタン処理
        private void ResetGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            Setting.CommandMememto = CommandTable.CreateDefaultMemento();
            UpdateCommandList();
            this.CommandListView.Items.Refresh();
        }

        // 履歴クリアボタン処理
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            History.Items.Clear();
            OnPropertyChanged(nameof(History));
        }

        // プラグインリスト：ドロップ受付判定
        private void PluginListView_PreviewDragOver(object sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e);
        }

        // プラグインリスト：ドロップ
        private void PluginListView_Drop(object sender, DragEventArgs e)
        {
            var list = (sender as ListBox).Tag as ObservableCollection<Susie.SusiePlugin>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop<Susie.SusiePlugin>(sender, e, list);
            }
        }

        // 設定画面終了処理
        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.DialogResult == true)
            {
                // ドラッグキーバインド反映
                DragKeyTable.UpdateMemento();

                // コマンド設定反映
                foreach (var command in CommandCollection)
                {
                    Setting.CommandMememto[command.Key].ShortCutKey = command.ShortCut;
                    Setting.CommandMememto[command.Key].MouseGesture = command.MouseGesture;
                    Setting.CommandMememto[command.Key].IsShowMessage = command.IsShowMessage;
                    Setting.CommandMememto[command.Key].IsToggled = command.IsToggled;
                }

                // プラグインリスト書き戻し
                if (ModelContext.Susie != null)
                {
                    ModelContext.Susie.AMPlgunList = AMPluginList.ToList();
                    ModelContext.Susie.INPlgunList = INPluginList.ToList();
                }

                // Susie プラグインリスト保存
                Setting.SusieMemento.SpiFiles = SusieContext.Memento.CreateSpiFiles(ModelContext.Susie);
            }
            else
            {
                // Susie設定を元に戻す
                if (IsDartySusieSetting)
                {
                    Setting.SusieMemento = OldSusieSetting;
                    ModelContext.SusieContext.Restore(Setting.SusieMemento);
                }
            }
        }

        // Susie設定のタブが選択された
        private void SusieSettingTab_Selected(object sender, RoutedEventArgs e)
        {
            IsDartySusieSetting = true;
        }

        // コンテキストメニュー編集ボタンが押された
        private void EditContextMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContextMenuSettingWindow(Setting.ViewMemento);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                // nop.
            }
        }

        private void GestureContextMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var command = new CommandParam()
            {
                Header = "コンテキストメニューを開く",
                MouseGesture = Setting.ViewMemento.ContextMenuSetting.MouseGesture
            };

            var dialog = new MouseGestureSettingWindow(command);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                Setting.ViewMemento.ContextMenuSetting.MouseGesture = command.MouseGesture;
            }
        }
    }


    // プラグイングループ分け用
    [ValueConversion(typeof(string), typeof(string))]
    public class ApiVersionToApiTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)value)
            {
                case "00AM":
                    return "圧縮ファイル展開用プラグイン";
                case "00IN":
                    return "画像表示用プラグイン";
                default:
                    return "その他";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ジェスチャー表示用コンバータ
    [ValueConversion(typeof(string), typeof(string))]
    public class MouseGestureToDispTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            return new MouseGestureSequence((string)value).ToDispString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
