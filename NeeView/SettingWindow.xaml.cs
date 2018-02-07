// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
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
using NeeView.Windows.Property;
using NeeLaboratory.Windows.Input;
using NeeView.Windows;
using NeeLaboratory.ComponentModel;
using NeeView.Data;

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


        // ドラッグ一覧専用パラメータ
        public class DragActionParam : BindableBase
        {
            public DragActionType Key { get; set; }
            public string Header { get; set; }
            public bool IsLocked { get; set; }

            /// <summary>
            /// DragAction property.
            /// </summary>
            private string _dragAction;
            public string DragAction
            {
                get { return _dragAction; }
                set { if (_dragAction != value) { _dragAction = value; RaisePropertyChanged(); } }
            }

            public string Tips { get; set; }
        }


        // コマンド一覧
        public ObservableCollection<DragActionParam> DragActionCollection { get; set; }


        // コマンド一覧用パラメータ
        public class CommandParam : BindableBase
        {
            public CommandType Key { get; set; }
            public string Group { get; set; }
            public string Header { get; set; }

            public string ShortCut { get; set; }
            public string ShortCutNote { get; set; }
            public ObservableCollection<GestureElement> ShortCuts { get; set; } = new ObservableCollection<GestureElement>();

            public string MouseGesture { get; set; }
            public GestureElement MouseGestureElement { get; set; }

            public string TouchGesture { get; set; }
            public string TouchGestureNote { get; set; }
            public ObservableCollection<GestureElement> TouchGestures { get; set; } = new ObservableCollection<GestureElement>();

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
        private PropertyDocument _propertyDocument;


        // 詳細設定一覧用パラメータ
        public class PropertyParam
        {
            public PropertyMemberElement Source { get; set; }

            public string Key => Source.Path;
            public string Name => Source.Name;
            public string State => Source.HasCustomValue ? "ユーザ設定" : "初期設定値";
            public string TypeString => Source.GetValueTypeString();
            public string Value => Source.GetValueString();
            public string Tips => Source.Tips;
        }

        // 詳細設定一覧
        public ObservableCollection<PropertyParam> PropertyCollection { get; set; }


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
        public static Dictionary<PanelListItemStyle, string> FolderListItemStyleList { get; } = new Dictionary<PanelListItemStyle, string>
        {
            [PanelListItemStyle.Normal] = "テキスト表示",
            [PanelListItemStyle.Content] = "コンテンツ表示",
            [PanelListItemStyle.Banner] = "バナー表示",
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
            [SliderDirection.LeftToRight] = "▶ 左から右",
            [SliderDirection.RightToLeft] = "◀ 右から左",
            [SliderDirection.SyncBookReadDirection] = "本を開く方向に依存",
        };

        //
        public static Dictionary<SliderIndexLayout, string> SliderIndexLayoutList { get; } = new Dictionary<SliderIndexLayout, string>
        {
            [SliderIndexLayout.None] = "表示しない",
            [SliderIndexLayout.Left] = "左",
            [SliderIndexLayout.Right] = "右",
        };

        #region Property: ExternalApplicationParam
        public string ExternalApplicationParam
        {
            get { return Setting.Memento.BookOperation.ExternalApplication.Parameter; }
            set
            {
                var validValue = ExternalApplication.ValidateApplicationParam(value);
                Setting.Memento.BookOperation.ExternalApplication.Parameter = validValue;
                RaisePropertyChanged();
            }
        }
        #endregion


        public static Dictionary<ExternalProgramType, string> ExternalProgramTypeList { get; } = new Dictionary<ExternalProgramType, string>
        {
            [ExternalProgramType.Normal] = "外部プログラム",
            [ExternalProgramType.Protocol] = "プロトコル起動",
        };

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
            [PageEndAction.NextFolder] = "次のブックに移動",
            [PageEndAction.Loop] = "ループする"
        };

        //
        public static Dictionary<PreLoadMode, string> PreLoadModeList { get; } = new Dictionary<PreLoadMode, string>
        {
            [PreLoadMode.None] = "しない",
            [PreLoadMode.AutoPreLoad] = "自動",
            [PreLoadMode.PreLoad] = "先読みする",
            [PreLoadMode.PreLoadNoUnload] = "先読みする(開放なし)"
        };

        //
        public static Dictionary<PageMode, string> PageModeList => PageModeExtension.PageModeList;

        //
        public static Dictionary<PageReadOrder, string> PageReadOrderList => PageReadOrderExtension.PageReadOrderList;

        //
        public static Dictionary<PageSortMode, string> PageSortModeList => PageSortModeExtension.PageSortModeList;

        //
        public static Dictionary<TouchAction, string> TouchActionList => TouchActionExtensions.TouchActionList;

        //
        public static Dictionary<TouchAction, string> TouchActionLimitedList => TouchActionExtensions.TouchActionLimitedList;


        // ビュー回転のスナップ値
        public AngleFrequency AngleFrequency { get; set; }

        // 履歴制限
        public HistoryLimitSize HistoryLimitSize { get; set; }
        public HistoryLimitSpan HistoryLimitSpan { get; set; }

        // スライドショー間隔
        public SlideShowInterval SlideShowInterval { get; set; }

        //
        public string CommandSwapTooltip { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="setting"></param>
        public SettingWindow(Setting setting, BookHistory.Memento history)
        {
            InitializeComponent();

            // Susieが機能しない場合はSusieタブを使用禁止にする
            if (!SusieContext.Current.IsSupportedSusie)
            {
                this.SusieSettingTab.IsEnabled = false;
                this.SusieSettingTab.Visibility = Visibility.Collapsed;
            }

            this.RemoveAllDataButton.Visibility = (Config.Current.IsUseLocalApplicationDataFolder && !Config.Current.IsAppxPackage) ? Visibility.Visible : Visibility.Collapsed;

            Setting = setting;
            History = history;
            OldSusieSetting = setting.SusieMemento.Clone();
            _isDartySusieSetting = false;

            // ドラッグアクション一覧作成
            DragActionCollection = new ObservableCollection<DragActionParam>();
            UpdateDragActionList();

            // コマンド一覧作成
            CommandCollection = new ObservableCollection<CommandParam>();
            UpdateCommandList();

            // コマンド入れ替え説明文生成
            UpdateCommandSwapTooltip();

            // 詳細設定一覧作成
            _propertyDocument = new PropertyDocument(new object[]
                {
                    Setting.App,
                    Setting.Memento.JobEngine,
                    Setting.Memento.FileIOProfile,

                    Setting.Memento.PictureProfile,
                    Setting.Memento.ImageFilter,

                    Setting.Memento.ArchiverManager,
                    Setting.Memento.SevenZipArchiverProfile,
                    Setting.Memento.PdfArchiverProfile,
                    Setting.Memento.ThumbnailProfile,
                    Setting.Memento.MainWindowModel,
                    Setting.Memento.FolderList,
                    Setting.Memento.SidePanelProfile,
                    Setting.Memento.SidePanel,
                    Setting.Memento.ThumbnailList,
                    Setting.Memento.MenuBar,
                    Setting.Memento.BookProfile,
                    Setting.Memento.BookHub,

                    Setting.Memento.MouseInput.Normal,
                    Setting.Memento.MouseInput.Gesture,
                    Setting.Memento.MouseInput.Loupe,

                    Setting.Memento.TouchInput.Gesture,
                    Setting.Memento.TouchInput.Drag,
                    Setting.Memento.TouchInput.Drag.Manipulation,
                });
            PropertyCollection = new ObservableCollection<PropertyParam>();
            UpdatePropertyList();

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
            AngleFrequency = new AngleFrequency(Setting.Memento.DragTransform.AngleFrequency);
            AngleFrequency.ValueChanged += (s, e) => Setting.Memento.DragTransform.AngleFrequency = e.NewValue;

            // History Limit
            HistoryLimitSize = new HistoryLimitSize(History.LimitSize);
            HistoryLimitSize.ValueChanged += (s, e) => History.LimitSize = e.NewValue;

            HistoryLimitSpan = new HistoryLimitSpan(History.LimitSpan);
            HistoryLimitSpan.ValueChanged += (s, e) => History.LimitSpan = e.NewValue;

            // SlideShow Interval
            SlideShowInterval = new SlideShowInterval(Setting.Memento.SlideShow.SlideShowInterval);
            SlideShowInterval.ValueChanged += (s, e) => Setting.Memento.SlideShow.SlideShowInterval = e.NewValue;
        }

        //
        private void UpdateCommandSwapTooltip()
        {
            var commandTable = CommandTable.Current;

            // ペア収集
            var pairs = commandTable
                .Where(e => e.Value.PairPartner != CommandType.None)
                .ToDictionary(e => e.Key, e => e.Value.PairPartner);

            while (true)
            {
                var element = pairs.Last();
                if (!pairs.ContainsKey(element.Value)) break;
                pairs.Remove(element.Key);
            }

            var text = "本を開く方向「左開き」の場合に以下のコマンド操作を入れ替えます。\n\n"
                + string.Join("\n", pairs.Select(e => $"- {commandTable[e.Key].Text} / {commandTable[e.Value].Text}"));

            this.CommandSwapTooltip = text;
        }

        //
        private void UpdateDragActionList()
        {
            DragActionCollection.Clear();
            foreach (var element in DragActionTable.Current)
            {
                var memento = Setting.DragActionMemento[element.Key];

                var item = new DragActionParam()
                {
                    Key = element.Key,
                    Header = element.Key.ToLabel(),
                    IsLocked = element.Value.IsLocked,
                    DragAction = memento.Key,
                    Tips = element.Key.ToTips(),
                };

                DragActionCollection.Add(item);
            }

            this.DragActionListView.Items.Refresh();
        }

        // コマンド一覧 更新
        private void UpdateCommandList()
        {
            CommandCollection.Clear();
            foreach (var element in CommandTable.Current)
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
                    TouchGesture = memento.TouchGesture,
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
            UpdateCommandListTouchGesture();

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


        // コマンド一覧 タッチ更新
        private void UpdateCommandListTouchGesture()
        {
            foreach (var item in CommandCollection)
            {
                item.TouchGestureNote = null;

                if (!string.IsNullOrEmpty(item.TouchGesture))
                {
                    var elements = new ObservableCollection<GestureElement>();
                    foreach (var key in item.TouchGesture.Split(','))
                    {
                        var overlaps = CommandCollection
                            .Where(e => !string.IsNullOrEmpty(e.TouchGesture) && e.Key != item.Key && e.TouchGesture.Split(',').Contains(key))
                            .Select(e => $"「{e.Key.ToDispString()}」")
                            .ToList();

                        if (overlaps.Count > 0)
                        {
                            if (item.TouchGestureNote != null) item.TouchGestureNote += "\n";
                            item.TouchGestureNote += $"{key} は {string.Join("", overlaps)} と競合しています";
                        }

                        var element = new GestureElement();
                        element.Gesture = key;
                        element.IsConflict = overlaps.Count > 0;
                        element.Splitter = ",";

                        elements.Add(element);
                    }

                    if (elements.Count > 0)
                    {
                        elements.Last().Splitter = null;
                    }

                    item.TouchGestures = elements;
                }
                else
                {
                    item.TouchGestures = new ObservableCollection<GestureElement>();
                }
            }
        }

        // 詳細一覧 更新
        private void UpdatePropertyList()
        {
            PropertyCollection.Clear();

            foreach (var element in _propertyDocument.PropertyMembers.Where(e => !e.IsObsolete))
            {
                if (element.Path.StartsWith("_")) continue;

                var item = new PropertyParam()
                {
                    Source = element,
                };
                PropertyCollection.Add(item);
            }
        }

        //
        private void PropertyListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // sender がダブルクリックされた項目
            ListViewItem targetItem = (ListViewItem)sender;

            // データバインディングを使っているなら、
            // DataContext からデータを取得できる
            PropertyParam p = (PropertyParam)targetItem.DataContext;
            EnditProperty(p.Source, true);
        }

        //
        private void EnditProperty(PropertyMemberElement param, bool isSimple)
        {
            if (isSimple && param.GetValueType() == typeof(bool))
            {
                param.SetValue(!(bool)param.GetValue());
                this.PropertyListView.Items.Refresh();
            }
            else
            {
                var dialog = new PropertyEditWindow(param);
                dialog.Owner = this;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    this.PropertyListView.Items.Refresh();
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
            if (!SusieContext.Current.IsSupportedSusie) return;

            // プラグインリスト書き戻し
            if (SusieContext.Current.Susie != null)
            {
                SusieContext.Current.Susie.AMPlgunList = AMPluginList.ToList();
                SusieContext.Current.Susie.INPlgunList = INPluginList.ToList();
            }

            // 現在のSusieプラグイン情報保存
            Setting.SusieMemento.SusiePluginPath = path;
            Setting.SusieMemento.SpiFiles = SusieContext.Memento.CreateSpiFiles(SusieContext.Current.Susie);

            SusieContext.Current.Restore(Setting.SusieMemento);
            UpdateSusiePluginList();
        }

        // Susieプラグイン一覧 更新
        public void UpdateSusiePluginList()
        {
            if (!SusieContext.Current.IsSupportedSusie) return;

            INPluginList = new ObservableCollection<Susie.SusiePlugin>(SusieContext.Current.Susie?.INPlgunList);
            RaisePropertyChanged(nameof(INPluginList));
            this.INPluginListView.Items.Refresh();

            AMPluginList = new ObservableCollection<Susie.SusiePlugin>(SusieContext.Current.Susie?.AMPlgunList);
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


        /// <summary>
        /// タッチ設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TouchGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (CommandParam)this.CommandListView.SelectedValue;

            var context = new InputTouchSettingContext();
            context.Command = value.Key;
            context.Gestures = CommandCollection.ToDictionary(i => i.Key, i => i.TouchGesture);

            var dialog = new InputTouchSettingWindow(context);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                foreach (var item in CommandCollection)
                {
                    item.TouchGesture = context.Gestures[item.Key];
                }

                UpdateCommandListTouchGesture();
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

            var dialog = new MessageDialog("", "履歴を削除しました");
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        // プラグインリスト：ドロップ受付判定
        private void PluginListView_PreviewDragOver(object sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e, "SusiePlugin");
        }

        // プラグインリスト：ドロップ
        private void PluginListView_Drop(object sender, DragEventArgs e)
        {
            var list = (sender as ListBox).Tag as ObservableCollection<Susie.SusiePlugin>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop<Susie.SusiePlugin>(sender, e, "SusiePlugin", list);
            }
        }

        // 設定画面終了処理
        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.DialogResult == true)
            {
                // コンテキストメニュー確定
                this.ContextMenuSettingControl.Decide();

                // ドラッグ設定反映
                foreach (var dragAction in DragActionCollection)
                {
                    Setting.DragActionMemento[dragAction.Key].Key = dragAction.DragAction;
                }

                // コマンド設定反映
                foreach (var command in CommandCollection)
                {
                    Setting.CommandMememto[command.Key].ShortCutKey = command.ShortCut;
                    Setting.CommandMememto[command.Key].MouseGesture = command.MouseGesture;
                    Setting.CommandMememto[command.Key].TouchGesture = command.TouchGesture;
                    Setting.CommandMememto[command.Key].IsShowMessage = command.IsShowMessage;
                    Setting.CommandMememto[command.Key].Parameter = command.ParameterJson;
                }

                if (SusieContext.Current.IsSupportedSusie)
                {
                    // プラグインリスト書き戻し
                    if (SusieContext.Current.Susie != null)
                    {
                        SusieContext.Current.Susie.AMPlgunList = AMPluginList.ToList();
                        SusieContext.Current.Susie.INPlgunList = INPluginList.ToList();
                    }

                    // Susie プラグインリスト保存
                    Setting.SusieMemento.SpiFiles = SusieContext.Memento.CreateSpiFiles(SusieContext.Current.Susie);
                }
            }
            else
            {
                // Susie設定を元に戻す
                if (_isDartySusieSetting)
                {
                    Setting.SusieMemento = OldSusieSetting;
                    SusieContext.Current.Restore(Setting.SusieMemento);
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
        private void PropertyEditButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (PropertyParam)this.PropertyListView.SelectedValue;
            EnditProperty(value.Source, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetAllPropertyButton_Click(object sender, RoutedEventArgs e)
        {
            _propertyDocument.Reset();
            this.PropertyListView.Items.Refresh();
        }

        /// <summary>
        /// Remove Cahe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveCache_Click(object sender, RoutedEventArgs e)
        {
            ThumbnailCache.Current.Remove();

            var dialog = new MessageDialog("", "キャッシュを削除しました");
            dialog.Owner = this;
            dialog.ShowDialog();
        }


        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveAllData_Click(object sender, RoutedEventArgs e)
        {
            Config.Current.RemoveApplicationData();
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
                var source = CommandTable.Current[command.Key];
                var parameterDfault = source.DefaultParameter;

                var parameter = command.ParameterJson != null
                    ? (CommandParameter)Json.Deserialize(command.ParameterJson, source.DefaultParameter.GetType())
                    : parameterDfault.Clone();

                var context = new PropertyDocument(parameter);
                context.Name = command.Header;

                var dialog = new CommandParameterWindow(context, parameterDfault);
                dialog.Owner = this;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (dialog.ShowDialog() == true)
                {
                    command.ParameterJson = Json.Serialize(context.Source, context.Source.GetType());
                }
            }
        }

        private void CommandListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ParameterSettingCommand.RaiseCanExecuteChanged();
        }

        //
        private void DragActionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // nop.
        }

        //
        private void DragActionListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem targetItem = (ListViewItem)sender;

            var value = (DragActionParam)targetItem.DataContext;
            OpenDragActionSettingDialog(value);
        }

        //
        private void DragActionSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (DragActionParam)this.DragActionListView.SelectedValue;
            OpenDragActionSettingDialog(value);
        }

        //
        private void OpenDragActionSettingDialog(DragActionParam value)
        {
            if (value.IsLocked)
            {
                var dlg = new MessageDialog("", "この操作は変更できません");
                dlg.Owner = this;
                dlg.ShowDialog();
                return;
            }

            var context = new MouseDragSettingContext();
            context.Command = value.Key;
            context.Gestures = DragActionCollection.ToDictionary(i => i.Key, i => i.DragAction);

            var dialog = new MouseDragSettingWindow(context);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                foreach (var item in DragActionCollection)
                {
                    item.DragAction = context.Gestures[item.Key];
                }

                this.CommandListView.Items.Refresh();
            }
        }

        //
        private void ResetDragActionSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog($"すべてのドラッグ操作を初期化します。よろしいですか？", "ドラッグ操作を初期化します");
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);
            dialog.Owner = this;
            var answer = dialog.ShowDialog();

            if (answer == UICommands.Yes)
            {
                Setting.DragActionMemento = DragActionTable.CreateDefaultMemento();
                UpdateDragActionList();
                this.DragActionListView.Items.Refresh();
            }
        }

        /// <summary>
        /// カスタム背景設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditCustomBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BackgroundSettingWindow(Setting.Memento.ContentCanvasBrush.CustomBackground.Clone());
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                Setting.Memento.ContentCanvasBrush.CustomBackground = dialog.Result;
            }
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
            IsValueSyncIndex = false;
            Value = value;
        }

        //
        public override string ValueString => $"{Value}秒";
    }

}
