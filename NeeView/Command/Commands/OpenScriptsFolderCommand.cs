namespace NeeView
{
    public class OpenScriptsFolderCommand : CommandElement
    {
        public OpenScriptsFolderCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return Config.Current.Script.IsScriptFolderEnabled;
        }

        public override void Execute(object sender, CommandContext e)
        {
            CommandTable.Current.ScriptManager.OpenScriptsFolder();
        }
    }
}
