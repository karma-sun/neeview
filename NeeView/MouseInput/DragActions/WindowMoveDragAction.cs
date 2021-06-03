namespace NeeView
{
    public class WindowMoveDragAction : DragAction
    {
        public WindowMoveDragAction()
        {
            Note = Properties.Resources.DragActionType_WindowMove;
            DragKey = new DragKey("RightButton+LeftButton");
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            sender.DragWindowMove(e.Start, e.End);
        }
    }
}
