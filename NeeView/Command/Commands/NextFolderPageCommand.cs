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

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return null;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.NextFolderPage(this, this.IsShowMessage);
        }
    }
}
