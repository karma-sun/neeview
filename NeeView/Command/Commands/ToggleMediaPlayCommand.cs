namespace NeeView
{
    public class ToggleMediaPlayCommand : CommandElement
    {
        public ToggleMediaPlayCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Video;
        }
        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookOperation.Current.IsMediaPlaying() ? Properties.Resources.Word_Stop : Properties.Resources.Word_Play;
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
