// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// テーブル値 基底
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract public class IndexValue<T> : BindableBase
    {
        //
        public event EventHandler<ValueChangedEventArgs<T>> ValueChanged;

        //
        public bool IsValueSyncIndex { get; set; } = true;

        //  
        private List<T> _values;

        /// <summary>
        /// Max Index
        /// </summary>
        public int IndexMax => _values.Count - 1;

        /// <summary>
        /// constructor
        /// </summary>
        public IndexValue(List<T> values)
        {
            _values = values;
            _index = 0;
            _value = _values[_index];
        }

        /// <summary>
        /// Index property.
        /// </summary>
        private int _index;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = NVUtility.Clamp<int>(value, 0, IndexMax);

                SetValue(_values[_index]);
            }
        }

        /// <summary>
        /// Value
        /// </summary>
        private T _value;
        public T Value
        {
            get { return _value; }

            set
            {
                _index = IndexOfNear(value, _values);
                SetValue(IsValueSyncIndex ? _values[_index] : value);
            }
        }

        //
        virtual public string ValueString => Value.ToString();

        //
        public override string ToString()
        {
            return ValueString;
        }


        //
        abstract protected int IndexOfNear(T value, IEnumerable<T> values);


        //
        private void SetValue(T value)
        {
            _value = value; // _values[_index];

            RaisePropertyChanged(null);
            ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>() { NewValue = _value });
        }
    }

    /// <summary>
    /// 値変更イベント引数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public T NewValue { get; set; }
    }

    /// <summary>
    /// テーブル値(int)
    /// </summary>
    public class IndexIntValue : IndexValue<int>
    {
        public IndexIntValue(List<int> values) : base(values)
        {
        }

        protected override int IndexOfNear(int value, IEnumerable<int> values)
        {
            var diff = values.Select((x, index) =>
            {
                var diffX = Math.Abs(x - value);
                return new { index, diffX };
            });
            return diff.OrderBy(d => d.diffX).First().index;
        }
    }

    /// <summary>
    /// テーブル値(double)
    /// </summary>
    public class IndexDoubleValue : IndexValue<double>
    {
        public IndexDoubleValue(List<double> values) : base(values)
        {
        }

        protected override int IndexOfNear(double value, IEnumerable<double> values)
        {
            var diff = values.Select((x, index) =>
            {
                var diffX = Math.Abs(x - value);
                return new { index, diffX };
            });
            return diff.OrderBy(d => d.diffX).First().index;
        }
    }

    /// <summary>
    /// テーブル値(TimeSpan)
    /// </summary>
    public class IndexTimeSpanValue : IndexValue<TimeSpan>
    {
        public IndexTimeSpanValue(List<TimeSpan> values) : base(values)
        {
        }

        protected override int IndexOfNear(TimeSpan value, IEnumerable<TimeSpan> values)
        {
            var diff = values.Select((x, index) =>
            {
                var diffX = (x - value).Duration();
                return new { index, diffX };
            });
            return diff.OrderBy(d => d.diffX).First().index;
        }
    }

}
