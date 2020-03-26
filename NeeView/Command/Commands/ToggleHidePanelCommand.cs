using System.Windows.Data;


namespace NeeView
{
    public class ToggleHidePanelCommand : CommandElement
    {
        public ToggleHidePanelCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleHidePanel;
            this.MenuText = Properties.Resources.CommandToggleHidePanelMenu;
            this.Note = Properties.Resources.CommandToggleHidePanelNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(PanelsConfig.IsHidePanel)) { Source = Config.Current.Layout.Panels };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.Layout.Panels.IsHidePanel ? Properties.Resources.CommandToggleHidePanelOff : Properties.Resources.CommandToggleHidePanelOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.ToggleHidePanel();
        }
    }
}
