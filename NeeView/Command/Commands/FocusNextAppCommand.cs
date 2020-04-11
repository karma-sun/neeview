namespace NeeView
{
    public class FocusNextAppCommand : CommandElement
    {
        public FocusNextAppCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandFocusNextApp;
            this.MenuText = Properties.Resources.CommandFocusNextAppMenu;
            this.Note = Properties.Resources.CommandFocusNextAppNote;
            this.ShortCutKey = "Ctrl+Tab";
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ProcessActivator.NextActivate(+1);
        }
    }

}
