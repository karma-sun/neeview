// Copyright (c) 2016 Mitsuhiro Ito (nee)
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
        public int Value { get; set; }

        // ページ番号
        public int Index
        {
            get { return Value / 2; }
            set { Value = value * 2; }
        }

        // パーツ番号
        public int Part
        {
            get { return Value % 2; }
            set { Value = Index * 2 + value; }
        }

        // constructor
        public PagePosition(int index, int part)
        {
            Value = index * 2 + part;
        }

        //
        public override string ToString()
        {
            return Index.ToString() + (Part == 1 ? ".5" : "");
        }

        #region operators

        // add
        public static PagePosition operator +(PagePosition a, PagePosition b)
        {
            return new PagePosition() { Value = a.Value + b.Value };
        }

        public static PagePosition operator +(PagePosition a, int b)
        {
            return new PagePosition() { Value = a.Value + b };
        }

        public static PagePosition operator -(PagePosition a, PagePosition b)
        {
            return new PagePosition() { Value = a.Value - b.Value };
        }

        public static PagePosition operator -(PagePosition a, int b)
        {
            return new PagePosition() { Value = a.Value - b };
        }

        // compare
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is PagePosition)) return false;
            return Value == ((PagePosition)obj).Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(PagePosition a, PagePosition b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(PagePosition a, PagePosition b)
        {
            return a.Value != b.Value;
        }

        public static bool operator <(PagePosition a, PagePosition b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >(PagePosition a, PagePosition b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <=(PagePosition a, PagePosition b)
        {
            return a.Value <= b.Value;
        }

        public static bool operator >=(PagePosition a, PagePosition b)
        {
            return a.Value >= b.Value;
        }

        #endregion

        // clamp
        public PagePosition Clamp(PagePosition min, PagePosition max)
        {
            if (min.Value > max.Value) throw new ArgumentOutOfRangeException();

            int value = Value;
            if (value < min.Value) value = min.Value;
            if (value > max.Value) value = max.Value;

            return new PagePosition() { Value = value };
        }
    }
}
