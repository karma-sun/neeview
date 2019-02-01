using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 履歴操作
    /// </summary>
    public class BookHistoryCommand 
    {
        public static BookHistoryCommand Current { get; private set; }

        public BookHistoryCommand()
        {
            Current = this;
        }

        // 履歴を戻ることができる？
        public bool CanPrevHistory()
        {
            var node = BookHistoryCollection.Current.FindNode(BookHub.Current.Address);

            // 履歴が存在するなら真
            if (node == null && BookHistoryCollection.Current.Count > 0) return true;

            // 現在の履歴位置より古いものがあれば真。リストと履歴の方向は逆
            return node != null && node.Next != null;
        }

        // 履歴を戻る
        public void PrevHistory()
        {
            if (BookHub.Current.IsLoading || BookHistoryCollection.Current.Count <= 0) return;

            var node = BookHistoryCollection.Current.FindNode(BookHub.Current.Address);
            var previous = node == null ? BookHistoryCollection.Current.First.Value : node?.Next.Value; // リストと履歴の方向は逆

            if (previous != null)
            {
                BookHub.Current.RequestLoad(previous.Place, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyHistoryTerminal);
            }
        }

        // 履歴を進めることができる？
        public bool CanNextHistory()
        {
            var node = BookHistoryCollection.Current.FindNode(BookHub.Current.Address);
            return (node != null && node.Previous != null); // リストと履歴の方向は逆
        }

        // 履歴を進める
        public void NextHistory()
        {
            if (BookHub.Current.IsLoading) return;

            var unit = BookHistoryCollection.Current.FindNode(BookHub.Current.Address);
            var next = unit?.Previous; // リストと履歴の方向は逆 
            if (next != null)
            {
                BookHub.Current.RequestLoad(next.Value.Place, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.NotifyHistoryLastest);
            }
        }
    }
}
