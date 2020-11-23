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
            BookOperation.Current.PrevFolderPage(this, this.IsShowMessage);
        }
    }
}
