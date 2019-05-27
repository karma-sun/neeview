using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeLaboratory.Collections.Specialized
{
    /// <summary>
    /// ファイル拡張子コレクション (immutable)
    /// </summary>
    [DataContract]
    public class FileExtensionCollection : IEnumerable<string>, IEquatable<FileExtensionCollection>
    {
        [DataMember]
        private List<string> _items;

        public FileExtensionCollection()
        {
            _items = new List<string>();
        }

        public FileExtensionCollection(IEnumerable<string> items)
        {
            _items = ValidateCollection(items);
        }

        public FileExtensionCollection(string items)
        {
            _items = ValidateCollection(items?.Split(';').Select(e => e.Trim()));
        }



        public bool IsEmpty()
        {
            return !_items.Any();
        }

        public bool Contains(string item)
        {
            item = ValidateItem(item);
            return _items.Contains(item);
        }


        public string ToOneLine()
        {
            return _items.Count > 0 ? string.Join(";", _items) : null;
        }

        private string ValidateItem(string item)
        {
            return string.IsNullOrWhiteSpace(item) ? null : "." + item.Trim().TrimStart('.').ToLower();
        }

        private List<string> ValidateCollection(IEnumerable<string> items)
        {
            if (items == null) return new List<string>();

            return items
                .Select(e => ValidateItem(e))
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .OrderBy(e => e)
                .ToList();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)_items).GetEnumerator();
        }

        public bool Equals(FileExtensionCollection other)
        {
            return _items.SequenceEqual(other._items);
        }

        public override bool Equals(object obj)
        {
            if (obj is FileExtensionCollection other)
            {
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _items.GetHashCode();
        }
    }
}
