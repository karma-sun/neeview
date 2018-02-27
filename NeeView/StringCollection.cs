using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 文字列コレクション
    /// </summary>
    public class StringCollection : ObservableCollection<string>
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

        //
        public StringCollection(IEnumerable<string> items) : base(items)
        {
        }

        //
        public new virtual string Add(string item)
        {
            base.Add(item);
            return item;
        }

        //
        public new virtual bool Remove(string item)
        {
            return base.Remove(item);
        }

        //
        public void Sort()
        {
            FromCollection(this.OrderBy(e => e).ToList());
        }

        //
        public void FromCollection(IEnumerable<string> items)
        {
            Clear();

            if (items != null)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// セミコロン区切りの文字列を分解してコレクションにする
        /// </summary>
        /// <param name="items"></param>
        public virtual void FromString(string items)
        {
            FromCollection(items?.Split(';').Select(e => e.Trim()).OrderBy(e => e));
        }

        /// <summary>
        /// セミコロンで連結した１つの文字列を作成する
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(";", this);
        }
    }

}
