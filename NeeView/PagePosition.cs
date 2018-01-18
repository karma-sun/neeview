// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ページの場所を表す構造体。
    /// ページ番号と、部分を示すパーツ番号で構成されています。
    /// </summary>
    public struct PagePosition
    {
        #region Fields

        private readonly int _value;

        #endregion

        #region Properties

        /// <summary>
        /// ページの場所(0,0)
        /// </summary>
        public static PagePosition Zero { get; } = new PagePosition(0);

        //
        public int Value => _value;

        // ページ番号
        public int Index => _value / 2;

        // パーツ番号
        public int Part => _value % 2;

        #endregion

        #region Constructors

        public PagePosition(int value)
        {
            _value = value;
        }

        public PagePosition(int index, int part)
        {
            _value = index * 2 + part;
        }

        #endregion

        #region Methods

        //
        public override string ToString()
        {
            return Index.ToString() + (Part == 1 ? ".5" : "");
        }

        // clamp
        public PagePosition Clamp(PagePosition min, PagePosition max)
        {
            if (min._value > max._value) throw new ArgumentOutOfRangeException();

            int value = _value;
            if (value < min._value) value = min._value;
            if (value > max._value) value = max._value;

            return new PagePosition(value);
        }


        // add
        public static PagePosition operator +(PagePosition a, PagePosition b)
        {
            return new PagePosition(a._value + b._value);
        }

        public static PagePosition operator +(PagePosition a, int b)
        {
            return new PagePosition(a._value + b);
        }

        public static PagePosition operator -(PagePosition a, PagePosition b)
        {
            return new PagePosition(a._value - b._value);
        }

        public static PagePosition operator -(PagePosition a, int b)
        {
            return new PagePosition(a._value - b);
        }

        // compare
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is PagePosition)) return false;
            return _value == ((PagePosition)obj)._value;
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public static bool operator ==(PagePosition a, PagePosition b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(PagePosition a, PagePosition b)
        {
            return a._value != b._value;
        }

        public static bool operator <(PagePosition a, PagePosition b)
        {
            return a._value < b._value;
        }

        public static bool operator >(PagePosition a, PagePosition b)
        {
            return a._value > b._value;
        }

        public static bool operator <=(PagePosition a, PagePosition b)
        {
            return a._value <= b._value;
        }

        public static bool operator >=(PagePosition a, PagePosition b)
        {
            return a._value >= b._value;
        }

        #endregion
    }
}
