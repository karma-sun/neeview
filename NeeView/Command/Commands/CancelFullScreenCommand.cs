namespace NeeView
{
    public class CancelFullScreenCommand : CommandElement
    {
        public CancelFullScreenCommand() : base(CommandType.CancelFullScreen)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandCancelFullScreen;
            this.Note = Properties.Resources.CommandCancelFullScreenNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            WindowShape.Current.SetFullScreen(false);
        }
    }
}
