using NeeLaboratory;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ViewScrollNTypeUpCommand : CommandElement
    {
        public ViewScrollNTypeUpCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollNTypeUp;
            this.Note = Properties.Resources.CommandViewScrollNTypeUpNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ViewScrollNTypeCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.ScrollNTypeUp((ViewScrollNTypeCommandParameter)e.Parameter);
        }
    }


    /// <summary>
    /// N字スクロール
    /// </summary>
    public class ViewScrollNTypeCommandParameter : ReversibleCommandParameter, IScrollNTypeParameter
    {
        private double _scroll = 1.0;
        private double _margin = 50;
        private double _scrollDuration = 0.2;

        [PropertyMember("@ParamCommandParameterScrollPageMargin", Tips = "@ParamCommandParameterScrollPageMarginTips")]
        public double Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, Math.Max(value, 10));
        }

        [PropertyPercent("@ParamCommandParameterScrollPageAmount", Tips = "@ParamCommandParameterScrollPageAmountTips")]
        public double Scroll
        {
            get => _scroll;
            set => SetProperty(ref _scroll, MathUtility.Clamp(value, 0.0, 1.0));
        }

        [PropertyRange("@ParamCommandParameterScrollPageDuration", 0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double ScrollDuration
        {
            get { return _scrollDuration; }
            set { SetProperty(ref _scrollDuration, Math.Max(value, 0.0)); }
        }
    }

}
