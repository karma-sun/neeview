namespace NeeView
{
    public class ToggleMediaPlayCommand : CommandElement
    {
        public ToggleMediaPlayCommand() : base("ToggleMediaPlay")
        {
            this.Group = Properties.Resources.CommandGroupVideo;
            this.Text = Properties.Resources.CommandToggleMediaPlay;
            this.Note = Properties.Resources.CommandToggleMediaPlayNote;
        }
        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.IsMediaPlaying() ? Properties.Resources.WordStop : Properties.Resources.WordPlay;
        }

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return BookOperation.Current.Book != null && BookOperation.Current.Book.IsMedia;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookOperation.Current.ToggleMediaPlay();
        }
    }
}
