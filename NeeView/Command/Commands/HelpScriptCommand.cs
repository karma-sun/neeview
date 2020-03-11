namespace NeeView
{
    public class HelpScriptCommand : CommandElement
    {
        public HelpScriptCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandHelpScript;
            this.MenuText = Properties.Resources.CommandHelpScriptMenu;
            this.Note = Properties.Resources.CommandHelpScriptNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            CommandTable.Current.OpenScriptHelp();
        }
    }
}
