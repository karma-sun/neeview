using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleSlideShowCommand : CommandElement
    {
        public ToggleSlideShowCommand(string name) : base(name)
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

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SlideShow.Current.IsPlayingSlideShow ? Properties.Resources.CommandToggleSlideShowOff : Properties.Resources.CommandToggleSlideShowOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SlideShow.Current.IsPlayingSlideShow = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                SlideShow.Current.TogglePlayingSlideShow();
            }
        }
    }
}
