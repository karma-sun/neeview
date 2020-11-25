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
            return new Binding(nameof(DragTransform.IsFlipVertical)) { Source = ViewComponentProvider.Current.GetViewComponent().DragTransform, Mode = BindingMode.OneWay };
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                ViewComponentProvider.Current.GetViewController(sender).FlipVertical(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                ViewComponentProvider.Current.GetViewController(sender).ToggleFlipVertical();
            }
        }
    }
}
