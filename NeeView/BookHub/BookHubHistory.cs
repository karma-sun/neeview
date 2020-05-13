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

        private const int _historyCapacity = 100;
        private readonly HistoryLimitedCollection<QueryPath> _history = new HistoryLimitedCollection<QueryPath>(_historyCapacity);


        public void Add(object sender, QueryPath query)
        {
            // NOTE: 履歴操からの操作では履歴を変更しない
            if (sender == this) return;

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
            var option = BookLoadOption.IsBook | BookLoadOption.SkipSamePlace | BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe;
            BookHub.Current.RequestLoad(this, query.SimplePath, null, option, true);
        }

        internal List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return _history.GetHistory(direction, size);
        }
    }
}

