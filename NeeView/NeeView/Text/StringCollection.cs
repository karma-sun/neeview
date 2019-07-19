﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView.Text
{

    /// <summary>
    /// 文字列コレクション
    /// </summary>
    [DataContract]
    public class StringCollection
    {
        public StringCollection()
        {
            Items = new List<string>();
        }

        public StringCollection(string items)
        {
            Restore(items);
        }

        public StringCollection(IEnumerable<string> items)
        {
            Restore(items);
        }


        [DataMember]
        public bool IsSorted { get; set; } = true;

        [DataMember]
        public bool IsDistinction { get; set; } = true;

        [DataMember]
        public bool IsNullable { get; set; } = false;

        // immutable
        [DataMember]
        public List<string> Items { get; private set; }


        [DataMember(EmitDefaultValue = false)]
        public string OneLine
        {
            get { return Items.Count > 0 ? string.Join(";", Items) : null; }
            set { Restore(value); }
        }


        public bool IsEmpty()
        {
            return !Items.Any();
        }

        public void Clear()
        {
            Items = new List<string>();
        }

        public bool Contains(string item)
        {
            item = ValidateItem(item);
            return Items.Contains(item);
        }

        public string Add(string item)
        {
            item = ValidateItem(item);
            AddRange(new List<string>() { item });
            return item;
        }

        public void AddRange(IEnumerable<string> items)
        {
            Items = ValidateCollection(Items.Concat(items));
        }

        public void Remove(string item)
        {
            RemoveRange(new List<string>() { item });
        }

        public void RemoveRange(IEnumerable<string> items)
        {
            Items = Items.Except(ValidateCollection(items)).ToList();
        }

        public void Restore(IEnumerable<string> items)
        {
            Items = ValidateCollection(items);
        }

        /// <summary>
        /// セミコロン区切りの文字列を分解してコレクションにする
        /// </summary>
        public void Restore(string items)
        {
            Items = ValidateCollection(items?.Split(';').Select(e => e.Trim()));
        }

        /// <summary>
        /// セミコロンで連結した１つの文字列を作成する
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return OneLine;
        }

        /// <summary>
        /// 項目のフォーマット
        /// </summary>
        public virtual string ValidateItem(string items)
        {
            return items;
        }

        private List<string> ValidateCollection(IEnumerable<string> items)
        {
            if (items == null) return new List<string>();

            var collection = items;

            if (!IsNullable)
            {
                collection = collection.Where(e => !string.IsNullOrEmpty(e));
            }
            if (IsDistinction)
            {
                collection = collection.Distinct();
            }
            if (IsSorted)
            {
                collection = collection.OrderBy(e => e);
            }

            return collection.ToList();
        }

    }

}