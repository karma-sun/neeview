using NeeView.Collections;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// BookHubの履歴。
    /// 本を開いた順番そのままを記録している。
    /// </summary>
    public class BookHubHistory
    {
        static BookHubHistory() => Current = new BookHubHistory();
        public static BookHubHistory Current { get; }

        private readonly HistoryCollection<QueryPath> _history = new HistoryCollection<QueryPath>();

        public void Add(QueryPath query)
        {
            _history.TrimEnd(null);

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
            LoadBook(query);
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
            LoadBook(query);
            _history.Move(+1);
        }

        public void MoveToHistory(KeyValuePair<int, QueryPath> item)
        {
            var query = _history.GetHistory(item.Key);
            LoadBook(query);
            _history.SetCurrent(item.Key + 1);
        }

        private void LoadBook(QueryPath query)
        {
            if (query == null) return;
            var option = BookLoadOption.IsBook | BookLoadOption.SkipSamePlace;
            BookHub.Current.RequestLoad(query.SimplePath, null, option, true);
        }

        internal List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return _history.GetHistory(direction, size);
        }
    }
}

