namespace NeeView
{
    public class NextHistoryPageCommand : CommandElement
    {
        public NextHistoryPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
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
