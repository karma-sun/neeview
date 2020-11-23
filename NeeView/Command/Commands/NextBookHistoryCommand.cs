namespace NeeView
{
    public class NextBookHistoryCommand : CommandElement
    {
        public NextBookHistoryCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookMove;
            this.Text = Properties.Resources.CommandNextBookHistory;
            this.Note = Properties.Resources.CommandNextBookHistoryNote;
            this.ShortCutKey = "Alt+Right";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookHubHistory.Current.CanMoveToNext();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookHubHistory.Current.MoveToNext();
        }
    }
}
