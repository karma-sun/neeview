namespace NeeView
{
    public class LoupeOffCommand : CommandElement
    {
        public LoupeOffCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.SetLoupeMode(false);
        }
    }
}
