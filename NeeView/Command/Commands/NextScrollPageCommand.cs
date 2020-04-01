using NeeLaboratory;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class NextScrollPageCommand : CommandElement
    {
        public NextScrollPageCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupMove;
            this.Text = Properties.Resources.CommandNextScrollPage;
            this.Note = Properties.Resources.CommandNextScrollPageNote;
            this.ShortCutKey = "WheelDown";
            this.IsShowMessage = false;
            this.PairPartner = "PrevScrollPage";

            // PrevScrollPage
            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter() { IsNScroll = true, IsAnimation = true, Margin = 50, Scroll = 100 });
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.NextScrollPage((ScrollPageCommandParameter)param);
        }
    }


    /// <summary>
    /// スクロール＋ページ移動用パラメータ
    /// </summary>
    [DataContract]
    public class ScrollPageCommandParameter : ReversibleCommandParameter
    {
        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageN", Tips = "@ParamCommandParameterScrollPageNTips")]
        public bool IsNScroll { get; set; }

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageAnimation")]
        public bool IsAnimation { get; set; }

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageMargin", Tips = "@ParamCommandParameterScrollPageMarginTips")]
        public double Margin { get; set; }

        [DataMember]
        [PropertyRange("@ParamCommandParameterScrollPageAmount", 0, 100, Tips = "@ParamCommandParameterScrollPageAmountTips")]
        public int Scroll
        {
            get { return _scroll; }
            set { _scroll = MathUtility.Clamp(value, 0, 100); }
        }
        private int _scroll;

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageStop", Tips = "@ParamCommandParameterScrollPageStopTips")]
        public bool IsStop { get; set; }


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
