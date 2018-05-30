using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class BookHistoryCommand 
    {
        private BookHistoryCollection _bookHistory;
        private BookHub _bookHub;

        //
        public BookHistoryCommand(BookHistoryCollection bookHistory, BookHub bookHub)
        {
            _bookHistory = bookHistory;
            _bookHub = bookHub;
        }

        // 履歴を戻ることができる？
        public bool CanPrevHistory()
        {
            var node = _bookHistory.FindNode(_bookHub.Address);

            // 履歴が存在するなら真
            if (node == null && BookHistoryCollection.Current.Count > 0) return true;

            // 現在の履歴位置より古いものがあれば真。リストと履歴の方向は逆
            return node != null && node.Next != null;
        }

        // 履歴を戻る
        public void PrevHistory()
        {
            if (_bookHub.IsLoading || _bookHistory.Count <= 0) return;

            var node = _bookHistory.FindNode(_bookHub.Address);
            var previous = node == null ? BookHistoryCollection.Current.First.Value : node?.Next.Value; // リストと履歴の方向は逆

            if (previous != null)
            {
                _bookHub.RequestLoad(previous.Place, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyHistoryTerminal);
            }
        }

        // 履歴を進めることができる？
        public bool CanNextHistory()
        {
            var node = _bookHistory.FindNode(_bookHub.Address);
            return (node != null && node.Previous != null); // リストと履歴の方向は逆
        }

        // 履歴を進める
        public void NextHistory()
        {
            if (_bookHub.IsLoading) return;

            var unit = _bookHistory.FindNode(_bookHub.Address);
            var next = unit?.Previous; // リストと履歴の方向は逆 
            if (next != null)
            {
                _bookHub.RequestLoad(next.Value.Place, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyHistoryLastest);
            }
        }
    }
}
