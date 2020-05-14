namespace NeeView
{
    public class NextHistoryCommand : CommandElement
    {
        public NextHistoryCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandNextHistory;
            this.Note = Properties.Resources.CommandNextHistoryNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return HistoryList.Current.CanNextHistory();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            HistoryList.Current.NextHistory();
        }
    }
}
