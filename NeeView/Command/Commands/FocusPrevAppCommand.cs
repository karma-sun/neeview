namespace NeeView
{
    public class FocusPrevAppCommand : CommandElement
    {
        public FocusPrevAppCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.ShortCutKey = "Ctrl+Shift+Tab";
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            WindowActivator.Current.NextActivate(-1);
        }
    }

}
