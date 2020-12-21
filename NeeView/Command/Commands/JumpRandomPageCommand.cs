namespace NeeView
{
    public class JumpRandomPageCommand : CommandElement
    {
        public JumpRandomPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.JumpRandomPage(this);
        }
    }
}
