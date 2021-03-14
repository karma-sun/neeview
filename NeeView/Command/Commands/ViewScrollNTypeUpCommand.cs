using NeeLaboratory;
using NeeView.Windows.Property;
using System;

namespace NeeView
{
    public class ViewScrollNTypeUpCommand : CommandElement
    {
        public ViewScrollNTypeUpCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ViewScrollNTypeCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ScrollNTypeUp((ViewScrollNTypeCommandParameter)e.Parameter);
        }
    }


    /// <summary>
    /// N字スクロール
    /// </summary>
    public class ViewScrollNTypeCommandParameter : ReversibleCommandParameter, IScrollNTypeParameter
    {
        private double _scroll = 1.0;
        private double _scrollDuration = 0.2;
        private double _lineBreakStopTime;

        [PropertyPercent]
        public double Scroll
        {
            get => _scroll;
            set => SetProperty(ref _scroll, MathUtility.Clamp(value, 0.1, 1.0));
        }

        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double ScrollDuration
        {
            get { return _scrollDuration; }
            set { SetProperty(ref _scrollDuration, Math.Max(value, 0.0)); }
        }

        [PropertyRange(0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double LineBreakStopTime
        {
            get { return _lineBreakStopTime; }
            set { SetProperty(ref _lineBreakStopTime, value); }
        }
    }

}
