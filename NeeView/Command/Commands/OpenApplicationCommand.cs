namespace NeeView
{
    public class OpenApplicationCommand : CommandElement
    {
        public OpenApplicationCommand() : base("OpenApplication")
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandOpenApplication;
            this.Note = Properties.Resources.CommandOpenApplicationNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.OpenApplication();
        }
    }
}
