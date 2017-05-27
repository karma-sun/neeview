using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookHistoryCommand 
    {
        private BookHistory _bookHistory;
        private BookHub _bookHub;

        //
        public BookHistoryCommand(BookHistory bookHistory, BookHub bookHub)
        {
            _bookHistory = bookHistory;
            _bookHub = bookHub;
        }

        // 履歴を戻ることができる？
        public bool CanPrevHistory()
        {
            var unit = _bookHistory.Find(_bookHub.Address);
            // 履歴が存在するなら真
            if (unit == null && BookHistory.Current.Count > 0) return true;
            // 現在の履歴位置より古いものがあれば真。リストと履歴の方向は逆
            return unit?.HistoryNode != null && unit.HistoryNode.Next != null;
        }

        // 履歴を戻る
        public void PrevHistory()
        {
            if (_bookHub.IsLoading || _bookHistory.Count <= 0) return;

            var unit = _bookHistory.Find(_bookHub.Address);
            var previous = unit == null ? BookHistory.Current.First : unit.HistoryNode?.Next.Value; // リストと履歴の方向は逆

            if (previous != null)
            {
                _bookHub.RequestLoad(previous.Memento.Place, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "これより古い履歴はありません");
            }
        }

        // 履歴を進めることができる？
        public bool CanNextHistory()
        {
            var unit = _bookHistory.Find(_bookHub.Address);
            return (unit?.HistoryNode != null && unit.HistoryNode.Previous != null); // リストと履歴の方向は逆
        }

        // 履歴を進める
        public void NextHistory()
        {
            if (_bookHub.IsLoading) return;

            var unit = _bookHistory.Find(_bookHub.Address);
            var next = unit?.HistoryNode?.Previous; // リストと履歴の方向は逆
            if (next != null)
            {
                _bookHub.RequestLoad(next.Value.Memento.Place, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, "最新の履歴です");
            }
        }
    }
}
