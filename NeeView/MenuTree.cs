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
using System.Windows.Controls;

namespace NeeView
{

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class MenuTree : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion

        #region Property: IsExpanded
        private bool _IsExpanded;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set { _IsExpanded = value; OnPropertyChanged(); }
        }
        #endregion

        #region Property: IsSelected
        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value; OnPropertyChanged(); }
        }
        #endregion


        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public MenuElementType MenuElementType { get; set; }
        [DataMember]
        public CommandType Command { get; set; }
        [DataMember]
        public ObservableCollection<MenuTree> Children { get; set; }


        #region Property: Label
        public string Label
        {
            get { return Name ?? DefaultLabel; }
            set { Name = (value == DefaultLabel) ? null : value; OnPropertyChanged(); }
        }
        #endregion

        public string DefaultLabel
        {
            get
            {
                switch (MenuElementType)
                {
                    default:
                    case MenuElementType.None:
                        return "(なし)";
                    case MenuElementType.Command:
                        return Command.ToDispString();
                    case MenuElementType.Group:
                        return "《サブメニュー》";
                    case MenuElementType.Separator:
                        return "《セパレーター》";
                }
            }
        }

        public MenuTree Clone()
        {
            var clone = (MenuTree)this.MemberwiseClone();
            if (clone.Children != null)
            {
                clone.Children = new ObservableCollection<MenuTree>();
                foreach (var element in this.Children)
                {
                    clone.Children.Add(element.Clone());
                }
            }
            return clone;
        }


        public void Validate()
        {
            if (this.MenuElementType == MenuElementType.Group)
            {
                if (this.Children == null)
                {
                    this.Children = new ObservableCollection<MenuTree>();
                }
                var removes = new List<MenuTree>();
                foreach (var child in this.Children)
                {
                    if (this.Children.Count >= 2 && child.MenuElementType == MenuElementType.None)
                    {
                        removes.Add(child);
                    }
                    else
                    {
                        child.Validate();
                    }
                }
                foreach (var e in removes)
                {
                    this.Children.Remove(e);
                }
                if (this.Children.Count == 0)
                {
                    this.Children.Add(new MenuTree());
                }
            }
        }


        //
        public static MenuTree Create(MenuTree source)
        {
            var element = new MenuTree();
            element.MenuElementType = source.MenuElementType;
            element.Command = source.Command;
            if (element.MenuElementType == MenuElementType.Group)
            {
                element.Children = new ObservableCollection<MenuTree>();
                element.Children.Add(new MenuTree() { MenuElementType = MenuElementType.None });
            }
            return element;
        }


        //
        public MenuTree GetParent(MenuTree root)
        {
            if (root.Children == null) return null;
            if (root.Children.Contains(this)) return root;

            foreach (var group in root.Children.Where(e => e.Children != null))
            {
                var parent = this.GetParent(group);
                if (parent != null) return parent;
            }
            return null;
        }

        //
        public MenuTree GetNext(MenuTree root, bool isUseExpand = true)
        {
            var parent = this.GetParent(root);
            if (parent == null) return null;

            if (isUseExpand && this.IsExpanded)
            {
                return this.Children.First();
            }
            else if (parent.Children.Last() == this)
            {
                return parent.GetNext(root, false);
            }
            else
            {
                int index = parent.Children.IndexOf(this);
                return parent.Children[index + 1];
            }
        }

        private MenuTree GetLastChild()
        {
            if (!this.IsExpanded) return this;
            return this.Children.Last().GetLastChild();
        }


        //
        public MenuTree GetPrev(MenuTree root)
        {
            var parent = this.GetParent(root);
            if (parent == null) return null;

            if (parent.Children.First() == this)
            {
                return parent;
            }
            else
            {
                int index = parent.Children.IndexOf(this);
                var prev = parent.Children[index - 1];
                return prev.GetLastChild();
            }
        }



        //
        public object CreateMenuControl()
        {
            switch (this.MenuElementType)
            {
                case MenuElementType.Command:
                    {
                        var item = new MenuItem();
                        item.Header = this.Label;
                        item.Command = ModelContext.BookCommands[this.Command];
                        if (ModelContext.CommandTable[this.Command].CreateIsCheckedBinding != null)
                        {
                            item.SetBinding(MenuItem.IsCheckedProperty, ModelContext.CommandTable[this.Command].CreateIsCheckedBinding());
                        }
                        return item;
                    }
                case MenuElementType.Separator:
                    return new Separator();

                case MenuElementType.Group:
                    {
                        var item = new MenuItem();
                        item.Header = this.Label;
                        foreach (var child in this.Children)
                        {
                            var control = child.CreateMenuControl();
                            if (control != null) item.Items.Add(control);
                        }
                        item.IsEnabled = item.Items.Count > 0;
                        return item;
                    }

                default:
                    return null;
            }
        }

        public ContextMenu CreateContextMenu()
        {
            if (this.Children == null) return null;
            var contextMenu = new ContextMenu();

            foreach (var element in this.Children)
            {
                var control = element.CreateMenuControl();
                if (control != null) contextMenu.Items.Add(control);
            }

            return contextMenu.Items.Count > 0 ? contextMenu : null;
        }

        //
        public static MenuTree CreateDefault()
        {
            var tree = new MenuTree()
            {
                MenuElementType = MenuElementType.Group,
                Children = new ObservableCollection<MenuTree>()
                {
                    new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.ReLoad },
                    new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.ViewReset },
                    new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.ToggleFullScreen },
                    new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.OpenSettingWindow },
                    new MenuTree() { MenuElementType = MenuElementType.Separator },
                    new MenuTree() { MenuElementType = MenuElementType.Group, Name="ファイル(_F)", Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.LoadAs },
                        new MenuTree() { MenuElementType = MenuElementType.Separator },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.OpenApplication },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.OpenFilePlace },
                        new MenuTree() { MenuElementType = MenuElementType.Separator },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.Export },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.DeleteFile },
                    }},
                    new MenuTree() { MenuElementType = MenuElementType.Group, Name="ページモード(_P)", Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.SetPageMode1 },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.SetPageMode2 },
                    }},
                    new MenuTree() { MenuElementType = MenuElementType.Group, Name="スケール(_S)", Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.SetStretchModeNone },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.SetStretchModeInside },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.SetStretchModeOutside },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.SetStretchModeUniform },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.SetStretchModeUniformToFill },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.SetStretchModeUniformToSize },
                        new MenuTree() { MenuElementType = MenuElementType.Command, Command = CommandType.SetStretchModeUniformToVertical },
                    }},
                }
            };
            return tree;
        }
    }
}
