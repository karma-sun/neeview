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
            return new Binding(nameof(PanelsConfig.IsHidePanel)) { Source = Config.Current.Panels };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Panels.IsHidePanel ? Properties.Resources.CommandToggleHidePanelOff : Properties.Resources.CommandToggleHidePanelOn;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.ToggleHidePanel();
        }
    }
}
