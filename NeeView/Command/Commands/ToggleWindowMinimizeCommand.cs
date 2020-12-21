namespace NeeView
{
    public class ToggleWindowMinimizeCommand : CommandElement
    {
        public ToggleWindowMinimizeCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ToggleWindowMinimize(sender);
        }
    }
}
