namespace NeeView
{
    public class PrevFolderPageCommand : CommandElement
    {
        public PrevFolderPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
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
