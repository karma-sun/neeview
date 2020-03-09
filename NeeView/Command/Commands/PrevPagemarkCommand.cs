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

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            PagemarkList.Current.PrevPagemark();
        }
    }
}
