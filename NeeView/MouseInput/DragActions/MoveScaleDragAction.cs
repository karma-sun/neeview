using NeeView.Windows.Property;

namespace NeeView
{
    public class MoveScaleDragAction : DragAction
    {
        public MoveScaleDragAction()
        {
            Note = Properties.Resources.DragActionType_MoveScale;
            Group = DragActionGroup.Move;

            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            var param = (SensitiveDragActionParameter)Parameter;
            sender.DragMoveScale(e.Start, e.End, param.Sensitivity);
        }
    }
}
