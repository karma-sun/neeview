namespace NeeView
{
    public class FocusPrevAppCommand : CommandElement
    {
        public FocusPrevAppCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandFocusPrevApp;
            this.MenuText = Properties.Resources.CommandFocusPrevAppMenu;
            this.Note = Properties.Resources.CommandFocusPrevAppNote;
            this.ShortCutKey = "Ctrl+Shift+Tab";
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ProcessActivator.NextActivate(-1);
        }
    }

}
