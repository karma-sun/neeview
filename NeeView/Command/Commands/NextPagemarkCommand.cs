namespace NeeView
{
    public class NextPagemarkCommand : CommandElement
    {
        public NextPagemarkCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandNextPagemark;
            this.Note = Properties.Resources.CommandNextPagemarkNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            PagemarkList.Current.NextPagemark();
        }
    }
}
