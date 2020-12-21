using System.Windows.Data;


namespace NeeView
{
    public class ToggleHidePanelCommand : CommandElement
    {
        public ToggleHidePanelCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PanelsConfig.IsHidePanel)) { Source = Config.Current.Panels };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Panels.IsHidePanel ? Properties.Resources.ToggleHidePanelCommand_Off : Properties.Resources.ToggleHidePanelCommand_On;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.ToggleHidePanel();
        }
    }
}
