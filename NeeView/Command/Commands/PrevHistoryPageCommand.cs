namespace NeeView
{
    public class PrevHistoryPageCommand : CommandElement
    {
        public PrevHistoryPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevPageHistory;
            this.Note = Properties.Resources.CommandPrevPageHistoryNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return PageHistory.Current.CanMoveToPrevious();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            PageHistory.Current.MoveToPrevious();
        }
    }
}
