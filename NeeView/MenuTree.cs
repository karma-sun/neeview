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

namespace NeeView
{
    public enum MenuElementType
    {
        None,
        Group,
        Command,
        History,
        Separator,
    }

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
            set { Name = (value == DefaultLabel) ? null : value; OnPropertyChanged(); }
        }
        #endregion

        public string DefaultLongLabel
        {
            get
            {
                switch (MenuElementType)
                {
                    default:
                    case MenuElementType.None:
                        return "(なし)";
                    case MenuElementType.Group:
                        return "《サブメニュー》";
                    case MenuElementType.Command:
                        return Command.ToDispLongString();
                    case MenuElementType.History:
                        return "《最近使ったフォルダー》";
                    case MenuElementType.Separator:
                        return "《セパレーター》";
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
                    case MenuElementType.None:
                        return "(なし)";
                    case MenuElementType.Group:
                        return "サブメニュー";
                    case MenuElementType.Command:
                        return Command.ToMenuString();
                    case MenuElementType.History:
                        return "最近使ったフォルダー";
                    case MenuElementType.Separator:
                        return "セパレーター";
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

                case MenuElementType.History:
                    {
                        var item = new MenuItem();
                        item.Header = this.Label;
                        item.SetBinding(MenuItem.ItemsSourceProperty, new Binding("LastFiles"));
                        item.SetBinding(MenuItem.IsEnabledProperty, new Binding("IsEnableLastFiles"));
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
            if (this.Children == null) return null;
            var menu = new Menu();

            foreach (var element in this.Children)
            {
                var control = element.CreateMenuControl();
                if (control != null) menu.Items.Add(control);
            }

            return menu.Items.Count > 0 ? menu : null;
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
                        return ModelContext.CommandTable[Command].Note;
                    case MenuElementType.History:
                        return "最近使ったフォルダーの一覧から開きます";
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
                    new MenuTree(MenuElementType.Group) { Name="ファイル(_F)", Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.LoadAs, Name="開く(_O)..." },
                        new MenuTree(MenuElementType.History),
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.OpenApplication },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.OpenFilePlace },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.CopyFile, Name="コピー(_C)" },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.Paste, Name="貼り付け(_V)" },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.Export, Name="保存(_S)..." },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.DeleteFile, Name="削除(_D)..." },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.CloseApplication, Name="終了(_X)" }, // Alt+F4
                    }},
                    new MenuTree(MenuElementType.Group) { Name="表示(_V)", Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleFolderList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisiblePageList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleBookmarkList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleHistoryList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleFileInfo },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleHidePanel },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleThumbnailList },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleHideThumbnailList },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleTitleBar },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleVisibleAddressBar },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleHideMenu },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsLoupe },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleTopmost },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleFullScreen },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleSlideShow },
                    }},
                    new MenuTree(MenuElementType.Group) { Name="画像(_I)", Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeNone },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeInside },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeOutside },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeUniform },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeUniformToFill },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeUniformToSize },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetStretchModeUniformToVertical },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleIsEnabledNearestNeighbor },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.ToggleEffectGrayscale },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundBlack },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundWhite },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundAuto },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetBackgroundCheck },
                        //new MenuTree(MenuElementType.Separator),
                        //new MenuTree(MenuElementType.Command) { Command = CommandType.SetEffectNone },
                        //new MenuTree(MenuElementType.Command) { Command = CommandType.SetEffectGrayscale },
                    }},
                    new MenuTree(MenuElementType.Group) { Name="移動(_J)", Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.PrevPage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.NextPage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.PrevOnePage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.NextOnePage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.FirstPage },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.LastPage },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.PrevFolder },
                        new MenuTree(MenuElementType.Command) { Command = CommandType.NextFolder },
                    }},
                    new MenuTree(MenuElementType.Group) { Name="ページ(_P)", Children = new ObservableCollection<MenuTree>()
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
                        new MenuTree(MenuElementType.Command) { Command = CommandType.SetSortModeRandom },
                    }},
                    new MenuTree(MenuElementType.Group) { Name="その他(_O)", Children = new ObservableCollection<MenuTree>()
                    {
                        new MenuTree(MenuElementType.Command) { Command = CommandType.OpenSettingWindow, Name="設定(_O)..." },
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Group) { Name="ヘルプ(_H)", Children = new ObservableCollection<MenuTree>()
                        {
                            new MenuTree(MenuElementType.Command) { Command = CommandType.HelpOnline },
                            new MenuTree(MenuElementType.Command) { Command = CommandType.HelpMainMenu },
                            new MenuTree(MenuElementType.Command) { Command = CommandType.HelpCommandList },
                        }},
                        new MenuTree(MenuElementType.Separator),
                        new MenuTree(MenuElementType.Command) { Command = CommandType.OpenVersionWindow },
                    }},
                }
            };
            return tree;
        }
    }
}
