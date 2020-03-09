namespace NeeView
{
    public class ShowHiddenPanelsCommand : CommandElement
    {
        public ShowHiddenPanelsCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandShowHiddenPanels;
            this.MenuText = Properties.Resources.CommandShowHiddenPanelsMenu;
            this.Note = Properties.Resources.CommandShowHiddenPanelsNote;
            this.TouchGesture = "TouchCenter";
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            MainWindowModel.Current.EnterVisibleLocked();
        }
    }
}
