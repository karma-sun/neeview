using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    [DataContract]
    public class BookHistory
    {
        [DataMember]
        private LinkedList<BookSetting> _History;

        private const int _LimitCount = 100;

        private void Constructor()
        {
            _History = new LinkedList<BookSetting>();
        }

        public BookHistory()
        {
            Constructor();
        }

        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        public void Clear()
        {
            _History.Clear();
        }

        public void Add(Book book)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            var item = _History.FirstOrDefault(e => e.Place == book.Place);
            if (item != null) _History.Remove(item);

            var setting = new BookSetting();
            setting.Store(book);
            _History.AddFirst(setting);

            while (_History.Count > _LimitCount)
            {
                _History.RemoveLast();
            }
        }

        public BookSetting Find(string place)
        {
            return _History.FirstOrDefault(e => e.Place == place);
        }

        public List<BookSetting> ListUp(int size)
        {
            var list = new List<BookSetting>();
            foreach(var item in _History)
            {
                if (list.Count >= size) break;
                list.Add(item);
            }
            return list;
        }
    }

}
