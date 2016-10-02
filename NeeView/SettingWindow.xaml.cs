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
using System.Diagnostics;

namespace NeeView
{
    //
    public class GestureElement
    {
        public string Gesture { get; set; }
        public bool IsConflict { get; set; }
        public string Splitter { get; set; }
        public string Note { get; set; }
    }

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
        public class CommandParam : INotifyPropertyChanged
        {
            #region NotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
            #endregion

            public CommandType Key { get; set; }
            public string Group { get; set; }
            public string Header { get; set; }

            public string ShortCut { get; set; }
            public string ShortCutNote { get; set; }
            public ObservableCollection<GestureElement> ShortCuts { get; set; } = new ObservableCollection<GestureElement>();

            public string MouseGesture { get; set; }
            public GestureElement MouseGestureElement { get; set; }

            public bool IsShowMessage { get; set; }
            public string Tips { get; set; }

            public string ParameterJson { get; set; }
            public bool HasParameter { get; set; }
            public CommandType ParameterShareCommandType { get; set; }
            public bool IsShareParameter => ParameterShareCommandType != CommandType.None;
            public string ShareTips => $"「{ParameterShareCommandType.ToDispString()}」とパラメータ共有です";
        }

        // コマンド一覧
        public ObservableCollection<CommandParam> CommandCollection { get; set; }


        //
        private Preference _Preference;

        // 詳細設定一覧用パラメータ
        public class PreferenceParam
        {
            public PreferenceElement Source { get; set; }

            public string Key => Source.Key;
            public string State => Source.HasCustomValue ? "ユーザ設定" : "初期設定値";
            public string TypeString => Source.GetValueTypeString();
            public string Value => Source.Value.ToString();
            public string Tips => Source.Note;
        }

