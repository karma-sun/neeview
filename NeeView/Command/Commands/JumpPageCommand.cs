namespace NeeView
{
    public class JumpPageCommand : CommandElement
    {
        public JumpPageCommand() : base(CommandType.JumpPage)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandJumpPage;
            this.Note = Properties.Resources.CommandJumpPageNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.JumpPage();
        }
    }
}
