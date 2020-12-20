using NeeView.Windows.Property;

namespace NeeView
{
    public class PrevPagemarkInBookCommand : CommandElement
    {
        public PrevPagemarkInBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandPrevPagemarkInBook;
            this.Note = Properties.Resources.CommandPrevPagemarkInBookNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new MovePagemarkInBookCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanPrevPagemarkInPlace((MovePagemarkInBookCommandParameter)e.Parameter);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookOperation.Current.PrevPagemarkInPlace((MovePagemarkInBookCommandParameter)e.Parameter);
        }
    }


    /// <summary>
    /// ページマーク移動用パラメータ
    /// </summary>
    public class MovePagemarkInBookCommandParameter : CommandParameter
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
