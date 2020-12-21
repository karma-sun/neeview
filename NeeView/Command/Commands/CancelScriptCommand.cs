namespace NeeView
{
    public class CancelScriptCommand : CommandElement
    {
        public CancelScriptCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Script;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            CommandTable.Current.CancelScript();
        }
    }
}
