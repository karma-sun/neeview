namespace NeeView
{
    public class TouchEmulateCommand : CommandElement
    {
        public TouchEmulateCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.TouchInputEmutrate(sender);
        }
    }
}
