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
