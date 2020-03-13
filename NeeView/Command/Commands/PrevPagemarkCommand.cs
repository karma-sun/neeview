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

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            PagemarkList.Current.PrevPagemark();
        }
    }
}
