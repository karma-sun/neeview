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
            if (query != this._history.GetCurrent())
            {
                this._history.Add(query);
            }
        }

        public bool CanMoveToPrevious()
        {
            return this._history.CanPrevious();
        }

        public void MoveToPrevious()
        {
            if (!this._history.CanPrevious()) return;

            var query = this._history.GetPrevious();
            LoadBook(query);
            this._history.Move(-1);
        }

        public bool CanMoveToNext()
        {
            return this._history.CanNext();
        }

        public void MoveToNext()
        {
            if (!this._history.CanNext()) return;

            var query = this._history.GetNext();
            LoadBook(query);
            this._history.Move(+1);
        }

        public void MoveToHistory(KeyValuePair<int, QueryPath> item)
        {
            var query = this._history.GetHistory(item.Key);
            LoadBook(query);
            this._history.SetCurrent(item.Key + 1);
        }

        private void LoadBook(QueryPath query)
        {
            var option = BookLoadOption.IsBook | BookLoadOption.SkipSamePlace;
            BookHub.Current.RequestLoad(query.SimplePath, null, option, true);
        }

        internal List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return this._history.GetHistory(direction, size);
        }
    }
}

