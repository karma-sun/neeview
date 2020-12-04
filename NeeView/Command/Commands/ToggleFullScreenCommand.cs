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
            var windowStateManager =  MainWindow.Current.WindowStateManager;
            return new Binding(nameof(windowStateManager.IsFullScreen)) { Source = windowStateManager, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            var windowStateManager = MainWindow.Current.WindowStateManager;
            return windowStateManager.IsFullScreen ? Properties.Resources.CommandToggleFullScreenOff : Properties.Resources.CommandToggleFullScreenOn;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.ToggleWindowFullScreen(sender);
        }
    }
}
