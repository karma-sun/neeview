namespace NeeView
{
    public class ClearHistoryCommand : CommandElement
    {
        public ClearHistoryCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandClearHistory;
            this.Note = Properties.Resources.CommandClearHistoryNote;
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
