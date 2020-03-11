namespace NeeView
{
    public class JumpPageCommand : CommandElement
    {
        public JumpPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandJumpPage;
            this.Note = Properties.Resources.CommandJumpPageNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument(typeof(int), "@CommandJumpPageArgument")]
        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.JumpPage(arg as int?);
        }
    }
}
