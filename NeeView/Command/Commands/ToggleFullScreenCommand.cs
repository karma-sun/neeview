using System.Windows.Data;


namespace NeeView
{
    public class ToggleFullScreenCommand : CommandElement
    {
        public ToggleFullScreenCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleFullScreen;
            this.MenuText = Properties.Resources.CommandToggleFullScreenMenu;
            this.Note = Properties.Resources.CommandToggleFullScreenNote;
            this.ShortCutKey = "F11";
            this.MouseGesture = "U";
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowShape.Current.IsFullScreen)) { Source = WindowShape.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return WindowShape.Current.IsFullScreen ? Properties.Resources.CommandToggleFullScreenOff : Properties.Resources.CommandToggleFullScreenOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            WindowShape.Current.ToggleFullScreen();
        }
    }
}
