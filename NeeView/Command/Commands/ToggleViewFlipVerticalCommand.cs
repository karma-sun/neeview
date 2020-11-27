using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleViewFlipVerticalCommand : CommandElement
    {
        public ToggleViewFlipVerticalCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleViewFlipVertical;
            this.Note = Properties.Resources.CommandToggleViewFlipVerticalNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(DragTransform.IsFlipVertical)) { Source = ViewComponent.Current.DragTransform, Mode = BindingMode.OneWay };
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                ViewComponent.Current.ViewController.FlipVertical(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                ViewComponent.Current.ViewController.ToggleFlipVertical();
            }
        }
    }
}
