namespace NeeView
{
    public class CopyFileCommand : CommandElement
    {
        public CopyFileCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandCopyFile;
            this.MenuText = Properties.Resources.CommandCopyFileMenu;
            this.Note = Properties.Resources.CommandCopyFileNote;
            this.ShortCutKey = "Ctrl+C";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new CopyFileCommandParameter());

        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookOperation.Current.CanOpenFilePlace();
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.CopyToClipboard((CopyFileCommandParameter)param);
        }
    }
}
