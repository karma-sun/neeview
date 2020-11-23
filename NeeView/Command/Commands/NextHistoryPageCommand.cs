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

        public override bool CanExecute(object sender, CommandContext e)
        {
            return PageHistory.Current.CanMoveToNext();
        }

        public override void Execute(object sender, CommandContext e)
        {
            PageHistory.Current.MoveToNext();
        }
    }
}
