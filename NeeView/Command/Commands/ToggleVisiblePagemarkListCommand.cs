using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisiblePagemarkListCommand : CommandElement
    {
        public ToggleVisiblePagemarkListCommand() : base("ToggleVisiblePagemarkList")
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandToggleVisiblePagemarkList;
            this.MenuText = Properties.Resources.CommandToggleVisiblePagemarkListMenu;
            this.Note = Properties.Resources.CommandToggleVisiblePagemarkListNote;
            this.ShortCutKey = "M";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanel.IsVisiblePagemarkList)) { Source = SidePanel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SidePanel.Current.IsVisiblePagemarkList ? Properties.Resources.CommandToggleVisiblePagemarkListOff : Properties.Resources.CommandToggleVisiblePagemarkListOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SidePanel.Current.ToggleVisiblePagemarkList(option.HasFlag(CommandOption.ByMenu));
        }
    }
}
