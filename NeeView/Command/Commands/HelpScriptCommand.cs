namespace NeeView
{
    public class HelpScriptCommand : CommandElement
    {
        public HelpScriptCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            new ScriptManual().OpenScriptManual();
        }
    }
}
