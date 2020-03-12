using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleTitleBarCommand : CommandElement
    {
        public ToggleVisibleTitleBarCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleVisibleTitleBar;
            this.MenuText = Properties.Resources.CommandToggleVisibleTitleBarMenu;
            this.Note = Properties.Resources.CommandToggleVisibleTitleBarNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(WindowShape.IsCaptionVisible)) { Source = WindowShape.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return WindowShape.Current.IsCaptionVisible ? Properties.Resources.CommandToggleVisibleTitleBarOff : Properties.Resources.CommandToggleVisibleTitleBarOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            WindowShape.Current.ToggleCaptionVisible();
        }
    }
}
