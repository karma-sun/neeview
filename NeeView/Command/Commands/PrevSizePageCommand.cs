using NeeLaboratory;
using NeeView.Windows.Property;

namespace NeeView
{
    public class PrevSizePageCommand : CommandElement
    {
        public PrevSizePageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevSizePage;
            this.Note = Properties.Resources.CommandPrevSizePageNote;
            this.IsShowMessage = false;
            this.PairPartner = "NextSizePage";

            this.ParameterSource = new CommandParameterSource(new MoveSizePageCommandParameter() { Size = 10 });
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookOperation.Current.PrevSizePage(((MoveSizePageCommandParameter)param).Size);
        }
    }



    /// <summary>
    /// 指定ページ数移動コマンド用パラメータ
    /// </summary>
    public class MoveSizePageCommandParameter : ReversibleCommandParameter
    {
        private int _size;

        [PropertyMember("@ParamCommandParameterMoveSize")]
        public int Size
        {
            get { return _size; }
            set { SetProperty(ref _size, MathUtility.Clamp(value, 0, 1000)); }
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as MoveSizePageCommandParameter;
            if (target == null) return false;
            return this == target || (this.Size == target.Size);
        }
    }
}
