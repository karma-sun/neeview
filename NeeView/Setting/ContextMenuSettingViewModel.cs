using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView.Setting
{
    public class ContextMenuSettingViewModel : BindableBase
    {
        private MenuTree _root;
        public MenuTree Root
        {
            get { return _root; }
            set { _root = value; RaisePropertyChanged(); }
        }

        public List<MenuTree> SourceElementList { get; set; }


        private ContextMenuSetting _contextMenuSetting;

        public ContextMenuSettingViewModel()
        {
            if (CommandTable.Current == null) return;

            var list = CommandTable.Current.Keys
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

            Decide();
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

            Decide();
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

                Decide();
            }
        }

        //
        public void RenameNode(MenuTree target, string name)
        {
            target.Label = name;

            Decide();
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

                Decide();
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

                Decide();
            }
        }
    }
}
