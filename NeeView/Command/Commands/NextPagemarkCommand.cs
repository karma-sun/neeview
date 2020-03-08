namespace NeeView
{
    public class NextPagemarkCommand : CommandElement
    {
        public NextPagemarkCommand() : base("NextPagemark")
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandNextPagemark;
            this.Note = Properties.Resources.CommandNextPagemarkNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            PagemarkList.Current.NextPagemark();
        }
    }
}
