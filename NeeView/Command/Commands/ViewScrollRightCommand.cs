namespace NeeView
{
    public class ViewScrollRightCommand : CommandElement
    {
        public ViewScrollRightCommand() : base(CommandType.ViewScrollRight)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollRight;
            this.Note = Properties.Resources.CommandViewScrollRightNote;
            this.IsShowMessage = false;

            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true });
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            DragTransformControl.Current.ScrollRight((ViewScrollCommandParameter)param);
        }
    }
}
