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

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            PagemarkList.Current.NextPagemark();
        }
    }
}
