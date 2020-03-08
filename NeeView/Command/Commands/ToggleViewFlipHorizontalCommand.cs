using System.Windows.Data;


namespace NeeView
{
    public class ToggleViewFlipHorizontalCommand : CommandElement
    {
        public ToggleViewFlipHorizontalCommand() : base("ToggleViewFlipHorizontal")
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

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ToggleFlipHorizontal();
        }
    }
}
