namespace NeeView
{
    public class ToggleWindowMaximizeCommand : CommandElement
    {
        public ToggleWindowMaximizeCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ToggleWindowMaximize(sender);
        }
    }
}
