// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
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

namespace NeeView
{
    /// <summary>
    /// ContextMenuSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ContextMenuSettingWindow : Window
    {
        ContextMenuSettingWindowVM _VM;

        public ContextMenuSettingWindow(MainWindowVM.Memento vmemento)
        {
            InitializeComponent();

            _VM = new ContextMenuSettingWindowVM(vmemento);
            this.DataContext = _VM;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            _VM.Decide();
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var element = this.SourceComboBox.SelectedItem as MenuTree;
            if (element == null) return;

            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;

            _VM.AddNode(MenuTree.Create(element), node);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            if (node == null) return;

            _VM.RemoveNode(node);
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            if (node == null) return;

            var param = new RenameWindowParam() { Text = node.Label, DefaultText = node.DefaultLabel };

            var dialog = new RenameWindow(param);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                node.Label = param.Text;
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            if (node == null) return;

            _VM.MoveUp(node);
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            if (node == null) return;

            _VM.MoveDown(node);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _VM.Reset();
        }
    }

    public enum MenuElementType
    {
        None,
        Group,
        Separator,
        Command
    }

    /// <summary>
    /// 
    /// </summary>
    public class ContextMenuSettingWindowVM : INotifyPropertyChanged
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

        #region Property: Root
        private MenuTree _Root;
        public MenuTree Root
        {
            get { return _Root; }
            set { _Root = value; OnPropertyChanged(); }
        }
        #endregion

        public List<MenuTree> SourceElementList { get; set; }



        private MainWindowVM.Memento _ViewMemento;

        public ContextMenuSettingWindowVM(MainWindowVM.Memento vmemento)
        {
            _ViewMemento = vmemento;

            var list = new List<MenuTree>();
            list.Add(new MenuTree() { MenuElementType = MenuElementType.Group });
            list.Add(new MenuTree() { MenuElementType = MenuElementType.Separator });
            foreach (CommandType command in Enum.GetValues(typeof(CommandType)))
            {
                if (command.IsDisable()) continue;
                list.Add(new MenuTree() { MenuElementType = MenuElementType.Command, Command = command });
            }
            SourceElementList = list;

            /*
            var collection = new MenuElement();
            collection.Children = new ObservableCollection<MenuElement>();
            collection.Children.Add(new MenuElement() { MenuElementType = MenuElementType.Command, Command = CommandType.NextOnePage });
            collection.Children.Add(new MenuElement() { MenuElementType = MenuElementType.Command, Command = CommandType.PrevOnePage });
            collection.Children.Add(new MenuElement() { MenuElementType = MenuElementType.Separator });
            collection.Children.Add(new MenuElement() { MenuElementType = MenuElementType.Command, Command = CommandType.ToggleFullScreen });
            collection.Children.Add(new MenuElement() { MenuElementType = MenuElementType.Separator });
            var group = new MenuElement() { MenuElementType = MenuElementType.Group, Name = "ファイル(_F)" };
            group.Children = new ObservableCollection<MenuElement>();
            group.Children.Add(new MenuElement() { MenuElementType = MenuElementType.Command, Command = CommandType.LoadAs });
            group.Children.Add(new MenuElement() { MenuElementType = MenuElementType.Command, Command = CommandType.Export });
            group.Children.Add(new MenuElement() { MenuElementType = MenuElementType.Separator });
            group.Children.Add(new MenuElement() { MenuElementType = MenuElementType.Command, Command = CommandType.OpenSettingWindow });
            collection.Children.Add(group);
            */

            /*
            if (_ViewMemento.ContextMenuSource == null)
            {
                _ViewMemento.ContextMenuSource = MenuElement.CreateDefault();
            }
            */

            Root = _ViewMemento.ContextMenuSource.Clone();

            // validate
            Root.MenuElementType = MenuElementType.Group;
            Root.Validate();
        }


        public void Decide()
        {
            _ViewMemento.ContextMenuSource = Root;
        }

        public void Reset()
        {
            Root = MenuTree.CreateDefault();
        }

        //
        private ObservableCollection<MenuTree> GetParentCollection(ObservableCollection<MenuTree> collection, MenuTree target)
        {
            if (collection.Contains(target)) return collection;

            foreach (var group in collection.Where(e => e.Children != null))
            {
                var parent = GetParentCollection(group.Children, target);
                if (parent != null) return parent;
            }

            return null;
        }

        //
        public void AddNode(MenuTree element, MenuTree target)
        {
            if (target == null)
            {
                Root.Children.Add(element);
            }
            else if (target.Children != null && target.IsExpanded)
            {
                target.Children.Insert(0, element);
            }
            else
            {
                var parent = target.GetParent(Root);
                if (parent != null)
                {
                    int index = parent.Children.IndexOf(target);
                    parent.Children.Insert(index + 1, element);
                }
            }

            Root.Validate();
        }


        //
        public void RemoveNode(MenuTree target)
        {
            var parent = target.GetParent(Root);
            if (parent != null)
            {
                parent.Children.Remove(target);
                parent.Validate();
            }
        }

        //
        public void MoveUp(MenuTree target)
        {
            var targetParent = target.GetParent(Root);

            var prev = target.GetPrev(Root);
            if (prev != null && prev != Root)
            {
                var prevParent = prev.GetParent(Root);

                if (targetParent == prevParent)
                {
                    int index = targetParent.Children.IndexOf(target);
                    targetParent.Children.Move(index, index - 1);
                }
                else if (targetParent == prev)
                {
                    targetParent.Children.Remove(target);
                    int index = prevParent.Children.IndexOf(prev);
                    prevParent.Children.Insert(index, target);
                }
                else
                {
                    targetParent.Children.Remove(target);
                    int index = prevParent.Children.IndexOf(prev);
                    prevParent.Children.Insert(index + 1, target);
                }

                target.IsSelected = true;
                Root.Validate();
            }
        }


        public void MoveDown(MenuTree target)
        {
            var targetParent = target.GetParent(Root);

            var next = target.GetNext(Root);
            if (next != null && next != Root)
            {
                var nextParent = next.GetParent(Root);

                if (targetParent == nextParent)
                {
                    if (next.IsExpanded)
                    {
                        targetParent.Children.Remove(target);
                        next.Children.Insert(0, target);
                    }
                    else
                    {
                        int index = targetParent.Children.IndexOf(target);
                        targetParent.Children.Move(index, index + 1);
                    }
                }
                else
                {
                    targetParent.Children.Remove(target);
                    int index = nextParent.Children.IndexOf(next);
                    nextParent.Children.Insert(index, target);
                }

                target.IsSelected = true;
                Root.Validate();
            }
        }
    }
}
