namespace NeeView
{
    public class PrevHistoryPageCommand : CommandElement
    {
        public PrevHistoryPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
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
