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

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookOperation.Current.CanPrevPagemarkInPlace((MovePagemarkInBookCommandParameter)param);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.PrevPagemarkInPlace((MovePagemarkInBookCommandParameter)param);
        }
    }


    /// <summary>
    /// ページマーク移動用パラメータ
    /// </summary>
    public class MovePagemarkInBookCommandParameter : CommandParameter
    {
        private bool _isLoop;
        private bool _isIncludeTerminal;

        [PropertyMember("@ParamCommandParameterMovePagemarkLoop")]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }

        [PropertyMember("@ParamCommandParameterMovePagemarkIncludeTerminal")]
        public bool IsIncludeTerminal
        {
            get => _isIncludeTerminal;
            set => SetProperty(ref _isIncludeTerminal, value);
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as MovePagemarkInBookCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsLoop == target.IsLoop && this.IsIncludeTerminal == target.IsIncludeTerminal);
        }
    }
}
