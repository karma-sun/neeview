namespace NeeView
{
    public class OpenScriptsFolderCommand : CommandElement
    {
        public OpenScriptsFolderCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenSecriptsFolder;
            this.Note = Properties.Resources.CommandOpenSecriptsFolderNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.OpenScriptsFolder();
        }
    }
}
