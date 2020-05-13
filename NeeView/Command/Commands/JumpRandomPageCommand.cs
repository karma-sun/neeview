namespace NeeView
{
    public class JumpRandomPageCommand : CommandElement
    {
        public JumpRandomPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandJumpRandomPage;
            this.Note = Properties.Resources.CommandJumpRandomPageNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.JumpRandomPage(this);
        }
    }
}
