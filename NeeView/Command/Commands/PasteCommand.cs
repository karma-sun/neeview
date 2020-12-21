namespace NeeView
{
    public class PasteCommand : CommandElement
    {
        public PasteCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
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
