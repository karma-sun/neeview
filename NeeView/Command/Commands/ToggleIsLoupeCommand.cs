using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsLoupeCommand : CommandElement
    {
        public ToggleIsLoupeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleIsLoupe;
            this.MenuText = Properties.Resources.CommandToggleIsLoupeMenu;
            this.Note = Properties.Resources.CommandToggleIsLoupeNote;
            this.IsShowMessage = false;
        }
        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MouseInput.Current.IsLoupeMode)) { Mode = BindingMode.OneWay, Source = MouseInput.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object arg, CommandOption option)
        {
            return MouseInput.Current.IsLoupeMode ? Properties.Resources.CommandToggleIsLoupeOff : Properties.Resources.CommandToggleIsLoupeOn;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            MouseInput.Current.IsLoupeMode = !MouseInput.Current.IsLoupeMode;
        }
    }
}
