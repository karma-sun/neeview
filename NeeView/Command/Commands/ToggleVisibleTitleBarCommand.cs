using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleTitleBarCommand : CommandElement
    {
        public ToggleVisibleTitleBarCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowConfig.IsCaptionVisible)) { Source = Config.Current.Window, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Window.IsCaptionVisible ? Properties.Resources.ToggleVisibleTitleBarCommand_Off : Properties.Resources.ToggleVisibleTitleBarCommand_On;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindow.Current.ToggleCaptionVisible();
        }
    }
}
