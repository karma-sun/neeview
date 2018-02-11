using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// SettingItemCommandControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemCommandControl : UserControl
    {
        public SettingItemCommandControl()
        {
            InitializeComponent();
            this.Root.DataContext = this;

            // 初期化
            _commandMemento = CommandTable.Current.CreateMemento();
            CommandCollection = new ObservableCollection<CommandParam>();
            UpdateCommandList();
        }

        // TODO: 反映をメインウィンドウアクティブ化したタイミングにする
        // ##
        private CommandTable.Memento _commandMemento;


        #region Command

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



        // ショートカットキー設定ボタン処理
        private void ShortCutSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (CommandParam)this.CommandListView.SelectedValue;


            var gestures = CommandCollection.ToDictionary(i => i.Key, i => i.ShortCut);
            var key = value.Key;
            var dialog = new InputGestureSettingWindow(gestures, key);

            //var dialog = new InputGestureSettingWindow (CommandCollection, value);
            dialog.Owner = Window.GetWindow(this);
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

                //// コマンド反映
                RestoreCommand();
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
            dialog.Owner = Window.GetWindow(this);
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

                //// コマンド反映
                RestoreCommand();
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
            dialog.Owner = Window.GetWindow(this);
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

                //// コマンド反映
                RestoreCommand();
            }
        }



        // 全コマンド初期化ボタン処理
        private void ResetGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommandResetWindow();
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                ////Setting.CommandMememto = dialog.CreateCommandMemento();
                _commandMemento = dialog.CreateCommandMemento();
                UpdateCommandList();
                this.CommandListView.Items.Refresh();

                //// コマンド反映
                RestoreCommand();
            }
        }

        /// <summary>
        /// コマンド反映 ##
        /// </summary>
        private void RestoreCommand()
        {
            //// コマンド設定反映
            foreach (var command in CommandCollection)
            {
                _commandMemento[command.Key].ShortCutKey = command.ShortCut;
                _commandMemento[command.Key].MouseGesture = command.MouseGesture;
                _commandMemento[command.Key].TouchGesture = command.TouchGesture;
                _commandMemento[command.Key].IsShowMessage = command.IsShowMessage;
                _commandMemento[command.Key].Parameter = command.ParameterJson;
            }

            CommandTable.Current.Restore(_commandMemento);
        }

        // コマンド一覧 更新
        private void UpdateCommandList()
        {
            CommandCollection.Clear();
            foreach (var element in CommandTable.Current)
            {
                if (element.Key.IsDisable()) continue;

                ////var memento = Setting.CommandMememto[element.Key];
                var memento = _commandMemento[element.Key];

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
                dialog.Owner = Window.GetWindow(this);
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

        #endregion
    }
}
