namespace NeeView
{
    public class FlipVerticalDragAction : DragAction
    {
        public FlipVerticalDragAction()
        {
            Note = Properties.Resources.DragActionType_FlipVertical;
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            sender.DragFlipVertical(e.Start, e.End);
        }
    }
}
