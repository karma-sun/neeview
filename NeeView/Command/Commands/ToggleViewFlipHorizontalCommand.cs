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
            return new Binding(nameof(DragTransform.IsFlipHorizontal)) { Source = DragTransform.Current, Mode = BindingMode.OneWay };
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                DragTransformControl.Current.FlipHorizontal(Convert.ToBoolean(args[0]));
            }
            else
            {
                DragTransformControl.Current.ToggleFlipHorizontal();
            }
        }
    }
}
