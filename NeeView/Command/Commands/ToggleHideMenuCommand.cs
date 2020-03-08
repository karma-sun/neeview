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
            return new Binding(nameof(MainWindowModel.Current.IsHideMenu)) { Source = MainWindowModel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return MainWindowModel.Current.IsHideMenu ? Properties.Resources.CommandToggleHideMenuOff : Properties.Resources.CommandToggleHideMenuOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.ToggleHideMenu();
        }
    }
}
