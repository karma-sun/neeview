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

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.ScrollNTypeUp((ViewScrollNTypeCommandParameter)param);
        }
    }


    public interface IScrollNType
    {
        double Margin { get; set; }
        int Scroll { get; set; }
        double ScrollDuration { get; set; }
    }


    /// <summary>
    /// N字スクロール
    /// </summary>
    public class ViewScrollNTypeCommandParameter : ReversibleCommandParameter, IScrollNType
    {
        private int _scroll = 100;
        private double _margin = 0;
        private double _scrollDuration = 0.1;

        [PropertyMember("@ParamCommandParameterScrollPageMargin", Tips = "@ParamCommandParameterScrollPageMarginTips")]
        public double Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }

        [PropertyRange("@ParamCommandParameterScrollPageAmount", 0, 100, Tips = "@ParamCommandParameterScrollPageAmountTips")]
        public int Scroll
        {
            get => _scroll;
            set => SetProperty(ref _scroll, MathUtility.Clamp(value, 0, 100));
        }

        [PropertyRange("@ParamCommandParameterScrollPageDuration", 0.0, 1.0, TickFrequency = 0.1, IsEditable = true)]
        public double ScrollDuration
        {
            get { return _scrollDuration; }
            set { SetProperty(ref _scrollDuration, Math.Max(value, 0.0)); }
        }
    }

}
