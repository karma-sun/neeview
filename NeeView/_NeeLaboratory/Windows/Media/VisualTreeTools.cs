// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


namespace NeeLaboratory.Windows.Media
{
    /// <summary>
    /// 
    /// </summary>
    public static class VisualTreeTools
    {
        /// <summary>
        /// ListBox とその要素から、名前を指定してのコントロールを取得する 
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FrameworkElement GetListBoxItemElement(ListBox listBox, object item, string name)
        {
            return GetListBoxItemElement(GetListBoxItemFromItem(listBox, item), name);
        }

        /// <summary>
        /// ListBox とその要素から、ListBoxItem を取得する
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ListBoxItem GetListBoxItemFromItem(ListBox listBox, object item)
        {
            return (ListBoxItem)(listBox.ItemContainerGenerator.ContainerFromItem(item));
        }


        /// <summary>
        /// ListView とその要素から、ListViewItem を取得する
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ListViewItem GetListViewItemFromItem(ListView listView, object item)
        {
            return (ListViewItem)(listView.ItemContainerGenerator.ContainerFromItem(item));
        }


        /// <summary>
        /// ListBox と index から、ListBoxItem を取得する
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ListBoxItem GetListBoxItemFromIndex(ListBox listBox, int index)
        {
            return (ListBoxItem)(listBox.ItemContainerGenerator.ContainerFromIndex(index));
        }


        /// <summary>
        /// ListBoxitem から、名前を指定してコントロールを取得する 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FrameworkElement GetListBoxItemElement(ListBoxItem item, string name)
        {
            if (item == null) return null;

            // Getting the ContentPresenter of myListBoxItem
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(item);

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            if (myDataTemplate == null) throw new InvalidOperationException("DataTempate not exist.");
            return (FrameworkElement)myDataTemplate.FindName(name, myContentPresenter);
        }

        /// <summary>
        /// ListBoxitem から、型、名前を指定してコントロールを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetListBoxItemElement<T>(ListBoxItem item, string name = null)
            where T : FrameworkElement
        {
            if (item == null) return null;

            // Getting the ContentPresenter of myListBoxItem
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(item);

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate ?? (myContentPresenter.Content as ContentPresenter)?.ContentTemplate;

            if (myDataTemplate != null)
            {
                return myDataTemplate.FindName(name, myContentPresenter) as T;
            }
            else
            {
                return FindVisualChild<T>(item, name);
            }
        }


        /// <summary>
        /// DependencyObject から、型、名前を指定してコントロールを取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetChildElement<T>(DependencyObject item, string name = null)
            where T : FrameworkElement
        {
            if (item == null) return null;

            // Getting the ContentPresenter of myListBoxItem
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(item);

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate ?? (myContentPresenter.Content as ContentPresenter)?.ContentTemplate;

            if (myDataTemplate != null)
            {
                return myDataTemplate.FindName(name, myContentPresenter) as T;
            }
            else
            {
                return FindVisualChild<T>(item, name);
            }
        }

        #region for TreeView

        /// <summary>
        /// TreeViewの指定した項目のVisualControlを取得する
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="item">指定項目</param>
        /// <param name="name">コントロール名</param>
        /// <returns></returns>
        public static FrameworkElement GetTreeViewItemElement(TreeView treeView, object item, string name)
        {
            return GetTreeViewItemElement(FindContainer<TreeViewItem>(treeView, item), name);
        }

        /// <summary>
        /// childItemに対応したTreeViewItemの検索
        /// </summary>
        /// <param name="parent">親ノード。TreeViewまたはTreeViewItem</param>
        /// <param name="childItem">TreeViewItemを取得したいitem</param>
        /// <returns></returns>
        public static T FindContainer<T>(ItemsControl parent, object childItem) where T : DependencyObject
        {
            if (parent.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                // Containerが生成されていない
                return null;
            }
            var container = parent.ItemContainerGenerator.ContainerFromItem(childItem);
            if (container != null)
            {
                // parentの子の中にContainerが見つかった
                return container as T;
            }
            // parentの子を親として再帰検索
            foreach (var item in parent.Items)
            {
                var child = parent.ItemContainerGenerator.ContainerFromItem(item) as ItemsControl;
                if (child != null && child.Items.Count > 0)
                {
                    var result = FindContainer<T>(child, childItem);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// TreViewItem から名前を指定して VisualChildを取得する
        /// </summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FrameworkElement GetTreeViewItemElement(TreeViewItem item, string name)
        {
            if (item == null) return null;

            // Getting the ContentPresenter of myListBoxItem
            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(item);

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
            return (FrameworkElement)myDataTemplate.FindName(name, myContentPresenter);
        }

        #endregion


        /// <summary>
        /// 指定した型の子要素で最初に見つかったビジュアル要素を返す
        /// http://matatabi-ux.hateblo.jp/entry/2014/02/17/075520
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="root">探索対象のビジュアル要素</param>
        /// <returns>見つかった場合はその要素</returns>
        public static T FindVisualChild<T>(DependencyObject root, string name = null) where T : FrameworkElement
        {
            if (root == null)
            {
                return null;
            }

            var result = root as T;
            if (result != null && (string.IsNullOrEmpty(name) || name.Equals(result.Name)))
            {
                return result;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < childCount; i++)
            {
                var child = FindVisualChild<T>(VisualTreeHelper.GetChild(root, i), name);
                if (child != null)
                {
                    return child;
                }
            }
            return null;
        }
    }
}
