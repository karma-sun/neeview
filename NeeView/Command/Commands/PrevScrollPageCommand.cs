using NeeLaboratory;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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

            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter());
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
    public class ScrollPageCommandParameter : ReversibleCommandParameter, IScrollNType
    {
        private bool _isNScroll = true;
        private int _scroll = 100;
        private double _margin = 0;
        private double _scrollDuration = 0.1;
        private double _pageMoveMargin;


        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageN", Tips = "@ParamCommandParameterScrollPageNTips")]
        public bool IsNScroll
        {
            get => _isNScroll;
            set => SetProperty(ref _isNScroll, value);
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
        [PropertyRange("@ParamCommandParameterScrollPageDuration", 0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double ScrollDuration
        {
            get { return _scrollDuration; }
            set { SetProperty(ref _scrollDuration, Math.Max(value, 0.0)); }
        }

        [PropertyRange("@ParamCommandParameterScrollPageMoveMargin", 0.0, 1.0, TickFrequency = 0.1, IsEditable = true, Tips = "@ParamCommandParameterScrollPageMoveMarginTips")]
        public double PageMoveMargin
        {
            get { return _pageMoveMargin; }
            set { SetProperty(ref _pageMoveMargin, value); }
        }


        #region Obsolete

        [Obsolete, JsonIgnore, DataMember(EmitDefaultValue = false)] // ver.37
        [PropertyMapIgnore]
        [PropertyMember("@ParamCommandParameterScrollPageAnimation", IsVisible = false)]
        public bool IsAnimation
        {
            get => false;
            set => ScrollDuration = value ? 0.1 : 0.0;
        }

        [Obsolete, JsonIgnore, DataMember(EmitDefaultValue = false)] // ver.37
        [PropertyMapIgnore]
        [PropertyMember("@ParamCommandParameterScrollPageStop", Tips = "@ParamCommandParameterScrollPageStopTips", IsVisible = false)]
        public bool IsStop
        {
            get => false;
            set => PageMoveMargin = value ? 0.1 : 0.0;
        }

        #endregion


        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _isNScroll = true;
            _scroll = 100;
            _margin = 50;
            _scrollDuration = 0.1;
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            if (!base.MemberwiseEquals(other)) return false;

            var target = other as ScrollPageCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsNScroll == target.IsNScroll &&
                this.Margin == target.Margin &&
                this.Scroll == target.Scroll &&
                this.ScrollDuration == target.ScrollDuration &&
                this.PageMoveMargin == target.PageMoveMargin);
        }
    }
}
