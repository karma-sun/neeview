namespace NeeView
{
    public class NextHistoryCommand : CommandElement
    {
        public NextHistoryCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextHistory;
            this.Note = Properties.Resources.CommandNextHistoryNote;
            this.ShortCutKey = "Shift+Back";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookHistoryCommand.Current.CanNextHistory();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookHistoryCommand.Current.NextHistory();
        }
    }
}
