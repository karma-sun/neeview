using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookHistory
    {
        public LinkedList<Book.Memento> History { get; private set; } = new LinkedList<Book.Memento>();

        private int _MaxHistoryCount = 100;
        public int MaxHistoryCount
        {
            get { return _MaxHistoryCount; }
            set { _MaxHistoryCount = value; Resize(); }
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

            var setting = new Book.Memento();
            setting = book.CreateMemento(); //.Store(book);
            History.AddFirst(setting);

            Resize();
        }

        public void Remove(string place)
        {
            var item = History.FirstOrDefault(e => e.Place == place);
            if (item != null) History.Remove(item);
        }

        public Book.Memento Find(string place)
        {
            return History.FirstOrDefault(e => e.Place == place);
        }

        public List<Book.Memento> ListUp(int size)
        {
            var list = new List<Book.Memento>();
            foreach (var item in History)
            {
                if (list.Count >= size) break;
                list.Add(item);
            }
            return list;
        }


        // Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public List<Book.Memento> History { get; set; }

            [DataMember]
            public int MaxHistoryCount { get; set; }

            private void Constructor()
            {
                History = new List<Book.Memento>();
                MaxHistoryCount = 100;
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.History = this.History.ToList();
            memento.MaxHistoryCount = this.MaxHistoryCount;
            return memento;
        }

        public void Restore(Memento memento)
        {
            this.History = new LinkedList<Book.Memento>(memento.History);
            this.MaxHistoryCount = memento.MaxHistoryCount;
        }
    }
}
