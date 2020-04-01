using NeeView.Windows.Property;

namespace NeeView
{
    public class NextPagemarkInBookCommand : CommandElement
    {
        public NextPagemarkInBookCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPagemark;
            this.Text = Properties.Resources.CommandNextPagemarkInBook;
            this.Note = Properties.Resources.CommandNextPagemarkInBookNote;
            this.IsShowMessage = false;

            // PrevPagemarkInBook
            this.ParameterSource = new CommandParameterSource(new MovePagemarkInBookCommandParameter());
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return BookOperation.Current.CanNextPagemarkInPlace((MovePagemarkInBookCommandParameter)param);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.NextPagemarkInPlace((MovePagemarkInBookCommandParameter)param);
        }
    }



    /// <summary>
    /// ページマーク移動用パラメータ
    /// </summary>
    public class MovePagemarkInBookCommandParameter : CommandParameter
    {
        [PropertyMember("@ParamCommandParameterMovePagemarkLoop")]
        public bool IsLoop { get; set; }

        [PropertyMember("@ParamCommandParameterMovePagemarkIncludeTerminal")]
        public bool IsIncludeTerminal { get; set; }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as MovePagemarkInBookCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsLoop == target.IsLoop && this.IsIncludeTerminal == target.IsIncludeTerminal);
        }
    }
}
