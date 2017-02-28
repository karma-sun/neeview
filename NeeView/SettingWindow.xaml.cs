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
using NeeLaboratory.Property;

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

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Property: SusiePluginPath
        private string _susiePluginPath;
        public string SusiePluginPath
        {
            get { return _susiePluginPath; }
            set
            {
                _susiePluginPath = value;
                UpdateSusiePluginSetting(value);
            }
        }
        #endregion


        // 設定
        public Setting Setting { get; set; }
        public BookHistory.Memento History { get; set; }
        public SusieContext.Memento OldSusieSetting { get; set; }

        //
        private bool _isDartySusieSetting;

        // Susieプラグインリスト
        public ObservableCollection<Susie.SusiePlugin> AMPluginList { get; private set; }
        public ObservableCollection<Susie.SusiePlugin> INPluginList { get; private set; }



        // コマンド一覧用パラメータ
        public class CommandParam : INotifyPropertyChanged
        {
            #region NotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
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
        private Preference _preference;

        // 詳細設定一覧用パラメータ
        public class PreferenceParam
        {
            public PropertyMemberElement Source { get; set; }

            public string Key => Source.Path;
            public string Name => Source.Name;
            public string State => Source.HasCustomValue ? "ユーザ設定" : "初期設定値";
            public string TypeString => Source.GetValueTypeString();
            public string Value => Source.GetValue().ToString();
            public string Tips => Source.Tips;
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

        public static Dictionary<bool, string> ShowMessageVisibleList { get; } = new Dictionary<bool, string>
        {
            [false] = "表示しない",
            [true] = "表示する",
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


        //
        public static Dictionary<SliderDirection, string> SliderDirectionList { get; } = new Dictionary<SliderDirection, string>
        {
            [SliderDirection.LeftToRight] = "左から右",
            [SliderDirection.RightToLeft] = "右から左",
            [SliderDirection.SyncBookReadDirection] = "本を開く方向に依存",
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
                RaisePropertyChanged();
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
        public static Dictionary<PageEndAction, string> PageEndActionList { get; } = new Dictionary<PageEndAction, string>
        {
            [PageEndAction.None] = "そのまま",
            [PageEndAction.NextFolder] = "次のフォルダーに移動",
            [PageEndAction.Loop] = "ループする"
        };

        //
        public static Dictionary<PreLoadMode, string> PreLoadModeList { get; } = new Dictionary<PreLoadMode, string>
        {
            [PreLoadMode.None] = "しない",
            [PreLoadMode.AutoPreLoad] = "自動",
            [PreLoadMode.PreLoad] = "先読みする"
        };

        //
        public static Dictionary<PageMode, string> PageModeList => PageModeExtension.PageModeList;

        //
        public static Dictionary<PageReadOrder, string> PageReadOrderList => PageReadOrderExtension.PageReadOrderList;

        //
        public static Dictionary<PageSortMode, string> PageSortModeList => PageSortModeExtension.PageSortModeList;


        //
        public DragActionTable.KeyTable DragKeyTable { get; set; }

        // ビュー回転のスナップ値
        public AngleFrequency AngleFrequency { get; set; }

        // 履歴制限
        public HistoryLimitSize HistoryLimitSize { get; set; }
        public HistoryLimitSpan HistoryLimitSpan { get; set; }

        // スライドショー間隔
        public SlideShowInterval SlideShowInterval { get; set; }

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
            _isDartySusieSetting = false;

            // ドラッグキーテーブル作成
            DragKeyTable = new DragActionTable.KeyTable(Setting.DragActionMemento);

            // コマンド一覧作成
            CommandCollection = new ObservableCollection<CommandParam>();
            UpdateCommandList();

            // 詳細設定一覧作成
            _preference = new Preference();
            _preference.Restore(Setting.PreferenceMemento);
            PreferenceCollection = new ObservableCollection<PreferenceParam>();
            UpdatePreferenceList();

            // プラグイン一覧作成
            _susiePluginPath = Setting.SusieMemento.SusiePluginPath ?? "";
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

            // View AngleFrequency
            AngleFrequency = new AngleFrequency(Setting.ViewMemento.AngleFrequency);
            AngleFrequency.ValueChanged += (s, e) => Setting.ViewMemento.AngleFrequency = e.NewValue;

            // History Limit
            HistoryLimitSize = new HistoryLimitSize(History.LimitSize);
            HistoryLimitSize.ValueChanged += (s, e) => History.LimitSize = e.NewValue;

            HistoryLimitSpan = new HistoryLimitSpan(History.LimitSpan);
            HistoryLimitSpan.ValueChanged += (s, e) => History.LimitSpan = e.NewValue;

            // SlideShow Interval
            SlideShowInterval = new SlideShowInterval(Setting.BookHubMemento.SlideShowInterval);
            SlideShowInterval.ValueChanged += (s, e) => Setting.BookHubMemento.SlideShowInterval = e.NewValue;
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

            foreach (var element in _preference.Document.PropertyMembers.OrderBy(e => e.Path))
            {
                if (element.Path.StartsWith("_")) continue;

                var item = new PreferenceParam()
                {
                    Source = element,
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
        private void EditPreference(PropertyMemberElement param, bool isSimple)
        {
            if (isSimple && param.GetValueType() == typeof(bool))
            {
                param.SetValue(!(bool)param.GetValue());
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
                    _preference.Validate();
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
            RaisePropertyChanged(nameof(INPluginList));
            this.INPluginListView.Items.Refresh();

            AMPluginList = new ObservableCollection<Susie.SusiePlugin>(ModelContext.Susie?.AMPlgunList);
            RaisePropertyChanged(nameof(AMPluginList));
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
        private RelayCommand _parameterSettingCommand;
        public RelayCommand ParameterSettingCommand
        {
            get { return _parameterSettingCommand = _parameterSettingCommand ?? new RelayCommand(ParameterSettingCommand_Executed, ParameterSettingCommand_CanExecute); }
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



        // 全コマンド初期化ボタン処理
        private void ResetGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommandResetWindow();
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                Setting.CommandMememto = dialog.CreateCommandMemento();
                UpdateCommandList();
                this.CommandListView.Items.Refresh();
            }
        }

        // 履歴クリアボタン処理
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            History.Items.Clear();
            RaisePropertyChanged(nameof(History));
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
                // コンテキストメニュー確定
                this.ContextMenuSettingControl.Decide();

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
                Setting.PreferenceMemento = _preference.CreateMemento();

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
                if (_isDartySusieSetting)
                {
                    Setting.SusieMemento = OldSusieSetting;
                    ModelContext.SusieContext.Restore(Setting.SusieMemento);
                }
            }
        }

        // Susie設定のタブが選択された
        private void SusieSettingTab_Selected(object sender, RoutedEventArgs e)
        {
            _isDartySusieSetting = true;
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
            _preference.Reset();
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

                var context = PropertyDocument.Create(parameter);
                context.Name = command.Header;

                var dialog = new CommandParameterWindow(context, parameterDfault);
                dialog.Owner = this;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (dialog.ShowDialog() == true)
                {
                    command.ParameterJson = Utility.Json.Serialize(context.Source, context.Source.GetType());
                }
            }
        }

        private void CommandListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ParameterSettingCommand.RaiseCanExecuteChanged();
        }
    }


    /// <summary>
    /// ビュー回転スナップ値
    /// </summary>
    public class AngleFrequency : IndexDoubleValue
    {
        private static List<double> _values = new List<double>
        {
            0, 5, 10, 15, 20, 30, 45, 60, 90
        };

        //
        public AngleFrequency(double value) : base(_values)
        {
            Value = value;
        }

        //
        public override string ValueString => Value == 0 ? "無段階" : $"{Value}度";
    }


    /// <summary>
    /// 履歴サイズテーブル
    /// </summary>
    public class HistoryLimitSize : IndexIntValue
    {
        private static List<int> _values = new List<int>
        {
            0, 1, 10, 20, 50, 100, 200, 500, 1000, -1
        };

        //
        public HistoryLimitSize(int value) : base(_values)
        {
            Value = value;
        }

        //
        public override string ValueString => Value == -1 ? "制限なし" : Value.ToString();
    }

    /// <summary>
    /// 履歴期限テーブル
    /// </summary>
    public class HistoryLimitSpan : IndexTimeSpanValue
    {
        private static List<TimeSpan> _values = new List<TimeSpan>() {
                TimeSpan.FromDays(1),
                TimeSpan.FromDays(2),
                TimeSpan.FromDays(3),
                TimeSpan.FromDays(7),
                TimeSpan.FromDays(15),
                TimeSpan.FromDays(30),
                TimeSpan.FromDays(100),
                default(TimeSpan),
            };

        //
        public HistoryLimitSpan(TimeSpan value) : base(_values)
        {
            Value = value;
        }

        //
        public override string ValueString => Value == default(TimeSpan) ? "制限なし" : $"{Value.Days}日前まで";
    }


    /// <summary>
    /// スライドショー インターバルテーブル
    /// </summary>
    public class SlideShowInterval : IndexDoubleValue
    {
        private static List<double> _values = new List<double>
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 30, 45, 60, 90, 120, 180, 240, 300
        };

        //
        public SlideShowInterval(double value) : base(_values)
        {
            Value = value;
        }

        //
        public override string ValueString => $"{Value}秒";
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

    // Null判定コンバータ
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value == null) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
}
