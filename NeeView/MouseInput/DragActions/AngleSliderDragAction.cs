namespace NeeView
{
    public class AngleSliderDragAction : DragAction
    {
        public AngleSliderDragAction()
        {
            Note = Properties.Resources.DragActionType_AngleSlider;

            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            var param = (SensitiveDragActionParameter)Parameter;
            sender.DragAngleSlider(e.Start, e.End, param.Sensitivity);
        }
    }
}
