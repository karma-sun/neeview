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

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookOperation.Current.OpenFilePlace();
        }
    }
}
