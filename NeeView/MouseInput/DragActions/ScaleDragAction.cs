namespace NeeView
{
    public class ScaleDragAction : DragAction
    {
        public ScaleDragAction()
        {
            Note = Properties.Resources.DragActionType_Scale;
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            sender.DragScale(e.Start, e.End);
        }
    }
}
