namespace NeeView
{
    public class PrevBookHistoryCommand : CommandElement
    {
        public PrevBookHistoryCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevBookHistory;
            this.Note = Properties.Resources.CommandPrevBookHistoryNote;
            this.ShortCutKey = "Alt+Left";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookHubHistory.Current.CanMoveToPrevious();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHubHistory.Current.MoveToPrevious();
        }
    }
}
