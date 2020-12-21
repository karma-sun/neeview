namespace NeeView
{
    public class FocusNextAppCommand : CommandElement
    {
        public FocusNextAppCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.ShortCutKey = "Ctrl+Tab";
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            WindowActivator.Current.NextActivate(+1);
        }
    }

}
