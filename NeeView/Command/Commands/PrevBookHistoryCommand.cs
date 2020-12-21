namespace NeeView
{
    public class PrevBookHistoryCommand : CommandElement
    {
        public PrevBookHistoryCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookMove;
            this.ShortCutKey = "Alt+Left";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookHubHistory.Current.CanMoveToPrevious();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookHubHistory.Current.MoveToPrevious();
        }
    }
}
