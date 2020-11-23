namespace NeeView
{
    public class PasteCommand : CommandElement
    {
        public PasteCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFile;
            this.Text = Properties.Resources.CommandPaste;
            this.MenuText = Properties.Resources.CommandPasteMenu;
            this.Note = Properties.Resources.CommandPasteNote;
            this.ShortCutKey = "Ctrl+V";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return ContentDropManager.Current.CanLoadFromClipboard();
        }

        public override void Execute(object sender, CommandContext e)
        {
            ContentDropManager.Current.LoadFromClipboard();
        }
    }
}
