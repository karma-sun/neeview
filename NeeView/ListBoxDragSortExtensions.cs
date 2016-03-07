// Copyright (c) 2016 Mitsuhiro Ito (nee)
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

namespace NeeView
{
    /// <summary>
    /// ListBoxのドラッグ&ドロップによる順番入れ替え用ヘルパ
    /// 使用条件：ItemsSource が List<T>
    /// </summary>
    public static class ListBoxDragSortExtension
    {
        // event PreviewDragOver
        // Drop前の受け入れ判定
        public static void PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListBoxItem)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }


        // event Drop
        public static void Drop<T>(object sender, DragEventArgs e) where T : class
        {
            if (!e.Data.GetDataPresent(typeof(ListBoxItem))) return;

            var listBox = sender as ListBox;

            // ドラッグオブジェクト
            var item = (e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem)?.DataContext as T;
            if (item == null) return;

            // ドラッグオブジェクトが所属しているリスト
            var items = listBox.ItemsSource as List<T>;
            if (!items.Contains(item)) return;

            var dropPos = e.GetPosition(listBox);
            int indexFrom = items.IndexOf(item);
            int indexTo = items.Count;
            for (int i = 0; i < items.Count; i++)
            {
                var listBoxItem = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (listBoxItem == null) continue;

                var pos = listBox.PointFromScreen(listBoxItem.PointToScreen(new Point(0, listBoxItem.ActualHeight / 2)));
                if (dropPos.Y < pos.Y)
                {
                    indexTo = i;
                    break;
                }
            }
            items.Move(indexFrom, indexTo);
            listBox.InvalidateProperty(ListBox.ItemsSourceProperty);
            listBox.Items.Refresh();
        }
    }


    /// <summary>
    /// List拡張
    /// </summary>
    public static class ListExtensions
    {
        // List要素の順番変更
        public static void Move<T>(this List<T> list, int a0, int a1)
        {
            if (a0 == a1) return;

            var value = list.ElementAt(a0);

            list.RemoveAt(a0);
            if (a0 < a1) a1--;
            if (a1 > list.Count) a1 = list.Count;
            if (a1 < 0) a1 = 0;

            list.Insert(a1, value);
        }
    }
}
