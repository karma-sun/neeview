namespace NeeView
{
    public class ViewFlipHorizontalOffCommand : CommandElement
    {
        public ViewFlipHorizontalOffCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewFlipHorizontalOff;
            this.Note = Properties.Resources.CommandViewFlipHorizontalOffNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            DragTransformControl.Current.FlipHorizontal(false);
        }
    }
}
