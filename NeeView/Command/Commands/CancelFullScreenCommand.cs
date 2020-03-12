namespace NeeView
{
    public class CancelFullScreenCommand : CommandElement
    {
        public CancelFullScreenCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandCancelFullScreen;
            this.Note = Properties.Resources.CommandCancelFullScreenNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            WindowShape.Current.SetFullScreen(false);
        }
    }
}
