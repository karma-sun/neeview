using System.Windows.Data;


namespace NeeView
{
    public class ToggleMainViewFloatingCommand : CommandElement
    {
        public ToggleMainViewFloatingCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleMainViewFloating;
            this.MenuText = Properties.Resources.CommandToggleMainViewFloatingMenu;
            this.Note = Properties.Resources.CommandToggleMainViewFloatingNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MainViewConfig.IsFloating)) { Source = Config.Current.MainView, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.MainView.IsFloating ? Properties.Resources.ToggleMainViewFloatingOff : Properties.Resources.ToggleMainViewFloatingOn;
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.MainView.IsFloating = !Config.Current.MainView.IsFloating;
        }
    }
}