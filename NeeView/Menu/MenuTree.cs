using NeeLaboratory.ComponentModel;
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

        // TODO: 不要？
        [DataMember(Name = "Command")]
        public string CommandString
        {
            get { return Command; }
            set { Command = value; }
        }

        public string Command { get; set; }

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
                        return Command.ToCommand().LongText;
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
                        return Command.ToCommand().MenuText;
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
                    else if (child.MenuElementType == MenuElementType.Command)
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
                        item.Tag = this.Command;
                        item.Command = RoutedCommandTable.Current.Commands[this.Command];
                        item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
                        var binding = CommandTable.Current[this.Command].CreateIsCheckedBinding();
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
                        new MenuTree(MenuElementType.Command) { Command = "LoadAs" },
                        new MenuTree(MenuElementType.Command) { Command = "Unload" },
                        new MenuTree(MenuElementType.History),
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "OpenApplication" },
                        new MenuTree(MenuElementType.Command) { Command = "OpenFilePlace" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "CopyFile" },
                        new MenuTree(MenuElementType.Command) { Command = "Paste" },
                        new MenuTree(MenuElementType.Command) { Command = "Export" },
                        new MenuTree(MenuElementType.Command) { Command = "Print" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "DeleteFile" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "CloseApplication" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeView, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisibleBookshelf" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisiblePageList" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisibleBookmarkList" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisiblePagemarkList" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisibleHistoryList" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisibleFileInfo" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisibleEffectInfo" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleHidePanel" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisibleThumbnailList" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleHideThumbnailList" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisibleTitleBar" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisibleAddressBar" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleVisibleSideBar" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleHideMenu" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleHidePageSlider" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleTopmost" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleFullScreen" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleSlideShow" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeImage, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = "SetStretchModeNone" },
                        new MenuTree(MenuElementType.Command) { Command = "SetStretchModeUniform" },
                        new MenuTree(MenuElementType.Command) { Command = "SetStretchModeUniformToFill" },
                        new MenuTree(MenuElementType.Command) { Command = "SetStretchModeUniformToSize" },
                        new MenuTree(MenuElementType.Command) { Command = "SetStretchModeUniformToVertical" },
                        new MenuTree(MenuElementType.Command) { Command = "SetStretchModeUniformToHorizontal" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleStretchAllowEnlarge" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleStretchAllowReduce" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleIsEnabledNearestNeighbor" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleIsAutoRotateLeft" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleIsAutoRotateRight" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "SetBackgroundBlack" },
                        new MenuTree(MenuElementType.Command) { Command = "SetBackgroundWhite" },
                        new MenuTree(MenuElementType.Command) { Command = "SetBackgroundAuto" },
                        new MenuTree(MenuElementType.Command) { Command = "SetBackgroundCheck" },
                        new MenuTree(MenuElementType.Command) { Command = "SetBackgroundCheckDark" },
                        new MenuTree(MenuElementType.Command) { Command = "SetBackgroundCustom" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeJump, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = "PrevPage" },
                        new MenuTree(MenuElementType.Command) { Command = "NextPage" },
                        new MenuTree(MenuElementType.Command) { Command = "PrevOnePage" },
                        new MenuTree(MenuElementType.Command) { Command = "NextOnePage" },
                        new MenuTree(MenuElementType.Command) { Command = "PrevSizePage" },
                        new MenuTree(MenuElementType.Command) { Command = "NextSizePage" },
                        new MenuTree(MenuElementType.Command) { Command = "FirstPage" },
                        new MenuTree(MenuElementType.Command) { Command = "LastPage" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "PrevFolder" },
                        new MenuTree(MenuElementType.Command) { Command = "NextFolder" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreePage, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = "SetPageMode1" },
                        new MenuTree(MenuElementType.Command) { Command = "SetPageMode2" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "SetBookReadOrderRight" },
                        new MenuTree(MenuElementType.Command) { Command = "SetBookReadOrderLeft" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleIsSupportedDividePage" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleIsSupportedWidePage" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleIsSupportedSingleFirstPage" },
                        new MenuTree(MenuElementType.Command) { Command = "ToggleIsSupportedSingleLastPage" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ToggleIsRecursiveFolder" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "SetSortModeFileName" },
                        new MenuTree(MenuElementType.Command) { Command = "SetSortModeFileNameDescending" },
                        new MenuTree(MenuElementType.Command) { Command = "SetSortModeTimeStamp" },
                        new MenuTree(MenuElementType.Command) { Command = "SetSortModeTimeStampDescending" },
                        new MenuTree(MenuElementType.Command) { Command = "SetSortModeSize" },
                        new MenuTree(MenuElementType.Command) { Command = "SetSortModeSizeDescending" },
                        new MenuTree(MenuElementType.Command) { Command = "SetSortModeRandom" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "SetDefaultPageSetting" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeBookmark, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = "ToggleBookmark" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "TogglePagemark" },
                        new MenuTree(MenuElementType.Command) { Command = "PrevPagemark"},
                        new MenuTree(MenuElementType.Command) { Command = "NextPagemark" },
                        new MenuTree(MenuElementType.Command) { Command = "PrevPagemarkInBook"},
                        new MenuTree(MenuElementType.Command) { Command = "NextPagemarkInBook" },
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeOption, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = "OpenSettingWindow" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "ReloadUserSetting" },
                        new MenuTree(MenuElementType.Command) { Command = "ExportBackup"},
                        new MenuTree(MenuElementType.Command) { Command = "ImportBackup"},
                        new MenuTree(MenuElementType.Command) { Command = "OpenSettingFilesFolder" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "TogglePermitFileCommand"},
                    }},
                    new MenuTree(MenuElementType.Group) { Name=Properties.Resources.MenuTreeHelp, Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = "HelpMainMenu" },
                        new MenuTree(MenuElementType.Command) { Command = "HelpCommandList" },
                        new MenuTree(MenuElementType.Command) { Command = "HelpSearchOption" },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = "OpenVersionWindow" },
                    }},
                }
            };

            // Appxは設定ファイル閲覧が無意味
            if (Config.Current.IsAppxPackage)
            {
                tree.RemoveCommand("OpenSettingFilesFolder");
            }

            return tree;
        }

        private void RemoveCommand(string commandType)
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
