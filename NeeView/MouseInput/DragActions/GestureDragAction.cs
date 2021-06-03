namespace NeeView
{
    public class GestureDragAction : DragAction
    {
        public GestureDragAction(string name) : base(name)
        {
            Note = Properties.Resources.DragActionType_Gesture;
            IsLocked = true;
            IsDummy = true;
            DragKey = new DragKey("RightButton");
        }
    }
}
