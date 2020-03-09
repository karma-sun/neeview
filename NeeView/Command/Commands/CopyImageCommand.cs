namespace NeeView
{
    public class CopyImageCommand : CommandElement
    {
        public CopyImageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandCopyImage;
            this.MenuText = Properties.Resources.CommandCopyImageMenu;
            this.Note = Properties.Resources.CommandCopyImageNote;
            this.ShortCutKey = "Ctrl+Shift+C";
            this.IsShowMessage = true;
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return ContentCanvas.Current.CanCopyImageToClipboard();
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            ContentCanvas.Current.CopyImageToClipboard();
        }
    }
}