        // 詳細設定一覧
        public ObservableCollection<PreferenceParam> PreferenceCollection { get; set; }


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
            [PanelColor.Dark] = "ダーク",
            [PanelColor.Light] = "ライトグレー",
        };


        // 通知表示タイプリスト
        public static Dictionary<ShowMessageStyle, string> ShowMessageTypeList { get; } = new Dictionary<ShowMessageStyle, string>
        {
            [ShowMessageStyle.None] = "表示しない",
            [ShowMessageStyle.Normal] = "表示する",
            [ShowMessageStyle.Tiny] = "小さく表示する",
        };

        //
        public static Dictionary<FolderListItemStyle, string> FolderListItemStyleList { get; } = new Dictionary<FolderListItemStyle, string>
        {
            [FolderListItemStyle.Normal] = "テキスト表示",
            [FolderListItemStyle.Picture] = "バナー表示",
        };

        //
        public static Dictionary<LongButtonDownMode, string> LongButtonDownModeList { get; } = new Dictionary<LongButtonDownMode, string>
        {
            [LongButtonDownMode.None] = "なし",
            [LongButtonDownMode.Loupe] = "ルーペ",
        };


        // ドラッグアクション
        public static Dictionary<DragActionType, string> DragActionTypeList { get; } = DragActionTypeExtension.LabelList;


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
        public static Dictionary<MultiPageOptionType, string> MultiPageOptionTypeList { get; } = new Dictionary<MultiPageOptionType, string>
        {
            [MultiPageOptionType.Once] = "1ページのみ実行する",
            [MultiPageOptionType.Twice] = "2ページとも実行する",
        };

        //
        public static Dictionary<ArchiveOptionType, string> ArchiveOptionTypeList { get; } = new Dictionary<ArchiveOptionType, string>
        {
            [ArchiveOptionType.None] = "実行しない",
            [ArchiveOptionType.SendArchiveFile] = "圧縮ファイルを渡す",
            [ArchiveOptionType.SendExtractFile] = "出力したファイルを渡す(一時ファイル)",
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

            //
            this.RemoveAllDataButton.Visibility = App.Config.IsUseLocalApplicationDataFolder ? Visibility.Visible : Visibility.Collapsed;

            Setting = setting;
            History = history;
            OldSusieSetting = setting.SusieMemento.Clone();
            IsDartySusieSetting = false;

            // ドラッグキーテーブル作成
            DragKeyTable = new DragActionTable.KeyTable(Setting.DragActionMemento);

            // コマンド一覧作成
            CommandCollection = new ObservableCollection<CommandParam>();
            UpdateCommandList();

            // 詳細設定一覧作成
            _Preference = new Preference();
            _Preference.Restore(Setting.PreferenceMemento);
            PreferenceCollection = new ObservableCollection<PreferenceParam>();
            UpdatePreferenceList();

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

                var memento = Setting.CommandMememto[element.Key];

                var item = new CommandParam()
                {
                    Key = element.Key,
                    Group = element.Value.Group,
                    Header = element.Value.Text,
                    ShortCut = memento.ShortCutKey,
                    MouseGesture = memento.MouseGesture,
                    IsShowMessage = memento.IsShowMessage,
                    Tips = element.Value.NoteToTips(),
                };

                if (element.Value.HasParameter)
                {
                    item.HasParameter = true;
                    item.ParameterJson = memento.Parameter;

                    var share = element.Value.DefaultParameter as ShareCommandParameter;
                    if (share != null)
                    {
                        item.ParameterShareCommandType = share.CommandType;
                    }
                }

                CommandCollection.Add(item);
            }

            UpdateCommandListShortCut();
            UpdateCommandListMouseGesture();

            this.CommandListView.Items.Refresh();
        }

        // コマンド一覧 ショートカット更新
        private void UpdateCommandListShortCut()
        {
            foreach (var item in CommandCollection)
            {
                item.ShortCutNote = null;

                if (!string.IsNullOrEmpty(item.ShortCut))
                {
                    var shortcuts = new ObservableCollection<GestureElement>();
                    foreach (var key in item.ShortCut.Split(','))
                    {
                        var overlaps = CommandCollection
                            .Where(e => !string.IsNullOrEmpty(e.ShortCut) && e.Key != item.Key && e.ShortCut.Split(',').Contains(key))
                            .Select(e => $"「{e.Key.ToDispString()}」")
                            .ToList();

                        if (overlaps.Count > 0)
                        {
                            if (item.ShortCutNote != null) item.ShortCutNote += "\n";
                            item.ShortCutNote += $"{key} は {string.Join("", overlaps)} と競合しています";
                        }

                        var element = new GestureElement();
                        element.Gesture = key;
                        element.IsConflict = overlaps.Count > 0;
                        element.Splitter = ",";

                        shortcuts.Add(element);
                    }

                    if (shortcuts.Count > 0)
                    {
                        shortcuts.Last().Splitter = null;
                    }

                    item.ShortCuts = shortcuts;
                }
                else
                {
                    item.ShortCuts = new ObservableCollection<GestureElement>();
                }
            }
        }


        // コマンド一覧 マウスジェスチャー更新
        private void UpdateCommandListMouseGesture()
        {
            foreach (var item in CommandCollection)
            {
                if (!string.IsNullOrEmpty(item.MouseGesture))
                {
                    var overlaps = CommandCollection
                        .Where(e => e.Key != item.Key && e.MouseGesture == item.MouseGesture)
                        .Select(e => $"「{e.Key.ToDispString()}」")
                        .ToList();

                    var element = new GestureElement();
                    element.Gesture = item.MouseGesture;
                    element.IsConflict = overlaps.Count > 0;
                    if (overlaps.Count > 0)
                    {
                        element.Note = $"{string.Join("", overlaps)} と競合しています";
                    }

                    item.MouseGestureElement = element;
                }
                else
                {
                    item.MouseGestureElement = new GestureElement();
                }
            }
        }

        // 詳細一覧 更新
        private void UpdatePreferenceList()
        {
            PreferenceCollection.Clear();

            foreach (var element in _Preference.Dictionary)
            {
                if (element.Key.StartsWith(".")) continue;

                var item = new PreferenceParam()
                {
                    Source = element.Value,
                };
                PreferenceCollection.Add(item);
            }
        }

        //
        private void PreferenceListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // sender がダブルクリックされた項目
            ListViewItem targetItem = (ListViewItem)sender;

            // データバインディングを使っているなら、
            // DataContext からデータを取得できる
            PreferenceParam p = (PreferenceParam)targetItem.DataContext;
            EditPreference(p.Source, true);
        }

        //
        private void EditPreference(PreferenceElement param, bool isSimple)
        {
            if (isSimple && param.GetValueType() == typeof(bool))
            {
                param.Set(!param.Boolean);
                this.PreferenceListView.Items.Refresh();
            }
            else
            {
                var dialog = new PreferenceEditWindow(param);
                dialog.Owner = this;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    this.PreferenceListView.Items.Refresh();
                }
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


            var gestures = CommandCollection.ToDictionary(i => i.Key, i => i.ShortCut);
            var key = value.Key;
            var dialog = new InputGestureSettingWindow(gestures, key);

            //var dialog = new InputGestureSettingWindow (CommandCollection, value);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                foreach (var item in CommandCollection)
                {
                    item.ShortCut = gestures[item.Key];
                }

                UpdateCommandListShortCut();
                this.CommandListView.Items.Refresh();
            }
        }

        // マウスジェスチャー設定ボタン処理
        private void MouseGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (CommandParam)this.CommandListView.SelectedValue;

            var context = new MouseGestureSettingContext();
            context.Command = value.Key;
            context.Gestures = CommandCollection.ToDictionary(i => i.Key, i => i.MouseGesture);

            var dialog = new MouseGestureSettingWindow(context);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                foreach (var item in CommandCollection)
                {
                    item.MouseGesture = context.Gestures[item.Key];
                }

                UpdateCommandListMouseGesture();
                this.CommandListView.Items.Refresh();
            }
        }


        #region ParameterSettingCommand
        private RelayCommand _ParameterSettingCommand;
        public RelayCommand ParameterSettingCommand
        {
            get { return _ParameterSettingCommand = _ParameterSettingCommand ?? new RelayCommand(ParameterSettingCommand_Executed, ParameterSettingCommand_CanExecute); }
        }

        private bool ParameterSettingCommand_CanExecute()
        {
            var command = (CommandParam)this.CommandListView.SelectedValue;
            return (command != null && command.HasParameter && !command.IsShareParameter);
        }

        private void ParameterSettingCommand_Executed()
        {
            var command = (CommandParam)this.CommandListView.SelectedValue;
            EditCommandParameter(command);
        }
        #endregion



        // キーバインド初期化ボタン処理
        private void ResetGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBoxEx.Show(this, "コマンドの設定を全て初期化します。よろしいですか？", "確認", MessageBoxButton.OKCancel);

            if (result == true)
            {
                Setting.CommandMememto = CommandTable.CreateDefaultMemento();
                UpdateCommandList();
                this.CommandListView.Items.Refresh();
            }
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
                    Setting.CommandMememto[command.Key].Parameter = command.ParameterJson;
                }

                // Preference反映
                Setting.PreferenceMemento = _Preference.CreateMemento();

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GestureContextMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // 現状、コンテキストメニュー操作は他の操作との競合チェックを行わない

            var context = new MouseGestureSettingContext();
            context.Header = "コンテキストメニューを開く";
            context.Command = CommandType.None; // 仮
            context.Gestures = new Dictionary<CommandType, string>();
            context.Gestures.Add(context.Command, Setting.ViewMemento.ContextMenuSetting.MouseGesture);

            var dialog = new MouseGestureSettingWindow(context);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                //UpdateCommandListMouseGesture();
                //this.CommandListView.Items.Refresh();

                Setting.ViewMemento.ContextMenuSetting.MouseGesture = context.Gesture;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreferenceEditButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (PreferenceParam)this.PreferenceListView.SelectedValue;
            EditPreference(value.Source, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetAllPreferenceButton_Click(object sender, RoutedEventArgs e)
        {
            _Preference.Reset();
            this.PreferenceListView.Items.Refresh();
        }

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveAllData_Click(object sender, RoutedEventArgs e)
        {
            App.Config.RemoveApplicationData();
        }

        //
        private void EditCommandParameterButton_Clock(object sender, RoutedEventArgs e)
        {
            var command = (sender as Button)?.Tag as CommandParam;
            EditCommandParameter(command);
        }

        private void EditCommandParameter(CommandParam command)
        {
            if (command != null && command.HasParameter && !command.IsShareParameter)
            {
                var source = ModelContext.CommandTable[command.Key];
                var parameterDfault = source.DefaultParameter;

                var parameter = command.ParameterJson != null
                    ? (CommandParameter)Utility.Json.Deserialize(command.ParameterJson, source.DefaultParameter.GetType())
                    : parameterDfault.Clone();

                var context = CommandParameterEditContext.Create(parameter, command.Header);

                var dialog = new CommandParameterWindow(context, parameterDfault);
                dialog.Owner = this;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (dialog.ShowDialog() == true)
                {
                    command.ParameterJson = context.Source.ToJson();
                }
            }
        }

        private void CommandListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ParameterSettingCommand.RaiseCanExecuteChanged();
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

    // ドラッグ操作Tips表示用コンバータ
    [ValueConversion(typeof(DragActionType), typeof(string))]
    public class DragActionTypeToTipsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is DragActionType ? ((DragActionType)value).ToTips() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //  長押し操作Tips表示用コンバータ
    [ValueConversion(typeof(LongButtonDownMode), typeof(string))]
    public class LongButtonDownModeToTipsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is LongButtonDownMode ? ((LongButtonDownMode)value).ToTips() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // バナーサーズ表示用コンバータ
    [ValueConversion(typeof(double), typeof(string))]
    public class BannerSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                var bannerSize = (double)value;
                return $"{(int)bannerSize}x{(int)(bannerSize / 4)}";
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 履歴サイズ制限表示用コンバータ
    [ValueConversion(typeof(int), typeof(string))]
    public class HistoryLimitSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                var limitSize = (int)value;
                return limitSize == 0 ? "制限なし" : limitSize.ToString();
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    // 履歴サイズ制限表示用コンバータ
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class HistoryLimitSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TimeSpan)
            {
                var limitSpan = (TimeSpan)value;
                return limitSpan == default(TimeSpan) ? "制限なし" : $"{limitSpan.Days}日前まで";
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Null判定コンバータ
    [ValueConversion(typeof(object), typeof(bool))]
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
}
