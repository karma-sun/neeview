using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class PrevScrollPageCommand : CommandElement
    {
        public PrevScrollPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Move;
            this.ShortCutKey = "WheelUp";
            this.IsShowMessage = false;
            this.PairPartner = "NextScrollPage";

            this.ParameterSource = new CommandParameterSource(new ScrollPageCommandParameter());
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.PrevScrollPage(sender, (ScrollPageCommandParameter)e.Parameter);
        }
    }


    /// <summary>
    /// スクロール＋ページ移動用パラメータ
    /// </summary>
    [DataContract]
    public class ScrollPageCommandParameter : ReversibleCommandParameter, IScrollNTypeParameter, IScrollNTypeEndMargin
    {
        private bool _isNScroll = true;
        private double _scroll = 1.0;
        private double _scrollDuration = 0.2;
        private double _endMargin = 10.0;
        private double _lineBreakStopTime;
        private LineBreakStopMode _lineBreakStopMode = LineBreakStopMode.Line;


        [DataMember]
        [PropertyMember]
        public bool IsNScroll
        {
            get => _isNScroll;
            set => SetProperty(ref _isNScroll, value);
        }

        [DataMember(Name = "ScrollV2")]
        [PropertyPercent]
        public double Scroll
        {
            get => _scroll;
            set => SetProperty(ref _scroll, MathUtility.Clamp(value, 0.1, 1.0));
        }

        [DataMember]
        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double ScrollDuration
        {
            get { return _scrollDuration; }
            set { SetProperty(ref _scrollDuration, Math.Max(value, 0.0)); }
        }

        [PropertyMember]
        public double EndMargin
        {
            get => _endMargin;
            set => SetProperty(ref _endMargin, Math.Max(value, 0.0));
        }

        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double LineBreakStopTime
        {
            get { return _lineBreakStopTime; }
            set { SetProperty(ref _lineBreakStopTime, value); }
        }

        [PropertyMember]
        public LineBreakStopMode LineBreakStopMode
        {
            get { return _lineBreakStopMode; }
            set { SetProperty(ref _lineBreakStopMode, value); }
        }


        #region Obsolete

        [Obsolete("Use LineBreakStopTime instead.")] // ver.39
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double PageMoveMargin
        {
            get { return 0.0; }
            set { LineBreakStopTime = value; LineBreakStopMode = LineBreakStopMode.Page; }
        }

        [Obsolete, JsonIgnore, EqualsIgnore, DataMember(Name = "Scroll", EmitDefaultValue = false)] // ver.37
        [PropertyMapIgnore]
        public int ScrollV1
        {
            get => 0;
            set => Scroll = value / 100.0;
        }

        [Obsolete, JsonIgnore, EqualsIgnore, DataMember(EmitDefaultValue = false)] // ver.37
        [PropertyMapIgnore]
        public bool IsAnimation
        {
            get => false;
            set => ScrollDuration = value ? 0.1 : 0.0;
        }

        [Obsolete, JsonIgnore, EqualsIgnore, DataMember(EmitDefaultValue = false)] // ver.37
        [PropertyMapIgnore]
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
            _scroll = 1.0;
            _scrollDuration = 0.1;
            _endMargin = 10.0;
        }
    }


    public enum LineBreakStopMode
    {
        Line,
        Page,
    }
}
