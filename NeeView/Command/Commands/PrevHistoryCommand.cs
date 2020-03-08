namespace NeeView
{
    public class PrevHistoryCommand : CommandElement
    {
        public PrevHistoryCommand() : base("PrevHistory")
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevHistory;
            this.Note = Properties.Resources.CommandPrevHistoryNote;
            this.ShortCutKey = "Back";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookHistoryCommand.Current.CanPrevHistory();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHistoryCommand.Current.PrevHistory();
        }
    }
}
