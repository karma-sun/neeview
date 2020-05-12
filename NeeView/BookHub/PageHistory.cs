using NeeView.Collections;
using System;
using System.Collections.Generic;

namespace NeeView
{
    public struct PageHistoryUnit : IEquatable<PageHistoryUnit>
    {
        public static PageHistoryUnit Empty = new PageHistoryUnit(null, null);

        public PageHistoryUnit(string bookAddress, string pageName)
        {
            BookAddress = bookAddress;
            PageName = pageName;
        }

        public string BookAddress { get; private set; }
        public string PageName { get; private set; }


        public bool IsEmpty()
        {
            return BookAddress == null && PageName == null;
        }

        public override bool Equals(object obj)
        {
            if (obj is PageHistoryUnit other)
            {
                return this.Equals(other);
            }
            return false;
        }

        public bool Equals(PageHistoryUnit other)
        {
            return (BookAddress == other.BookAddress) && (PageName == other.PageName);
        }

        public override int GetHashCode()
        {
            return BookAddress.GetHashCode() ^ PageName.GetHashCode();
        }

        public static bool operator ==(PageHistoryUnit lhs, PageHistoryUnit rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PageHistoryUnit lhs, PageHistoryUnit rhs)
        {
            return !(lhs.Equals(rhs));
        }
    }

    /// <summary>
    /// BookHubの履歴。
    /// 本を開いた順番そのままを記録している。
    /// </summary>
    public class PageHistory
    {
        static PageHistory() => Current = new PageHistory();
        public static PageHistory Current { get; }


        private const int _historyCapacity = 100;
        private readonly HistoryLimitedCollection<PageHistoryUnit> _history = new HistoryLimitedCollection<PageHistoryUnit>(_historyCapacity);


        public PageHistory()
        {
            _history.Changed += (s, e) => Changed?.Invoke(s, e);
        }


        public event EventHandler Changed;
      

        public void Add(PageHistoryUnit query)
        {
            if (query.IsEmpty()) return;

            _history.TrimEnd(PageHistoryUnit.Empty);

            if (query != _history.GetCurrent())
            {
                _history.Add(query);
            }
        }

        public bool CanMoveToPrevious()
        {
            return _history.CanPrevious();
        }

        public void MoveToPrevious()
        {
            if (!_history.CanPrevious()) return;

            var query = _history.GetPrevious();
            LoadPage(query);
            _history.Move(-1);
        }

        public bool CanMoveToNext()
        {
            return _history.CanNext();
        }

        public void MoveToNext()
        {
            if (!_history.CanNext()) return;

            var query = _history.GetNext();
            LoadPage(query);
            _history.Move(+1);
        }

        public void MoveToHistory(KeyValuePair<int, PageHistoryUnit> item)
        {
            var query = _history.GetHistory(item.Key);
            LoadPage(query);
            _history.SetCurrent(item.Key + 1);
        }

        private void LoadPage(PageHistoryUnit unit)
        {
            if (unit.IsEmpty()) return;

            if (BookOperation.Current.Address == unit.BookAddress)
            {
                BookOperation.Current.JumpPage(unit.PageName);
            }
            else
            {
                var option = BookLoadOption.SkipSamePlace;
                BookHub.Current.RequestLoad(unit.BookAddress, unit.PageName, option, true);
            }
        }

        internal List<KeyValuePair<int, PageHistoryUnit>> GetHistory(int direction, int size)
        {
            return _history.GetHistory(direction, size);
        }
    }
}

