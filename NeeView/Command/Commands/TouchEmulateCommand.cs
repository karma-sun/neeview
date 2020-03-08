namespace NeeView
{
    public class TouchEmulateCommand : CommandElement
    {
        public TouchEmulateCommand() : base("TouchEmulate")
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandTouchEmulate;
            this.Note = Properties.Resources.CommandTouchEmulateNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            TouchInput.Current.Emulator.Execute();
        }
    }
}
