namespace NeeView
{
    public class NextBookHistoryCommand : CommandElement
    {
        public NextBookHistoryCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextBookHistory;
            this.Note = Properties.Resources.CommandNextBookHistoryNote;
            this.ShortCutKey = "Alt+Right";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookHubHistory.Current.CanMoveToNext();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookHubHistory.Current.MoveToNext();
        }
    }
}
