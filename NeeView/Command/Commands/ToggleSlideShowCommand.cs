using System.Windows.Data;


namespace NeeView
{
    public class ToggleSlideShowCommand : CommandElement
    {
        public ToggleSlideShowCommand() : base(CommandType.ToggleSlideShow)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandToggleSlideShow;
            this.MenuText = Properties.Resources.CommandToggleSlideShowMenu;
            this.Note = Properties.Resources.CommandToggleSlideShowNote;
            this.ShortCutKey = "F5";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SlideShow.IsPlayingSlideShow)) { Source = SlideShow.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return SlideShow.Current.IsPlayingSlideShow ? Properties.Resources.CommandToggleSlideShowOff : Properties.Resources.CommandToggleSlideShowOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            SlideShow.Current.ToggleSlideShow();
        }
    }
}
