// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // 待ち行列優先度
    public enum QueueElementPriority
    {
        Top, // 最優先
        Hi,
        Default,
        PageThumbnail, // サムネイル専用
        FolderThumbnail, // フォルダサムネイル専用
    }

    /// <summary>
    /// 優先順位付き待ち行列
    /// </summary>
    /// <typeparam name="T">要素の型</typeparam>
    public class PriorityQueue<T> where T : class
    {
        // 優先度毎の待機リスト
        private volatile Dictionary<QueueElementPriority, LinkedList<T>> _queue;

        // コンストラクタ
        public PriorityQueue()
        {
            _queue = new Dictionary<QueueElementPriority, LinkedList<T>>();
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                _queue[priority] = new LinkedList<T>();
            }
        }

        /// <summary>
        /// 追加
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="priority">優先度</param>
        public void Enqueue(T element, QueueElementPriority priority, bool reverse = false)
        {
            if (reverse)
            {
                _queue[priority].AddFirst(element);
            }
            else
            {
                _queue[priority].AddLast(element);
            }
        }

        /// <summary>
        /// 繰り返し処理
        /// </summary>
        /// <param name="cancelAction"></param>
        /// <param name="priority"></param>
        public void Foreach(Action<T> cancelAction, QueueElementPriority priority)
        {
            foreach (var element in _queue[priority])
            {
                cancelAction(element);
            }
        }


        // 待機要素数
        public int Count
        {
            get
            {
                int sum = 0;
                foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
                {
                    sum += _queue[priority].Count;
                }

                return sum;
            }
        }

        // 待機要素数
        public int CountAt(QueueElementPriority priority)
        {
            return _queue[priority].Count;
        }

        // 先頭要素の取得
        public T First()
        {
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                if (_queue[priority].Count > 0) return _queue[priority].First();
            }
            return default(T);
        }

        // 先頭要素の削除
        public void RemoveFirst()
        {
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                if (_queue[priority].Count > 0)
                {
                    _queue[priority].RemoveFirst();
                    return;
                }
            }
        }

        // 先頭要素を取得し、削除する
        public T Dequeue(QueueElementPriority priority, bool reverse = false)
        {
            if (_queue[priority].Count > 0)
            {
                if (reverse)
                {
                    var item = _queue[priority].Last();
                    _queue[priority].RemoveLast();
                    return item;
                }
                else
                {
                    var item = _queue[priority].First();
                    _queue[priority].RemoveFirst();
                    return item;
                }
            }
            else
            {
                return default(T);
            }
        }

        // 先頭要素を取得し、削除する
        public T DequeueAll(QueueElementPriority? bottom = null)
        {
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                if (bottom != null && priority > bottom) break;
                var item = Dequeue(priority);
                if (item != null)
                {
                    return item;
                }
            }
            return null;
        }

        // 要素の優先度を変更する
        public void ChangePriority(T element, QueueElementPriority newPriority)
        {
            // 検索し、見つかったら登録しなおす
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                if (priority == newPriority) continue;

                if (_queue[priority].Remove(element))
                {
                    Enqueue(element, newPriority);
                    return;
                }
            }
        }

        // 再登録
        public void ReAdd(T element, QueueElementPriority priority, bool reverse = false)
        {
            if (_queue[priority].Remove(element))
            {
                Enqueue(element, priority, reverse);
            }
        }
    }
}
