using System.Windows.Data;


namespace NeeView
{
    public class ToggleTopmostCommand : CommandElement
    {
        public ToggleTopmostCommand() : base(CommandType.ToggleTopmost)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleTopmost;
            this.MenuText = Properties.Resources.CommandToggleTopmostMenu;
            this.Note = Properties.Resources.CommandToggleTopmostNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowShape.IsTopmost)) { Source = WindowShape.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return WindowShape.Current.IsTopmost ? Properties.Resources.CommandToggleTopmostOff : Properties.Resources.CommandToggleTopmostOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            WindowShape.Current.ToggleTopmost();
        }
    }
}
