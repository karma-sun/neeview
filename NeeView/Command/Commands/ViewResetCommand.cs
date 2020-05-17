namespace NeeView
{
    public class ViewResetCommand : CommandElement
    {
        public ViewResetCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewReset;
            this.Note = Properties.Resources.CommandViewResetNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvas.Current.ResetContentSizeAndTransform();
        }
    }
}
