namespace NeeView
{
    public class PrevHistoryPageCommand : CommandElement
    {
        public PrevHistoryPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevPageHistory;
            this.Note = Properties.Resources.CommandPrevPageHistoryNote;
            this.ShortCutKey = "Back";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return PageHistory.Current.CanMoveToPrevious();
        }

        public override void Execute(object sender, CommandContext e)
        {
            PageHistory.Current.MoveToPrevious();
        }
    }
}
