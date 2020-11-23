namespace NeeView
{
    public class ReLoadCommand : CommandElement
    {
        public ReLoadCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandReLoad;
            this.Note = Properties.Resources.CommandReLoadNote;
            this.MouseGesture = "UD";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookHub.Current.CanReload();
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookHub.Current.RequestReLoad(this);
        }
    }
}
