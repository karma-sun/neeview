namespace NeeView
{
    public class PrevFolderPageCommand : CommandElement
    {
        public PrevFolderPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevFolderPage;
            this.Note = Properties.Resources.CommandPrevFolderPageNote;
            this.IsShowMessage = true;
            this.PairPartner = "NextFolderPage";

            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return null;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.PrevFolderPage(this.IsShowMessage);
        }
    }
}
