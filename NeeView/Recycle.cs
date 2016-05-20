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
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Recycle Manager.
    /// インスタンスのリサイクルを行います
    /// </summary>
    public class Recycle
    {
        public RectanglePool RectanglePool { get; private set; } = new RectanglePool();

        public void CleanUp()
        {
            RectanglePool.CleanUp();
        }
    }


    /// <summary>
    /// Rectangle Pool
    /// </summary>
    public class RectanglePool : VisualPool<Rectangle>
    {
        protected override void Free(Rectangle element)
        {
            element.Fill = null;
            element.Effect = null;
        }
    }

    /// <summary>
    /// Visual Element Pool generic
    /// </summary>
    /// <typeparam name="T">Visual</typeparam>
    public abstract class VisualPool<T> where T : Visual, new()
    {
        private List<T> _Pool = new List<T>();
        private List<T> _Locked = new List<T>();

        /// <summary>
        /// 未使用要素の確保
        /// 確保されていない、親に接続されていない要素を返す
        /// </summary>
        /// <returns></returns>
        public T Allocate()
        {
            var element = _Pool.FirstOrDefault(e => !_Locked.Contains(e) && LogicalTreeHelper.GetParent(e) == null);
            if (element == null)
            {
                element = new T();
                _Pool.Add(element);
            }
            _Locked.Add(element);
            return element;
        }

        /// <summary>
        /// 未使用要素の開放
        /// 親に接続されていない要素を開放する
        /// </summary>
        public void CleanUp()
        {
            _Locked.Clear();
            foreach (var element in _Pool.Where(e => LogicalTreeHelper.GetParent(e) == null))
            {
                Free(element);
            }
        }

        /// <summary>
        /// 要素の開放処理
        /// </summary>
        /// <param name="element"></param>
        protected abstract void Free(T element);
    }



}
