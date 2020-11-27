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
            return new Binding(nameof(MouseInput.IsLoupeMode)) { Mode = BindingMode.OneWay, Source = ViewComponent.Current.MouseInput };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return ViewComponent.Current.ViewController.GetLoupeMode() ? Properties.Resources.CommandToggleIsLoupeOff : Properties.Resources.CommandToggleIsLoupeOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                ViewComponent.Current.ViewController.SetLoupeMode(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                ViewComponent.Current.ViewController.ToggleLoupeMode();
            }
        }
    }
}
