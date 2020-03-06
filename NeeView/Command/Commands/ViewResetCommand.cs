namespace NeeView
{
    public class ViewResetCommand : CommandElement
    {
        public ViewResetCommand() : base(CommandType.ViewReset)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewReset;
            this.Note = Properties.Resources.CommandViewResetNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvas.Current.ResetTransform(true);
        }
    }
}
