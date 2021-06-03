namespace NeeView
{
    public class MarqueeZoomDragAction : DragAction
    {
        public MarqueeZoomDragAction()
        {
            Note = Properties.Resources.DragActionType_MarqueeZoom;
        }

        public override void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
            sender.DragMarqueeZoom(e.Start, e.End);
        }

        public override void ExecuteEnd(DragTransformControl sender, DragTransformActionArgs e)
        {
            sender.DragMarqueeZoomEnd(e.Start, e.End);
        }
    }
}
