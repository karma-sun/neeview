namespace NeeView
{
    public class ViewFlipVerticalOffCommand : CommandElement
    {
        public ViewFlipVerticalOffCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewFlipVerticalOff;
            this.Note = Properties.Resources.CommandViewFlipVerticalOffNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            DragTransformControl.Current.FlipVertical(false);
        }
    }
}
