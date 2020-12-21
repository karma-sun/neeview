namespace NeeView
{
    public class HelpCommandListCommand : CommandElement
    {
        public HelpCommandListCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            CommandTable.Current.OpenCommandListHelp();
        }
    }
}
