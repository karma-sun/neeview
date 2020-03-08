namespace NeeView
{
    public class ViewScaleUpCommand : CommandElement
    {
        public ViewScaleUpCommand() : base("ViewScaleUp")
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScaleUp;
            this.Note = Properties.Resources.CommandViewScaleUpNote;
            this.ShortCutKey = "RightButton+WheelUp";
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter() { Scale = 20, IsSnapDefaultScale = true });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var parameter = (ViewScaleCommandParameter)param;
            DragTransformControl.Current.ScaleUp(parameter.Scale / 100.0, parameter.IsSnapDefaultScale, ContentCanvas.Current.MainContentScale);
        }
    }
}
