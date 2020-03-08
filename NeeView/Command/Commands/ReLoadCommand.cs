namespace NeeView
{
    public class ReLoadCommand : CommandElement
    {
        public ReLoadCommand() : base("ReLoad")
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandReLoad;
            this.Note = Properties.Resources.CommandReLoadNote;
            this.MouseGesture = "UD";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookHub.Current.CanReload();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookHub.Current.RequestReLoad();
        }
    }
}
