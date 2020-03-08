namespace NeeView
{
    public class CopyImageCommand : CommandElement
    {
        public CopyImageCommand() : base("CopyImage")
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandCopyImage;
            this.MenuText = Properties.Resources.CommandCopyImageMenu;
            this.Note = Properties.Resources.CommandCopyImageNote;
            this.ShortCutKey = "Ctrl+Shift+C";
            this.IsShowMessage = true;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ContentCanvas.Current.CanCopyImageToClipboard();
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.CopyImageToClipboard();
        }
    }
}
