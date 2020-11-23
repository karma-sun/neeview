namespace NeeView
{
    public class ToggleMediaPlayCommand : CommandElement
    {
        public ToggleMediaPlayCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupVideo;
            this.Text = Properties.Resources.CommandToggleMediaPlay;
            this.Note = Properties.Resources.CommandToggleMediaPlayNote;
        }
        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookOperation.Current.IsMediaPlaying() ? Properties.Resources.WordStop : Properties.Resources.WordPlay;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.Book != null && BookOperation.Current.Book.IsMedia;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.ToggleMediaPlay();
        }
    }
}
