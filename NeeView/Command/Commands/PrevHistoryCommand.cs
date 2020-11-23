namespace NeeView
{
    public class PrevHistoryCommand : CommandElement
    {
        public PrevHistoryCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandPrevHistory;
            this.Note = Properties.Resources.CommandPrevHistoryNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return HistoryList.Current.CanPrevHistory();
        }

        public override void Execute(object sender, CommandContext e)
        {
            HistoryList.Current.PrevHistory();
        }
    }
}
