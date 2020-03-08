namespace NeeView
{
    public class NextFolderPageCommand : CommandElement
    {
        public NextFolderPageCommand() : base("NextFolderPage")
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextFolderPage;
            this.Note = Properties.Resources.CommandNextFolderPageNote;
            this.IsShowMessage = true;
            this.PairPartner = "PrevFolderPage";

            // PrevFolderPage
            this.ParameterSource = new CommandParameterSource(new ReversibleCommandParameter());
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return null;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.NextFolderPage(this.IsShowMessage);
        }
    }
}
