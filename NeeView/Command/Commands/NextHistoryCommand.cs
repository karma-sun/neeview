namespace NeeView
{
    public class NextHistoryCommand : CommandElement
    {
        public NextHistoryCommand()
        {
            this.Group = Properties.Resources.CommandGroup_BookMove;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return HistoryList.Current.CanNextHistory();
        }

        public override void Execute(object sender, CommandContext e)
        {
            HistoryList.Current.NextHistory();
        }
    }
}
