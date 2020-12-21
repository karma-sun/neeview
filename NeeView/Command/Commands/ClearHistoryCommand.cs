namespace NeeView
{
    public class ClearHistoryCommand : CommandElement
    {
        public ClearHistoryCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookHistoryCollection.Current.Clear();
        }
    }
}
