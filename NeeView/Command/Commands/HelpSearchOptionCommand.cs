namespace NeeView
{
    public class HelpSearchOptionCommand : CommandElement
    {
        public HelpSearchOptionCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }
        public override void Execute(object sender, CommandContext e)
        {
            MenuBar.Current.OpenSearchOptionHelp();
        }
    }
}
