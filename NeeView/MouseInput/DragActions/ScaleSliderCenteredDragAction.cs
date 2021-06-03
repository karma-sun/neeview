namespace NeeView
{
    public class ScaleSliderCenteredDragAction : DragAction
    {
        public ScaleSliderCenteredDragAction()
        {
            Note = Properties.Resources.DragActionType_ScaleSliderCentered;

            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            var param = (SensitiveDragActionParameter)Parameter;
            sender.DragScaleSliderCentered(e.Start, e.End, param.Sensitivity);
        }
    }
}
