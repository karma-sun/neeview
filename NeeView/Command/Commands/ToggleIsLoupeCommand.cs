using System;
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

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return MouseInput.Current.IsLoupeMode ? Properties.Resources.CommandToggleIsLoupeOff : Properties.Resources.CommandToggleIsLoupeOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MouseInput.Current.IsLoupeMode = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                MouseInput.Current.IsLoupeMode = !MouseInput.Current.IsLoupeMode;
            }
        }
    }
}
