using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleThumbnailListCommand : CommandElement
    {
        public ToggleVisibleThumbnailListCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupFilmStrip;
            this.Text = Properties.Resources.CommandToggleVisibleThumbnailList;
            this.MenuText = Properties.Resources.CommandToggleVisibleThumbnailListMenu;
            this.Note = Properties.Resources.CommandToggleVisibleThumbnailListNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(FilmStripConfig.IsEnabled)) { Source = Config.Current.FilmStrip };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return ThumbnailList.Current.IsVisible ? Properties.Resources.CommandToggleVisibleThumbnailListOff : Properties.Resources.CommandToggleVisibleThumbnailListOn;
        }

        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            if (args.Length > 0)
            {
                ThumbnailList.Current.SetVisibleThumbnailList(Convert.ToBoolean(args[0]));
            }
            else
            {
                ThumbnailList.Current.ToggleVisibleThumbnailList(option.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
