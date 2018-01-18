// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 文字列コレクション
    /// </summary>
    public class StringCollection
    {
        //
        public StringCollection()
        {
        }

        //
        public StringCollection(string items)
        {
            FromString(items);
        }

        protected List<string> _items = new List<string>();
        
        //
        public string this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        //
        public bool Contains(string item)
        {
            return _items.Contains(item);
        }

        //
        public void FromCollection(IEnumerable<string> items)
        {
            _items = new List<string>(items);
        }

        /// <summary>
        /// セミコロン区切りの文字列を分解してコレクションにする
        /// </summary>
        /// <param name="items"></param>
        public virtual void FromString(string items)
        {
            if (items == null) return;
            _items = items.Split(';').Select(e => e.Trim()).ToList();
        }

        /// <summary>
        /// セミコロンで連結した１つの文字列を作成する
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(";", _items);
        }
    }

}
