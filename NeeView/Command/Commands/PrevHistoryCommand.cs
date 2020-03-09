namespace NeeView
{
    public class PrevHistoryCommand : CommandElement
    {
        public PrevHistoryCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevHistory;
            this.Note = Properties.Resources.CommandPrevHistoryNote;
            this.ShortCutKey = "Back";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookHistoryCommand.Current.CanPrevHistory();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookHistoryCommand.Current.PrevHistory();
        }
    }
}
