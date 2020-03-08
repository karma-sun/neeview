namespace NeeView
{
    public class OpenFilePlaceCommand : CommandElement
    {
        public OpenFilePlaceCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandOpenFilePlace;
            this.Note = Properties.Resources.CommandOpenFilePlaceNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.OpenFilePlace();
        }
    }
}
