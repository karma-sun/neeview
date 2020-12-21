namespace NeeView
{
    public class StretchWindowCommand : CommandElement
    {
        public StretchWindowCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.StretchWindow();
        }
    }
}