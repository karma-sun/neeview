﻿using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class SettingItemCommandControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        // コマンド項目
        public class CommandItem : BindableBase
        {
            public string Key { get; set; }
            public CommandElement Command { get; set; }
            public string ShortCutNote { get; set; }
            public ObservableCollection<GestureElement> ShortCuts { get; set; } = new ObservableCollection<GestureElement>();
            public GestureElement MouseGestureElement { get; set; }
            public string TouchGestureNote { get; set; }
            public ObservableCollection<GestureElement> TouchGestures { get; set; } = new ObservableCollection<GestureElement>();
            public bool HasParameter { get; set; }
            public string ParameterShareCommandName { get; set; }
            public bool IsShareParameter => !string.IsNullOrEmpty(ParameterShareCommandName);
            public string ShareTips => ParameterShareCommandName != null ? string.Format(Properties.Resources.CommandListItem_Message_ShareParameter, CommandTable.Current.GetElement(ParameterShareCommandName).Text) : null;
        }

        private int _commandTableChangeCount;
        private ObservableCollection<CommandItem> _commandItems;
        private string _searchKeyword = "";
        private List<string> _searchKeywordTokens = new List<string>();


        public SettingItemCommandControl()
        {
            InitializeComponent();
            this.Root.DataContext = this;

            // 初期化
            _commandItems = new ObservableCollection<CommandItem>();
            UpdateCommandList();

            ItemsViewSource = new CollectionViewSource() { Source = _commandItems };
            ItemsViewSource.Filter += ItemsViewSource_Filter;

            this.Loaded += SettingItemCommandControl_Loaded;
        }


        public CollectionViewSource ItemsViewSource { get; set; }

        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    Search();
                }
            }
        }


        private void ItemsViewSource_Filter(object sender, FilterEventArgs eventArgs)
        {
            if (_searchKeywordTokens.Count <= 0)
            {
                eventArgs.Accepted = true;
            }
            else
            {
                var item = (CommandItem)eventArgs.Item;
                var text = NeeLaboratory.IO.Search.Node.ToNormalisedWord(item.Command.GetSearchText(), true);
                eventArgs.Accepted = _searchKeywordTokens.All(e => text.IndexOf(e) >= 0);
            }
        }

        private void SettingItemCommandControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_commandTableChangeCount != CommandTable.Current.ChangeCount)
            {
                UpdateCommandList();
            }

            this.SearchKeyword = "";
            Search();
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
                CommandTable.Current.ClearScriptCommand();
                CommandTable.Current.RestoreCommandCollection(dialog.CreateCommandMemento());

                UpdateCommandList();
            }
        }

        // コマンド一覧 更新
        private void UpdateCommandList()
        {
            _commandTableChangeCount = CommandTable.Current.ChangeCount;

            _commandItems.Clear();
            foreach (var element in CommandTable.Current.Elements.OrderBy(e => e.Value.Order))
            {
                var command = element.Value;

                var item = new CommandItem()
                {
                    Key = element.Key,
                    Command = command,
                };

                if (command.ParameterSource != null)
                {
                    item.HasParameter = true;

                    if (command.Share != null)
                    {
                        item.ParameterShareCommandName = command.Share.Name;
                    }
                }

                _commandItems.Add(item);
            }

            UpdateCommandListShortCut();
            UpdateCommandListMouseGesture();
            UpdateCommandListTouchGesture();

            this.CommandListView.Items.Refresh();
        }

        // コマンド一覧 ショートカット更新
        private void UpdateCommandListShortCut()
        {
            foreach (var item in _commandItems)
            {
                item.ShortCutNote = null;

                if (!string.IsNullOrEmpty(item.Command.ShortCutKey))
                {
                    var shortcuts = new ObservableCollection<GestureElement>();
                    foreach (var key in item.Command.ShortCutKey.Split(','))
                    {
                        var overlaps = _commandItems
                            .Where(e => !string.IsNullOrEmpty(e.Command.ShortCutKey) && e.Key != item.Key && e.Command.ShortCutKey.Split(',').Contains(key))
                            .Select(e => CommandTable.Current.GetElement(e.Key).Text)
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
            foreach (var item in _commandItems)
            {
                if (!string.IsNullOrEmpty(item.Command.MouseGesture))
                {
                    var overlaps = _commandItems
                        .Where(e => e.Key != item.Key && e.Command.MouseGesture == item.Command.MouseGesture)
                        .Select(e => CommandTable.Current.GetElement(e.Key).Text)
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
            foreach (var item in _commandItems)
            {
                item.TouchGestureNote = null;

                if (!string.IsNullOrEmpty(item.Command.TouchGesture))
                {
                    var elements = new ObservableCollection<GestureElement>();
                    foreach (var key in item.Command.TouchGesture.Split(','))
                    {
                        var overlaps = _commandItems
                            .Where(e => !string.IsNullOrEmpty(e.Command.TouchGesture) && e.Key != item.Key && e.Command.TouchGesture.Split(',').Contains(key))
                            .Select(e => CommandTable.Current.GetElement(e.Key).Text)
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


        private void EditCommandParameterButton_Clock(object sender, RoutedEventArgs e)
        {
            var command = (sender as Button)?.Tag as CommandItem;
            EditCommand(command.Key, EditCommandWindowTab.Parameter);
        }

        private void EditCommand(string key, EditCommandWindowTab tab)
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

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListViewItem)sender).Content as CommandItem;
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
            var item = ((ListViewItem)sender).Content as CommandItem;
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


        private void Search()
        {
            _searchKeywordTokens = this.SearchKeyword.Split(' ')
                .Select(e => NeeLaboratory.IO.Search.Node.ToNormalisedWord(e.Trim(), true))
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();

            ItemsViewSource.View.Refresh();
        }
    }
}
