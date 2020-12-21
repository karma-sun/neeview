using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleSideBarCommand : CommandElement
    {
        public ToggleVisibleSideBarCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PanelsConfig.IsSideBarEnabled)) { Source = Config.Current.Panels };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Panels.IsSideBarEnabled ? Properties.Resources.ToggleVisibleSideBarCommand_Off : Properties.Resources.ToggleVisibleSideBarCommand_On;
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.Panels.IsSideBarEnabled = !Config.Current.Panels.IsSideBarEnabled;
        }
    }
}
