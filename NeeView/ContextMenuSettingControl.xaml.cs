// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class ContextMenuSettingControl : UserControl
    {
        public static readonly RoutedCommand AddCommand = new RoutedCommand("AddCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand("RemoveCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand RenameCommand = new RoutedCommand("RenameCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand MoveUpCommand = new RoutedCommand("MoveUpCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand MoveDownCommand = new RoutedCommand("MoveDownCommand", typeof(ContextMenuSettingControl));



#if false
        public MainWindowVM.Memento ViewMemento
        {
            get { return (MainWindowVM.Memento)GetValue(ViewMementoProperty); }
            set { SetValue(ViewMementoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewMemento.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewMementoProperty =
            DependencyProperty.Register("ViewMemento", typeof(MainWindowVM.Memento), typeof(ContextMenuSettingControl), new PropertyMetadata(null, ViewMementoPropertyChanged));

        private static void ViewMementoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ContextMenuSettingControl;
            if (control != null)
            {
                control.Initialize();
            }
        }
#endif


        public ContextMenuSetting ContextMenuSetting
        {
            get { return (ContextMenuSetting)GetValue(ContextMenuSettingProperty); }
            set { SetValue(ContextMenuSettingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContextMenuSetting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextMenuSettingProperty =
            DependencyProperty.Register("ContextMenuSetting", typeof(ContextMenuSetting), typeof(ContextMenuSettingControl), new PropertyMetadata(null, ContextMenuSettingPropertyChanged));

        private static void ContextMenuSettingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ContextMenuSettingControl)?.Initialize();
        }


        //
        private ContextMenuSettingControlVM _VM;

        public ContextMenuSettingControl()
        {
            InitializeComponent();

            _VM = new ContextMenuSettingControlVM();
            this.Root.DataContext = _VM;


            this.CommandBindings.Add(new CommandBinding(AddCommand, Add_Exec));
            this.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, SelectedItem_CanExec));
            this.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Exec, Rename_CanExec));
            this.CommandBindings.Add(new CommandBinding(MoveUpCommand, MoveUp_Exec, SelectedItem_CanExec));
            this.CommandBindings.Add(new CommandBinding(MoveDownCommand, MoveDown_Exec, SelectedItem_CanExec));
        }

        private void Initialize()
        {
            if (this.ContextMenuSetting != null)
            {
                _VM.Initialize(ContextMenuSetting);
                this.SourceComboBox.SelectedIndex = 0;
            }
        }


        private void Add_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var element = this.SourceComboBox.SelectedItem as MenuTree;
            if (element == null) return;

            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;

            _VM.AddNode(MenuTree.Create(element), node);
        }

        //
        private void SelectedItem_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            e.CanExecute = node != null && node.MenuElementType != MenuElementType.None;
        }

        //
        private void Remove_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            if (node == null) return;

            _VM.RemoveNode(node);
        }

        //
        private void Rename_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            e.CanExecute = node != null && node.MenuElementType != MenuElementType.None && node.MenuElementType != MenuElementType.Separator;
        }

        //
        private void Rename_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            if (node == null) return;

            var param = new RenameWindowParam() { Text = node.Label, DefaultText = node.DefaultLabel };

            var dialog = new RenameWindow(param);
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                node.Label = param.Text;
            }
        }

        //
        private void MoveUp_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            if (node == null) return;

            _VM.MoveUp(node);
        }

        //
        private void MoveDown_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            var node = this.ContextMenuTreeView.SelectedItem as MenuTree;
            if (node == null) return;

            _VM.MoveDown(node);
        }

        public void Decide()
        {
            _VM.Decide();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _VM.Reset();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class ContextMenuSettingControlVM : BindableBase
    {
        #region Property: Root
        private MenuTree _root;
        public MenuTree Root
        {
            get { return _root; }
            set { _root = value; RaisePropertyChanged(); }
        }
        #endregion

        public List<MenuTree> SourceElementList { get; set; }



        private ContextMenuSetting _contextMenuSetting;

        public ContextMenuSettingControlVM()
        {
            if (CommandTable.Current == null) return;

            var list = Enum.GetValues(typeof(CommandType))
               .OfType<CommandType>()
               .Where(e => !e.IsDisable())
               .GroupBy(e => CommandTable.Current[e].Group)
               .SelectMany(g => g)
               .Select(e => new MenuTree() { MenuElementType = MenuElementType.Command, Command = e })
               .ToList();

            list.Insert(0, new MenuTree() { MenuElementType = MenuElementType.Group });
            list.Insert(1, new MenuTree() { MenuElementType = MenuElementType.Separator });
            list.Insert(2, new MenuTree() { MenuElementType = MenuElementType.History });

            SourceElementList = list;
        }

        //
        public void Initialize(ContextMenuSetting contextMenuSetting)
        {
            _contextMenuSetting = contextMenuSetting;

            Root = _contextMenuSetting.SourceTree.Clone();

            // validate
            Root.MenuElementType = MenuElementType.Group;
            Root.Validate();
        }

        //
        public void Decide()
        {
            _contextMenuSetting.SourceTree = Root.IsEqual(MenuTree.CreateDefault()) ? null : Root;
        }

        //
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

            element.IsSelected = true;
            Root.Validate();
        }


        //
        public void RemoveNode(MenuTree target)
        {
            var parent = target.GetParent(Root);
            if (parent != null)
            {
                var next = target.GetNext(Root, false) ?? target.GetPrev(Root);

                parent.Children.Remove(target);
                parent.Validate();

                if (next != null) next.IsSelected = true;
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
