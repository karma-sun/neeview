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
            return new Binding(nameof(DragTransform.IsFlipVertical)) { Source = DragTransform.Current, Mode = BindingMode.OneWay };
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                DragTransformControl.Current.FlipVertical(Convert.ToBoolean(args[0]));
            }
            else
            { 
                DragTransformControl.Current.ToggleFlipVertical();
            }
        }
    }
}
