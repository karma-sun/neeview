using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleHideThumbnailListCommand : CommandElement
    {
        public ToggleHideThumbnailListCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFilmStrip;
            this.Text = Properties.Resources.CommandToggleHideThumbnailList;
            this.MenuText = Properties.Resources.CommandToggleHideThumbnailListMenu;
            this.Note = Properties.Resources.CommandToggleHideThumbnailListNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(FilmStripConfig.IsHideFilmStrip)) { Source = Config.Current.FilmStrip };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.FilmStrip.IsHideFilmStrip ? Properties.Resources.CommandToggleHideThumbnailListOff : Properties.Resources.CommandToggleHideThumbnailListOn;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return Config.Current.FilmStrip.IsEnabled;
        }

        [MethodArgument("@CommandToggleArgument")]
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
