namespace NeeView
{
    public class MoveDragAction : DragAction
    {
        public MoveDragAction()
        {
            Note = Properties.Resources.DragActionType_Move;
            DragKey = new DragKey("LeftButton");
            Group = DragActionGroup.Move;
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            sender.DragMove(e.Start, e.End);
        }
    }
}
