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

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookHistoryCollection.Current.Clear();
        }
    }
}
