using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleViewFlipHorizontalCommand : CommandElement
    {
        public ToggleViewFlipHorizontalCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleViewFlipHorizontal;
            this.Note = Properties.Resources.CommandToggleViewFlipHorizontalNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(DragTransform.IsFlipHorizontal)) { Source = ViewComponentProvider.Current.GetViewComponent().DragTransform, Mode = BindingMode.OneWay };
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                ViewComponentProvider.Current.GetViewController(sender).FlipHorizontal(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                ViewComponentProvider.Current.GetViewController(sender).ToggleFlipHorizontal();
            }
        }
    }
}
