using NeeView.Windows.Property;

namespace NeeView
{
    public class PrevPlaylistItemInBookCommand : CommandElement
    {
        public PrevPlaylistItemInBookCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Playlist;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new MovePlaylsitItemInBookCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanPrevMarkInPlace((MovePlaylsitItemInBookCommandParameter)e.Parameter);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.PrevMarkInPlace((MovePlaylsitItemInBookCommandParameter)e.Parameter);
        }
    }


    /// <summary>
    /// プレイリスト項目移動用パラメータ
    /// </summary>
    public class MovePlaylsitItemInBookCommandParameter : CommandParameter
    {
        private bool _isLoop;
        private bool _isIncludeTerminal;

        [PropertyMember]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }

        [PropertyMember]
        public bool IsIncludeTerminal
        {
            get => _isIncludeTerminal;
            set => SetProperty(ref _isIncludeTerminal, value);
        }
    }
}
