namespace NeeView
{
    public class NextBookHistoryCommand : CommandElement
    {
        public NextBookHistoryCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookMove;
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
