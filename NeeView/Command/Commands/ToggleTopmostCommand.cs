using System.Windows.Data;


namespace NeeView
{
    public class ToggleTopmostCommand : CommandElement
    {
        public ToggleTopmostCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleTopmost;
            this.MenuText = Properties.Resources.CommandToggleTopmostMenu;
            this.Note = Properties.Resources.CommandToggleTopmostNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowConfig.IsTopmost)) { Source = Config.Current.Window, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Window.IsTopmost ? Properties.Resources.CommandToggleTopmostOff : Properties.Resources.CommandToggleTopmostOn;
        }

        public override void Execute(object sender, CommandContext e)
        {
            WindowShape.Current.ToggleTopmost();
        }
    }
}
