using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleSlideShowCommand : CommandElement
    {
        public ToggleSlideShowCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.ShortCutKey = "F5";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SlideShow.IsPlayingSlideShow)) { Source = SlideShow.Current };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return SlideShow.Current.IsPlayingSlideShow ? Properties.Resources.ToggleSlideShowCommand_Off : Properties.Resources.ToggleSlideShowCommand_On;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
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
