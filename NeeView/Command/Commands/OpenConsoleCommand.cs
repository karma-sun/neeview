namespace NeeView
{
    public class OpenConsoleCommand : CommandElement
    {
        public OpenConsoleCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ConsoleWindowManager.Current.OpenWindow();
        }
    }
}
