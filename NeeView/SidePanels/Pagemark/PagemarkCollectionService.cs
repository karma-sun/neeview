using NeeView.Collections;
using NeeView.Collections.Generic;
using System;
using System.Linq;

namespace NeeView
{
    public static class PagemarkCollectionService
    {
        /// <summary>
        /// ページマークの名前変更とそれに伴う統合を行う
        /// </summary>
        public static bool Rename(TreeListNode<IPagemarkEntry> node, string newName)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (!(node.Value is PagemarkFolder folder)) throw new ArgumentOutOfRangeException(nameof(node));

            newName = PagemarkFolder.GetValidateName(newName);
            var oldName = folder.Name;

            if (string.IsNullOrEmpty(newName))
            {
                return false;
            }

            if (newName != oldName)
            {
                var conflict = node.Parent.Children.FirstOrDefault(e => e != node && e.Value is PagemarkFolder && e.Value.Name == newName);
                if (conflict != null)
                {
                    var dialog = new MessageDialog(string.Format(Properties.Resources.DialogMergeFolder, newName), Properties.Resources.DialogMergeFolderTitle);
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    var result = dialog.ShowDialog();

                    if (result == UICommands.Yes)
                    {
                        PagemarkCollection.Current.Merge(node, conflict);
                        return true;
                    }
                }
                else
                {
                    folder.Name = newName;
                    PagemarkCollection.Current.RaisePagemarkChangedEvent(new PagemarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, node.Parent, node) { OldName = oldName });
                    return true;
                }
            }

            return false;
        }
    }
}
