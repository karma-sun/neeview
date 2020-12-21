namespace NeeView
{
    public class PrevPagemarkCommand : CommandElement
    {
        public PrevPagemarkCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Pagemark;
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
