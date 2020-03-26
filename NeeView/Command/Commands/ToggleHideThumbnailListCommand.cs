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
            return new Binding(nameof(FilmStripConfig.IsHideFilmStrip)) { Source = Config.Current.Layout.FilmStrip };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.Layout.FilmStrip.IsHideFilmStrip ? Properties.Resources.CommandToggleHideThumbnailListOff : Properties.Resources.CommandToggleHideThumbnailListOn;
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return Config.Current.Layout.FilmStrip.IsEnabled;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                Config.Current.Layout.FilmStrip.IsHideFilmStrip = Convert.ToBoolean(args[0]);
            }
            else
            {
                ThumbnailList.Current.ToggleHideThumbnailList();
            }
        }
    }
}
