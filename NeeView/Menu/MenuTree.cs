﻿using NeeLaboratory.ComponentModel;
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

namespace NeeView
{
    public enum MenuElementType
    {
        [AliasName("@EnumMenuElementTypeNone")]
        None,

        [AliasName("@EnumMenuElementTypeGroup")]
        Group,

        [AliasName("@EnumMenuElementTypeCommand")]
        Command,

        [AliasName("@EnumMenuElementTypeHistory")]
        History,

        [AliasName("@EnumMenuElementTypeSeparator")]
        Separator,
    }

    /// <summary>
    /// コマンドパラメータとして渡すオブジェクト。メニューからのコマンドであることを示す
    /// </summary>
    public class MenuCommandTag
    {
        public static MenuCommandTag Tag { get; } = new MenuCommandTag();
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class MenuTree : BindableBase
    {
        #region Property: IsExpanded
        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Property: IsSelected
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged(); }
        }
        #endregion


        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember]
        public MenuElementType MenuElementType { get; set; }

        [DataMember(Name = "Command")]
        public string CommandString
        {
            get { return Command.ToString(); }
            set
            {
                if (Enum.TryParse(value, out CommandType command))
                {
                    Command = command;
                }
            }
        }
        public CommandType Command { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ObservableCollection<MenuTree> Children { get; set; }

        public MenuTree()
        {
        }

        public MenuTree(MenuElementType type)
        {
            MenuElementType = type;
        }


        #region Property: Label
        public string Label
        {
            get { return Name ?? DefaultLabel; }
            set { Name = (value == DefaultLabel) ? null : value; RaisePropertyChanged(); RaisePropertyChanged(nameof(DispLabel)); }
        }
        #endregion

        public string DispLabel => Label?.Replace("_", "");


        public string DefaultLongLabel
        {
            get
            {
                switch (MenuElementType)
                {
                    default:
                        return $"《{MenuElementType.ToAliasName()}》";
                    case MenuElementType.None:
                        return $"({MenuElementType.ToAliasName()})";
                    case MenuElementType.Command:
                        return Command.ToDispLongString();
                }
            }
        }


        public string DefaultLabel
        {
            get
            {
                switch (MenuElementType)
                {
                    default:
                        return MenuElementType.ToAliasName();
                    case MenuElementType.None:
                        return $"({MenuElementType.ToAliasName()})";
                    case MenuElementType.Command:
                        return Command.ToMenuString();
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

        // 掃除
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
                    if (child.MenuElementType == MenuElementType.None)
                    {
                        removes.Add(child);
                    }
                    else if (child.MenuElementType == MenuElementType.Command && child.Command.IsDisable())
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
        public bool IsEqual(MenuTree target)
        {
            if (this.MenuElementType != target.MenuElementType) return false;
            if (this.Label != target.Label) return false;
            if (this.Command != target.Command) return false;
            if (this.Children != null && target.Children != null)
            {
                if (this.Children.Count != target.Children.Count) return false;
                for (int i = 0; i < this.Children.Count; ++i)
                {
                    if (!this.Children[i].IsEqual(target.Children[i])) return false;
                }
            }
            else if (this.Children != null || target.Children != null) return false;

            return true;
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
                        item.Command = RoutedCommandTable.Current.Commands[this.Command];
                        item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
                        if (CommandTable.Current[this.Command].CreateIsCheckedBinding != null)
                        {
                            item.SetBinding(MenuItem.IsCheckedProperty, CommandTable.Current[this.Command].CreateIsCheckedBinding());
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

                case MenuElementType.History:
                    {
                        var item = new MenuItem();
                        item.Header = this.Label;
                        item.SetBinding(MenuItem.ItemsSourceProperty, new Binding(nameof(MenuBar.LastFiles)) { Source = MenuBar.Current });
                        item.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(MenuBar.IsEnableLastFiles)) { Source = MenuBar.Current });
                        item.ItemContainerStyle = App.Current.MainWindow.Resources["HistoryMenuItemContainerStyle"] as Style;
                        return item;
                    }

                case MenuElementType.None:
                    return null;

                default:
                    throw new NotImplementedException();
            }
        }

        //
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
        public Menu CreateMenu()
        {
            var menu = new Menu();
            foreach(var element in CreateMenuItems())
            {
                menu.Items.Add(element);
            }

            return menu.Items.Count > 0 ? menu : null;
        }

        //
        public List<object> CreateMenuItems()
        {
            var collection = new List<object>();

            if (this.Children != null)
            {
                foreach (var element in this.Children)
                {
                    var control = element.CreateMenuControl();
                    if (control != null) collection.Add(control);
                }
            }

            return collection;
        }

        //
        public class TableData
        {
            public int Depth { get; set; }
            public MenuTree Element { get; set; }

            public TableData(int depth, MenuTree element)
            {
                Depth = depth;
                Element = element;
            }
        }

        //
        public string Note
        {
            get
            {
                switch (MenuElementType)
                {
                    default:
                    case MenuElementType.None:
                        return "";
                    case MenuElementType.Group:
                        return "";
                    case MenuElementType.Command:
                        return CommandTable.Current[Command].Note;
                    case MenuElementType.History:
                        return Properties.Resources.MenuTreeHistory;
                    case MenuElementType.Separator:
                        return "";
                }
            }
        }

        //
        public List<TableData> GetTable(int depth)
        {
            var list = new List<TableData>();

            foreach (var child in Children)
            {
                if (child.MenuElementType == MenuElementType.None)
                    continue;
                if (child.MenuElementType == MenuElementType.Separator)
                    continue;

                list.Add(new TableData(depth, child));

                if (child.MenuElementType == MenuElementType.Group)
                {
                    list.AddRange(child.GetTable(depth + 1));
                }
            }

            return list;
        }


        // 
        public static MenuTree CreateDefault()
        {
            var tree = new MenuTree()
            {
                MenuElementType = MenuElementType.Group,
                Children = new ObservableCollection<MenuTree>()
                {
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeFile, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.LoadAs },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.Unload },
                        new MenuTree(MenuElementType.History),
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.OpenApplication },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.OpenFilePlace },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.CopyFile },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.Paste },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.Export },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.Print },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.DeleteFile },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.CloseApplication },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeView, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleBookshelf },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisiblePageList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleBookmarkList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisiblePagemarkList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleHistoryList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleFileInfo },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleEffectInfo },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleHidePanel },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleThumbnailList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleHideThumbnailList },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleTitleBar },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleAddressBar },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleSideBar },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleHideMenu },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleHidePageSlider },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleTopmost },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleFullScreen },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleSlideShow },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeImage, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeNone },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeUniform },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeUniformToFill },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeUniformToSize },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeUniformToVertical },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeUniformToHorizontal },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleStretchAllowEnlarge },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleStretchAllowReduce },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsEnabledNearestNeighbor },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsAutoRotateLeft },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsAutoRotateRight },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundBlack },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundWhite },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundAuto },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundCheck },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundCheckDark },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundCustom },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeJump, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.PrevPage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.NextPage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.PrevOnePage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.NextOnePage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.PrevSizePage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.NextSizePage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.FirstPage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.LastPage },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.PrevFolder },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.NextFolder },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreePage, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetPageMode1 },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetPageMode2 },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBookReadOrderRight },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBookReadOrderLeft },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsSupportedDividePage },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsSupportedWidePage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsSupportedSingleFirstPage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsSupportedSingleLastPage },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsRecursiveFolder },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetSortModeFileName },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetSortModeFileNameDescending },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetSortModeTimeStamp },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetSortModeTimeStampDescending },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetSortModeSize },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetSortModeSizeDescending },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetSortModeRandom },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetDefaultPageSetting },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeBookmark, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleBookmark },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.TogglePagemark },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.PrevPagemark},
                        new MenuTree(MenuElementType.Command) { Command = CommandType.NextPagemark },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.PrevPagemarkInBook},
                        new MenuTree(MenuElementType.Command) { Command = CommandType.NextPagemarkInBook },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeOption, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.OpenSettingWindow },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ReloadUserSetting },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ExportBackup},
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ImportBackup},
                        new MenuTree(MenuElementType.Command) { Command = CommandType.OpenSettingFilesFolder },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.TogglePermitFileCommand},
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeHelp, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.HelpMainMenu },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.HelpCommandList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.HelpSearchOption },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.OpenVersionWindow },
                    }},
                }
            };

            // Appxは設定ファイル閲覧が無意味
            if (Config.Current.IsAppxPackage)
            {
                tree.RemoveCommand(CommandType.OpenSettingFilesFolder);
            }

            return tree;
        }

        //
        private void RemoveCommand(CommandType commandType)
        {
            if (this.Children == null) return;

            var removes = new List<MenuTree>();

            foreach (var item in this.Children)
            {
                switch (item.MenuElementType)
                {
                    case MenuElementType.Group:
                        item.RemoveCommand(commandType);
                        break;
                    case MenuElementType.Command:
                        if (item.Command == commandType)
                        {
                            removes.Add(item);
                        }
                        break;
                }
            }

            foreach (var item in removes)
            {
                this.Children.Remove(item);
            }
        }
    }
}
