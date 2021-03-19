using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
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

namespace NeeView
{
    public enum MenuElementType
    {
        [AliasName]
        None,

        [AliasName]
        Group,

        [AliasName]
        Command,

        [AliasName]
        History,

        [AliasName]
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
    public class MenuTree : BindableBase, IEnumerable<MenuTree>
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

        [DataMember(Name = "Command", EmitDefaultValue = false)]
        public string CommandName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ObservableCollection<MenuTree> Children { get; set; }

        public MenuTree()
        {
        }

        public MenuTree(MenuElementType type)
        {
            MenuElementType = type;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (MenuElementType != MenuElementType.Command)
            {
                CommandName = null;
            }
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
                        return CommandTable.Current.GetElement(CommandName).LongText;
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
                        return CommandTable.Current.GetElement(CommandName).Menu;
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
                        Debug.WriteLine($"MenuTree.Validate: Remove EmptyNode");
                        removes.Add(child);
                    }
                    else if (child.MenuElementType == MenuElementType.Command && !CommandTable.Current.ContainsKey(child.CommandName))
                    {
                        Debug.WriteLine($"MenuTree.Validate: Remove CommandNode=\"{child.CommandName}\"");
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
            element.CommandName = source.CommandName;
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
            if (this.CommandName != target.CommandName) return false;
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
                        if (!CommandTable.Current.ContainsKey(this.CommandName))
                        {
                            Debug.WriteLine($"Command {this.CommandName} is not defined.");
                            return null;
                        }

                        var item = new MenuItem();
                        item.Header = this.Label;
                        item.Tag = this.CommandName;
                        item.Command = RoutedCommandTable.Current.Commands[this.CommandName];
                        item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
                        var binding = CommandTable.Current.GetElement(this.CommandName).CreateIsCheckedBinding();
                        if (binding != null)
                        {
                            item.SetBinding(MenuItem.IsCheckedProperty, binding);
                        }

                        //  右クリックでコマンドパラメーターの設定ウィンドウを開く
                        item.MouseRightButtonUp += (s, e) =>
                        {
                            if (s is MenuItem menuItem && menuItem.Tag is string command)
                            {
                                e.Handled = true;
                                MainWindowModel.Current.OpenCommandParameterDialog(command);
                            }
                        };

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
                        item.SetBinding(MenuItem.ItemsSourceProperty, new Binding(nameof(RecentBooks.LastFiles)) { Source = RecentBooks.Current });
                        item.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(RecentBooks.IsEnableLastFiles)) { Source = RecentBooks.Current });
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
            foreach (var element in CreateMenuItems())
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
                        return CommandTable.Current.GetElement(CommandName).Remarks;
                    case MenuElementType.History:
                        return Properties.Resources.MenuTree_History;
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
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTree_File, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { CommandName = "LoadAs" },
                        new MenuTree(MenuElementType.Command) { CommandName = "Unload" },
                        new MenuTree(MenuElementType.History),
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "OpenExternalApp" },
                        new MenuTree(MenuElementType.Command) { CommandName = "OpenExplorer" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "CopyFile" },
                        new MenuTree(MenuElementType.Command) { CommandName = "Paste" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ExportImageAs" },
                        new MenuTree(MenuElementType.Command) { CommandName = "Print" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "DeleteFile" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "CloseApplication" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTree_View, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisibleBookshelf" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisiblePageList" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisibleBookmarkList" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisiblePagemarkList" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisibleHistoryList" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisibleFileInfo" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisibleNavigator" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisibleEffectInfo" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisibleSideBar" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleHidePanel" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisibleAddressBar" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleHideMenu" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisibleThumbnailList" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleHideThumbnailList" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleVisiblePageSlider" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleHidePageSlider" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleMainViewFloating" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleTopmost" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleFullScreen" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleSlideShow" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTree_Image, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { CommandName = "SetStretchModeNone" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetStretchModeUniform" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetStretchModeUniformToFill" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetStretchModeUniformToSize" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetStretchModeUniformToVertical" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetStretchModeUniformToHorizontal" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleStretchAllowScaleUp" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleStretchAllowScaleDown" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleNearestNeighbor" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleIsAutoRotateLeft" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleIsAutoRotateRight" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "SetBackgroundBlack" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetBackgroundWhite" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetBackgroundAuto" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetBackgroundCheck" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetBackgroundCheckDark" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetBackgroundCustom" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleHoverScroll" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTree_Jump, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { CommandName = "PrevPage" },
                        new MenuTree(MenuElementType.Command) { CommandName = "NextPage" },
                        new MenuTree(MenuElementType.Command) { CommandName = "PrevOnePage" },
                        new MenuTree(MenuElementType.Command) { CommandName = "NextOnePage" },
                        new MenuTree(MenuElementType.Command) { CommandName = "PrevSizePage" },
                        new MenuTree(MenuElementType.Command) { CommandName = "NextSizePage" },
                        new MenuTree(MenuElementType.Command) { CommandName = "FirstPage" },
                        new MenuTree(MenuElementType.Command) { CommandName = "LastPage" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "PrevBook" },
                        new MenuTree(MenuElementType.Command) { CommandName = "NextBook" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTree_Page, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { CommandName = "SetPageModeOne" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetPageModeTwo" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "SetBookReadOrderRight" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetBookReadOrderLeft" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleIsSupportedDividePage" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleIsSupportedWidePage" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleIsSupportedSingleFirstPage" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleIsSupportedSingleLastPage" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleIsRecursiveFolder" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "SetSortModeFileName" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetSortModeFileNameDescending" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetSortModeTimeStamp" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetSortModeTimeStampDescending" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetSortModeSize" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetSortModeSizeDescending" },
                        new MenuTree(MenuElementType.Command) { CommandName = "SetSortModeRandom" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "SetDefaultPageSetting" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTree_Bookmark, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { CommandName = "ToggleBookmark" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "TogglePagemark" },
                        new MenuTree(MenuElementType.Command) { CommandName = "PrevPagemark"},
                        new MenuTree(MenuElementType.Command) { CommandName = "NextPagemark" },
                        new MenuTree(MenuElementType.Command) { CommandName = "PrevPagemarkInBook"},
                        new MenuTree(MenuElementType.Command) { CommandName = "NextPagemarkInBook" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTree_Option, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { CommandName = "OpenOptionsWindow" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "ReloadSetting" },
                        new MenuTree(MenuElementType.Command) { CommandName = "ExportBackup"},
                        new MenuTree(MenuElementType.Command) { CommandName = "ImportBackup"},
                        new MenuTree(MenuElementType.Command) { CommandName = "OpenSettingFilesFolder" },
                        new MenuTree(MenuElementType.Command) { CommandName = "OpenScriptsFolder" },
                        new MenuTree(MenuElementType.Command) { CommandName = "OpenConsole" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "TogglePermitFile"},
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTree_Help, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { CommandName = "HelpMainMenu" },
                        new MenuTree(MenuElementType.Command) { CommandName = "HelpCommandList" },
                        new MenuTree(MenuElementType.Command) { CommandName = "HelpScript" },
                        new MenuTree(MenuElementType.Command) { CommandName = "HelpSearchOption" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { CommandName = "OpenVersionWindow" },
                    }},
                }
            };

            // Appxは設定ファイル閲覧が無意味
            if (Environment.IsAppxPackage)
            {
                tree.RemoveCommand("OpenSettingFilesFolder");
            }

            CheckCommandEntry(tree);

            return tree;
        }

        [Conditional("DEBUG")]
        private static void CheckCommandEntry(MenuTree tree)
        {
            foreach (var node in tree)
            {
                if (node.MenuElementType == MenuElementType.Command)
                {
                    Debug.Assert(CommandTable.Current.ContainsKey(node.CommandName));
                }
            }
        }

        private void RemoveCommand(string commandName)
        {
            if (this.Children == null) return;

            var removes = new List<MenuTree>();

            foreach (var item in this.Children)
            {
                switch (item.MenuElementType)
                {
                    case MenuElementType.Group:
                        item.RemoveCommand(commandName);
                        break;
                    case MenuElementType.Command:
                        if (item.CommandName == commandName)
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

        #region IEnumerable support

        public IEnumerator<MenuTree> GetEnumerator()
        {
            yield return this;

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var subChild in child)
                    {
                        yield return subChild;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        public MenuNode CreateMenuNode()
        {
            var node = new MenuNode();
            node.Name = this.Name;
            node.MenuElementType = this.MenuElementType;
            node.CommandName = this.CommandName;

            if (this.Children != null)
            {
                node.Children = new List<MenuNode>();
                foreach (var child in this.Children)
                {
                    node.Children.Add(child.CreateMenuNode());
                }
            }

            return node;
        }

        public static MenuTree CreateMenuTree(MenuNode node)
        {
            var tree = new MenuTree();
            tree.Name = node.Name;
            tree.MenuElementType = node.MenuElementType;
            tree.CommandName = node.CommandName;

            if (node.Children != null)
            {
                tree.Children = new ObservableCollection<MenuTree>();
                foreach (var child in node.Children)
                {
                    tree.Children.Add(CreateMenuTree(child));
                }
            }

            return tree;
        }
    }
}
