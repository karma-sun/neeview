using System.Windows.Data;


namespace NeeView
{
    public class ToggleFullScreenCommand : CommandElement
    {
        public ToggleFullScreenCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
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
            return windowStateManager.IsFullScreen ? Properties.Resources.ToggleFullScreenCommand_Off : Properties.Resources.ToggleFullScreenCommand_On;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ToggleWindowFullScreen(sender);
        }
    }
}
