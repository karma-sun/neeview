namespace NeeView
{
    public class PrevPagemarkCommand : CommandElement
    {
        public PrevPagemarkCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandPrevPagemark;
            this.Note = Properties.Resources.CommandPrevPagemarkNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            PagemarkList.Current.PrevPagemark();
        }
    }
}
