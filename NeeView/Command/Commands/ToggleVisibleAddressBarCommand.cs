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
            return new Binding(nameof(MainWindowModel.Current.IsVisibleAddressBar)) { Source = MainWindowModel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return MainWindowModel.Current.IsVisibleAddressBar ? Properties.Resources.CommandToggleVisibleAddressBarOff : Properties.Resources.CommandToggleVisibleAddressBarOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.ToggleVisibleAddressBar();
        }
    }
}
