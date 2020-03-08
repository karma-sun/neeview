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
            return new Binding(nameof(ThumbnailList.Current.IsEnableThumbnailList)) { Source = ThumbnailList.Current };
        }

        public override string ExecuteMessage(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return ThumbnailList.Current.IsVisible ? Properties.Resources.CommandToggleVisibleThumbnailListOff : Properties.Resources.CommandToggleVisibleThumbnailListOn;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ThumbnailList.Current.ToggleVisibleThumbnailList(option.HasFlag(CommandOption.ByMenu));
        }
    }
}
