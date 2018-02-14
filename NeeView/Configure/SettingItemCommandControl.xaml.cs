using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            CommandCollection = new ObservableCollection<CommandParam>();
            UpdateCommandList();
        }



        // コマンド一覧用パラメータ
        public class CommandParam : BindableBase
        {
            public CommandElement Command { get; set; }

            public CommandType Key { get; set; }
            public string ShortCutNote { get; set; }
            public ObservableCollection<GestureElement> ShortCuts { get; set; } = new ObservableCollection<GestureElement>();
            public GestureElement MouseGestureElement { get; set; }
            public string TouchGestureNote { get; set; }
            public ObservableCollection<GestureElement> TouchGestures { get; set; } = new ObservableCollection<GestureElement>();
            public bool HasParameter { get; set; }
            public CommandType ParameterShareCommandType { get; set; }
            public bool IsShareParameter => ParameterShareCommandType != CommandType.None;
            public string ShareTips => $"「{ParameterShareCommandType.ToDispString()}」とパラメータ共有です";
        }

        // コマンド一覧
        public ObservableCollection<CommandParam> CommandCollection { get; set; }





        // 全コマンド初期化ボタン処理
        private void ResetGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommandResetWindow();
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                // TODO: 遅延反映
                CommandTable.Current.Restore(dialog.CreateCommandMemento());

                UpdateCommandList();
                this.CommandListView.Items.Refresh();
            }
        }


        // コマンド一覧 更新
        private void UpdateCommandList()
        {
            CommandCollection.Clear();
            foreach (var element in CommandTable.Current)
            {
                if (element.Key.IsDisable()) continue;

                var command = element.Value;

                var item = new CommandParam()
                {
                    Key = element.Key,
                    Command = command,
                };

                if (command.HasParameter)
                {
                    item.HasParameter = true;

                    var share = command.DefaultParameter as ShareCommandParameter;
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

                if (!string.IsNullOrEmpty(item.Command.ShortCutKey))
                {
                    var shortcuts = new ObservableCollection<GestureElement>();
                    foreach (var key in item.Command.ShortCutKey.Split(','))
                    {
                        var overlaps = CommandCollection
                            .Where(e => !string.IsNullOrEmpty(e.Command.ShortCutKey) && e.Key != item.Key && e.Command.ShortCutKey.Split(',').Contains(key))
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
                if (!string.IsNullOrEmpty(item.Command.MouseGesture))
                {
                    var overlaps = CommandCollection
                        .Where(e => e.Key != item.Key && e.Command.MouseGesture == item.Command.MouseGesture)
                        .Select(e => $"「{e.Key.ToDispString()}」")
                        .ToList();

                    var element = new GestureElement();
                    element.Gesture = item.Command.MouseGesture;
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

                if (!string.IsNullOrEmpty(item.Command.TouchGesture))
                {
                    var elements = new ObservableCollection<GestureElement>();
                    foreach (var key in item.Command.TouchGesture.Split(','))
                    {
                        var overlaps = CommandCollection
                            .Where(e => !string.IsNullOrEmpty(e.Command.TouchGesture) && e.Key != item.Key && e.Command.TouchGesture.Split(',').Contains(key))
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
            var dialog = new Configure.EditCommandWindow();
            dialog.Initialize(command.Key, Configure.EditCommandWindowTab.Parameter);
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true)
            {
                // TODO: any?
            }

            UpdateCommandList();
        }


        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as CommandParam;
            if (item == null)
            {
                return;
            }

            var dialog = new Configure.EditCommandWindow();
            dialog.Initialize(item.Key);
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true)
            {
                // TODO: any?
            }

            UpdateCommandList();
        }
    }
}
