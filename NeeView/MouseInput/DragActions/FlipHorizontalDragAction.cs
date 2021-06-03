namespace NeeView
{
    public class FlipHorizontalDragAction : DragAction
    {
        public FlipHorizontalDragAction()
        {
            Note = Properties.Resources.DragActionType_FlipHorizontal;
            DragKey = new DragKey("Alt+LeftButton");
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            sender.DragFlipHorizontal(e.Start, e.End);
        }
    }
}
