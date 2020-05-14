namespace NeeView
{
    public class NextHistoryPageCommand : CommandElement
    {
        public NextHistoryPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextPageHistory;
            this.Note = Properties.Resources.CommandNextPageHistoryNote;
            this.ShortCutKey = "Shift+Back";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return PageHistory.Current.CanMoveToNext();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            PageHistory.Current.MoveToNext();
        }
    }
}
