using System.Windows.Data;


namespace NeeView
{
    public class ToggleTopmostCommand : CommandElement
    {
        public ToggleTopmostCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowConfig.IsTopmost)) { Source = Config.Current.Window, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Window.IsTopmost ? Properties.Resources.ToggleTopmostCommand_Off : Properties.Resources.ToggleTopmostCommand_On;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ToggleTopmost(sender);
        }
    }
}
