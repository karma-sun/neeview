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

namespace NeeView.Setting
{
    // TODO: 整備
    public class GestureElement
    {
        public string Gesture { get; set; }
        public bool IsConflict { get; set; }
        public string Splitter { get; set; }
        public string Note { get; set; }
    }


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
            public string ShareTips => string.Format(Properties.Resources.ControlCommandListShare, ParameterShareCommandType.ToDispString());
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
                CommandTable.Current.Restore(dialog.CreateCommandMemento(), false);

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
                            .Select(e => e.Key.ToDispString())
                            .ToList();

                        if (overlaps.Count > 0)
                        {
                            if (item.ShortCutNote != null) item.ShortCutNote += "\n";
                            item.ShortCutNote += string.Format(Properties.Resources.NotifyConflictWith, key, ResourceService.Join(overlaps));
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
                    item.ShortCuts = new ObservableCollection<GestureElement>() { new GestureElement() };
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
                        .Select(e => e.Key.ToDispString())
                        .ToList();

                    var element = new GestureElement();
                    element.Gesture = item.Command.MouseGesture;
                    element.IsConflict = overlaps.Count > 0;
                    if (overlaps.Count > 0)
                    {
                        element.Note = string.Format(Properties.Resources.NotifyConflict, ResourceService.Join(overlaps));
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
                            .Select(e => e.Key.ToDispString())
                            .ToList();

                        if (overlaps.Count > 0)
                        {
                            if (item.TouchGestureNote != null) item.TouchGestureNote += "\n";
                            item.TouchGestureNote += string.Format(Properties.Resources.NotifyConflictWith, key, ResourceService.Join(overlaps));
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
                    item.TouchGestures = new ObservableCollection<GestureElement>() { new GestureElement() };
                }
            }
        }



        //
        private void EditCommandParameterButton_Clock(object sender, RoutedEventArgs e)
        {
            var command = (sender as Button)?.Tag as CommandParam;
            EditCommand(command.Key, EditCommandWindowTab.Parameter);
        }

        //
        private void EditCommand(CommandType key, EditCommandWindowTab tab)
        {
            var dialog = new EditCommandWindow();
            dialog.Initialize(key, tab);
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true)
            {
                UpdateCommandList();
            }
        }

        //
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as CommandParam;
            if (item == null)
            {
                return;
            }

            // カーソル位置から初期TABを選択
            var listViewItem = (ListViewItem)sender;
            var hitResult = VisualTreeHelper.HitTest(listViewItem, e.GetPosition(listViewItem));
            var tag = GetAncestorTag(hitResult?.VisualHit, "@");
            EditCommandWindowTab tab;
            switch (tag)
            {
                default:
                    tab = EditCommandWindowTab.Default;
                    break;
                case "@shortcut":
                    tab = EditCommandWindowTab.InputGesture;
                    break;
                case "@gesture":
                    tab = EditCommandWindowTab.MouseGesture;
                    break;
                case "@touch":
                    tab = EditCommandWindowTab.InputTouch;
                    break;
            }

            EditCommand(item.Key, tab);
        }

        private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as CommandParam;
            if (item == null)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                EditCommand(item.Key, EditCommandWindowTab.Default);
                e.Handled = true;
            }
        }

        /// <summary>
        /// ビジュアルツリーの親に定義されている文字列タグを取得。
        /// </summary>
        /// <param name="obj">検索開始要素</param>
        /// <param name="prefix">文字列のプレフィックス</param>
        /// <returns></returns>
        private string GetAncestorTag(DependencyObject obj, string prefix)
        {
            while (obj != null)
            {
                var tag = (obj as FrameworkElement).Tag as string;
                if (tag != null && tag.StartsWith(prefix)) return tag;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

    }
}
