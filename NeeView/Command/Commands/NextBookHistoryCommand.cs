namespace NeeView
{
    public class NextBookHistoryCommand : CommandElement
    {
        public NextBookHistoryCommand() : base("NextBookHistory")
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextBookHistory;
            this.Note = Properties.Resources.CommandNextBookHistoryNote;
            this.ShortCutKey = "Alt+Right";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookHubHistory.Current.CanMoveToNext();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHubHistory.Current.MoveToNext();
        }
    }
}
