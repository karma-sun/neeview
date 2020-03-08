namespace NeeView
{
    public class FocusMainViewCommand : CommandElement
    {
        public FocusMainViewCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandFocusMainView;
            this.MenuText = Properties.Resources.CommandFocusMainViewMenu;
            this.Note = Properties.Resources.CommandFocusMainViewNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new FocusMainViewCommandParameter() { NeedClosePanels = false });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.FocusMainView((FocusMainViewCommandParameter)param, option.HasFlag(CommandOption.ByMenu));
        }
    }
}
