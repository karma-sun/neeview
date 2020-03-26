using System.Windows.Data;


namespace NeeView
{
    public class ToggleHideMenuCommand : CommandElement
    {
        public ToggleHideMenuCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleHideMenu;
            this.MenuText = Properties.Resources.CommandToggleHideMenuMenu;
            this.Note = Properties.Resources.CommandToggleHideMenuNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MenuBarConfig.IsHideMenu)) { Source = Config.Current.Layout.MenuBar };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.Layout.MenuBar.IsHideMenu ? Properties.Resources.CommandToggleHideMenuOff : Properties.Resources.CommandToggleHideMenuOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.ToggleHideMenu();
        }
    }
}
