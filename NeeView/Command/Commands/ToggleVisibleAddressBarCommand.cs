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
            return new Binding(nameof(MenuBarConfig.IsVisibleAddressBar)) { Source = Config.Current.MenuBar };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.MenuBar.IsVisibleAddressBar ? Properties.Resources.CommandToggleVisibleAddressBarOff : Properties.Resources.CommandToggleVisibleAddressBarOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.ToggleVisibleAddressBar();
        }
    }
}
