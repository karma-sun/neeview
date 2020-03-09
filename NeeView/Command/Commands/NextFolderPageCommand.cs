namespace NeeView
{
    public class NextFolderPageCommand : CommandElement
    {
        public NextFolderPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextFolderPage;
            this.Note = Properties.Resources.CommandNextFolderPageNote;
            this.IsShowMessage = true;
            this.PairPartner = "PrevFolderPage";

            // PrevFolderPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return null;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.NextFolderPage(this.IsShowMessage);
        }
    }
}
