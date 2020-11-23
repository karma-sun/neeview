using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleAddressBarCommand : CommandElement
    {
        public ToggleVisibleAddressBarCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleVisibleAddressBar;
            this.MenuText = Properties.Resources.CommandToggleVisibleAddressBarMenu;
            this.Note = Properties.Resources.CommandToggleVisibleAddressBarNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MenuBarConfig.IsAddressBarEnabled)) { Source = Config.Current.MenuBar };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.MenuBar.IsAddressBarEnabled ? Properties.Resources.CommandToggleVisibleAddressBarOff : Properties.Resources.CommandToggleVisibleAddressBarOn;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.ToggleVisibleAddressBar();
        }
    }
}
