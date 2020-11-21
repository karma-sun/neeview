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

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.Script.IsScriptFolderEnabled;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.OpenScriptsFolder();
        }
    }
}
