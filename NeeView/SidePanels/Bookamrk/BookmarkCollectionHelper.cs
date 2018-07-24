﻿using NeeView.Collections.Generic;
using System;
using System.Linq;

namespace NeeView
{
    public static class BookmarkCollectionHelper
    {
        public static bool Rename(TreeListNode<IBookmarkEntry> node, string newName)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (!(node.Value is BookmarkFolder folder)) throw new ArgumentOutOfRangeException(nameof(node));

            newName = BookmarkFolder.GetValidateName(newName);
            var oldName = folder.Name;

            if (string.IsNullOrEmpty(newName))
            {
                return false;
            }

            if (newName != oldName)
            {
                var conflict = node.Parent.Children.FirstOrDefault(e => e != node && e.Value is BookmarkFolder && e.Value.Name == newName);
                if (conflict != null)
                {
                    var dialog = new MessageDialog(string.Format(Properties.Resources.DialogMergeFolder, newName), Properties.Resources.DialogMergeFolderTitle);
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    var result = dialog.ShowDialog();

                    if (result == UICommands.Yes)
                    {
                        BookmarkCollection.Current.Merge(node, conflict);
                        return true;
                    }
                }
                else
                {
                    folder.Name = newName;
                    BookmarkCollection.Current.RaiseBookmarkChangedEvent(new BookmarkCollectionChangedEventArgs(EntryCollectionChangedAction.Rename, node.Parent, node) { OldName = oldName });
                    return true;
                }
            }

            return false;
        }
    }
}