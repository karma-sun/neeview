namespace NeeView
{
    public class ViewScaleDownCommand : CommandElement
    {
        public ViewScaleDownCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScaleDown;
            this.Note = Properties.Resources.CommandViewScaleDownNote;
            this.ShortCutKey = "RightButton+WheelDown";
            this.IsShowMessage = false;

            // ViewScaleUp
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter() { Scale = 20, IsSnapDefaultScale = true });
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            var parameter = (ViewScaleCommandParameter)param;
            DragTransformControl.Current.ScaleDown(parameter.Scale / 100.0, parameter.IsSnapDefaultScale, ContentCanvas.Current.MainContentScale);
        }
    }
}
