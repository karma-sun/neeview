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
        public LinkedList<BookSetting> History { get; private set; }

        [DataMember]
        private int _MaxHistoryCount;
        public int MaxHistoryCount
        {
            get { return _MaxHistoryCount; }
            set { _MaxHistoryCount = value; Resize(); }
        }

        private void Constructor()
        {
            History = new LinkedList<BookSetting>();
            MaxHistoryCount = 100;
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
            History.Clear();
        }

        private void Resize()
        {
            while (History.Count > MaxHistoryCount)
            {
                History.RemoveLast();
            }
        }

        public void Add(Book book)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            var item = History.FirstOrDefault(e => e.Place == book.Place);
            if (item != null) History.Remove(item);

            var setting = new BookSetting();
            setting.Store(book);
            History.AddFirst(setting);

            Resize();
        }

        public void Remove(string place)
        {
            var item = History.FirstOrDefault(e => e.Place == place);
            if (item != null) History.Remove(item);
        }

        public BookSetting Find(string place)
        {
            return History.FirstOrDefault(e => e.Place == place);
        }

        public List<BookSetting> ListUp(int size)
        {
            var list = new List<BookSetting>();
            foreach (var item in History)
            {
                if (list.Count >= size) break;
                list.Add(item);
            }
            return list;
        }
    }

}
