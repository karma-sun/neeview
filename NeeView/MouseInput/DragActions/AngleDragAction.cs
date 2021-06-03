namespace NeeView
{
    public class AngleDragAction : DragAction
    {
        public AngleDragAction()
        {
            Note = Properties.Resources.DragActionType_Angle;
            DragKey = new DragKey("Shift+LeftButton");
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            sender.DragAngle(e.Start, e.End);
        }
    }
}
