using NeeLaboratory;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class PrevScrollPageCommand : CommandElement
    {
        public PrevScrollPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandPrevScrollPage;
            this.Note = Properties.Resources.CommandPrevScrollPageNote;
            this.ShortCutKey = "WheelUp";
            this.IsShowMessage = false;
            this.PairPartner = "NextScrollPage";

            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter() { IsNScroll = true, IsAnimation = true, Margin = 50, Scroll = 100 });
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.PrevScrollPage((ScrollPageCommandParameter)param);
        }
    }


    /// <summary>
    /// スクロール＋ページ移動用パラメータ
    /// </summary>
    [DataContract]
    public class ScrollPageCommandParameter : ReversibleCommandParameter
    {
        private int _scroll = 100;
        private bool _isNScroll;
        private bool _isAnimation;
        private double _margin;
        private bool _isStop;


        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageN", Tips = "@ParamCommandParameterScrollPageNTips")]
        public bool IsNScroll
        {
            get => _isNScroll;
            set => SetProperty(ref _isNScroll, value);
        }

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageAnimation")]
        public bool IsAnimation
        {
            get => _isAnimation;
            set => SetProperty(ref _isAnimation, value);
        }

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageMargin", Tips = "@ParamCommandParameterScrollPageMarginTips")]
        public double Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }

        [DataMember]
        [PropertyRange("@ParamCommandParameterScrollPageAmount", 0, 100, Tips = "@ParamCommandParameterScrollPageAmountTips")]
        public int Scroll
        {
            get => _scroll;
            set => SetProperty(ref _scroll, MathUtility.Clamp(value, 0, 100));
        }

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageStop", Tips = "@ParamCommandParameterScrollPageStopTips")]
        public bool IsStop
        {
            get => _isStop;
            set => SetProperty(ref _isStop, value);
        }


        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _scroll = 100;
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ScrollPageCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsNScroll == target.IsNScroll &&
                this.IsAnimation == target.IsAnimation &&
                this.Margin == target.Margin &&
                this.Scroll == target.Scroll &&
                this.IsStop == target.IsStop);
        }
    }
}
