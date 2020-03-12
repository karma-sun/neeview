namespace NeeView
{
    public class OpenExternalAppCommand : CommandElement
    {
        public OpenExternalAppCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandOpenApplication;
            this.Note = Properties.Resources.CommandOpenApplicationNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.OpenApplication();
        }
    }
}
