using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHideThumbnailListCommand : CommandElement
    {
        public ToggleHideThumbnailListCommand()
        {
            this.Group = Properties.Resources.CommandGroup_FilmStrip;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(FilmStripConfig.IsHideFilmStrip)) { Source = Config.Current.FilmStrip };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.FilmStrip.IsHideFilmStrip ? Properties.Resources.ToggleHideThumbnailListCommand_Off : Properties.Resources.ToggleHideThumbnailListCommand_On;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return Config.Current.FilmStrip.IsEnabled;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.FilmStrip.IsHideFilmStrip = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                ThumbnailList.Current.ToggleHideThumbnailList();
            }
        }
    }
}
