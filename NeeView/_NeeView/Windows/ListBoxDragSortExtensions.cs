// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Windows
{
    /// <summary>
    /// ListBoxのドラッグ&ドロップによる順番入れ替え用ヘルパ
    /// 使用条件：ItemsSource が ObservableCollection<T>
    /// </summary>
    public static class ListBoxDragSortExtension
    {
        /// <summary>
        /// Drop受け入れ判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="format">データフォーマット</param>
        public static void PreviewDragOver(object sender, DragEventArgs e, string format)
        {
            if (e.Data.GetDataPresent(format))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }


        /// <summary>
        /// Drop処理
        /// </summary>
        /// <typeparam name="T">データ形</typeparam>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="format">データフォーマット</param>
        /// <param name="items">データコレクション</param>
        public static void Drop<T>(object sender, DragEventArgs e, string format, ObservableCollection<T> items)
            where T : class
        {
            var listBox = sender as ListBox;

            // ドラッグオブジェクト
            var item = GetData<T>(e, format);
            if (item == null) return;

            // ドラッグオブジェクトが所属しているリスト判定
            if (items.Count <= 0 || !items.Contains(item)) return;

            var dropPos = e.GetPosition(listBox);
            int oldIndex = items.IndexOf(item);
            int newIndex = items.Count - 1;
            for (int i = 0; i < items.Count; i++)
            {
                var listBoxItem = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (listBoxItem == null) continue;

                var pos = listBoxItem.TranslatePoint(new Point(0, listBoxItem.ActualHeight), listBox);
                if (dropPos.Y < pos.Y)
                {
                    newIndex = i;
                    break;
                }
            }

            items.Move(oldIndex, newIndex);
        }

        //
        private static T GetData<T>(DragEventArgs e, string format)
            where T : class
        {
            try
            {
                return e.Data.GetData(format) as T;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Drop failed: {ex.Message}");
                return null;
            }
        }
    }
}
