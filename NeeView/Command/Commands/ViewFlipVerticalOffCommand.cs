namespace NeeView
{
    public class ViewFlipVerticalOffCommand : CommandElement
    {
        public ViewFlipVerticalOffCommand() : base("ViewFlipVerticalOff")
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewFlipVerticalOff;
            this.Note = Properties.Resources.CommandViewFlipVerticalOffNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.FlipVertical(false);
        }
    }
}
