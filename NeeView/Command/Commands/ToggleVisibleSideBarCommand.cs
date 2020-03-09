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
            return new Binding(nameof(SidePanel.IsSideBarVisible)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return SidePanel.Current.IsSideBarVisible ? Properties.Resources.CommandToggleVisibleSideBarOff : Properties.Resources.CommandToggleVisibleSideBarOn;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            SidePanel.Current.IsSideBarVisible = !SidePanel.Current.IsSideBarVisible;
        }
    }
}
