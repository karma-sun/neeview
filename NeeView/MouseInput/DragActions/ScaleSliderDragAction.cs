namespace NeeView
{
    public class ScaleSliderDragAction : DragAction
    {
        public ScaleSliderDragAction()
        {
            Note = Properties.Resources.DragActionType_ScaleSlider;
            DragKey = new DragKey("Ctrl+LeftButton");

            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            var param = (SensitiveDragActionParameter)Parameter;
            sender.DragScaleSlider(e.Start, e.End, param.Sensitivity);
        }
    }



}
