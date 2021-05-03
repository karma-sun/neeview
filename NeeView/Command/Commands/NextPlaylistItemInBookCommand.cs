using NeeView.Windows.Property;

namespace NeeView
{
    public class NextPlaylistItemInBookCommand : CommandElement
    {
        public NextPlaylistItemInBookCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Playlist;
            this.IsShowMessage = false;

            // PrevPlaylistItemInBook
            this.ParameterSource = new CommandParameterSource(new MovePlaylsitItemInBookCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanNextMarkInPlace((MovePlaylsitItemInBookCommandParameter)e.Parameter);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.NextMarkInPlace((MovePlaylsitItemInBookCommandParameter)e.Parameter);
        }
    }

}
