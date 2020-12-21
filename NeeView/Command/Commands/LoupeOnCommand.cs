namespace NeeView
{
    public class LoupeOnCommand : CommandElement
    {
        public LoupeOnCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.SetLoupeMode(true);
        }
    }
}
