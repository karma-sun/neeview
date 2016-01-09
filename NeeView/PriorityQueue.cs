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
        Top, // 再優先
        Hi,
        Default // 標準
    }

    /// <summary>
    /// 優先順位付き待ち行列
    /// </summary>
    /// <typeparam name="T">要素の型</typeparam>
    public class PriorityQueue<T>
    {
        // 優先度毎の待機リスト
        private volatile Dictionary<QueueElementPriority, LinkedList<T>> _Queue;

        // コンストラクタ
        public PriorityQueue()
        {
            _Queue = new Dictionary<QueueElementPriority, LinkedList<T>>();
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                _Queue[priority] = new LinkedList<T>();
            }
        }

        /// <summary>
        /// 追加
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="priority">優先度</param>
        public void Enqueue(T element, QueueElementPriority priority)
        {
            _Queue[priority].AddLast(element);
        }


        // 待機要素数
        public int Count
        {
            get
            {
                int sum = 0;
                foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
                {
                    sum += _Queue[priority].Count;
                }

                return sum;
            }
        }

        // 先頭要素の取得
        public T First()
        {
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                if (_Queue[priority].Count > 0) return _Queue[priority].First();
            }
            return default(T);
        }

        // 先頭要素の削除
        public void RemoveFirst()
        {
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                if (_Queue[priority].Count > 0)
                {
                    _Queue[priority].RemoveFirst();
                    return;
                }
            }
        }

        // 先頭要素を取得し、削除する
        public T Decueue()
        {
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                if (_Queue[priority].Count > 0)
                {
                    var item = _Queue[priority].First();
                    _Queue[priority].RemoveFirst();
                    return item;
                }
            }
            return default(T);
        }

        // 要素の優先度を変更する
        public void ChangePriority(T element, QueueElementPriority newPriority)
        {
            // 検索し、見つかったら登録しなおす
            foreach (QueueElementPriority priority in Enum.GetValues(typeof(QueueElementPriority)))
            {
                if (priority == newPriority) continue;

                if (_Queue[priority].Remove(element))
                {
                    Enqueue(element, newPriority);
                    return;
                }
            }
        }
    }
}
