using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleSideBarCommand : CommandElement
    {
        public ToggleVisibleSideBarCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleVisibleSideBar;
            this.MenuText = Properties.Resources.CommandToggleVisibleSideBarMenu;
            this.Note = Properties.Resources.CommandToggleVisibleSideBarNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PanelsConfig.IsSideBarEnabled)) { Source = Config.Current.Panels };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Panels.IsSideBarEnabled ? Properties.Resources.CommandToggleVisibleSideBarOff : Properties.Resources.CommandToggleVisibleSideBarOn;
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.Panels.IsSideBarEnabled = !Config.Current.Panels.IsSideBarEnabled;
        }
    }
}
