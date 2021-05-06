namespace NeeView
{
    public class NextPlaylistItemCommand : CommandElement
    {
        public NextPlaylistItemCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Playlist;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading && PlaylistPresenter.Current.PlaylistListBox?.CanMoveNext() == true;
        }

        public override void Execute(object sender, CommandContext e)
        {
            var isSuccess = PlaylistPresenter.Current.PlaylistListBox?.MoveNext();
            if (isSuccess != true)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Command, Properties.Resources.Notice_PlaylistItemNextFailed);
            }
        }
    }
}
