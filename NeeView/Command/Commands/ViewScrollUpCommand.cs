using NeeLaboratory;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ViewScrollUpCommand : CommandElement
    {
        public ViewScrollUpCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollUp;
            this.Note = Properties.Resources.CommandViewScrollUpNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true });
        }
        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            DragTransformControl.Current.ScrollUp((ViewScrollCommandParameter)param);
        }
    }


    /// <summary>
    /// ビュースクロールコマンド用パラメータ
    /// </summary>
    [DataContract]
    public class ViewScrollCommandParameter : CommandParameter
    {
        private int _scroll;
        private bool _allowCrossScroll = true;

        // 属性に説明文
        [DataMember]
        [PropertyRange("@ParamCommandParameterScrollAmount", 0, 100, Tips = "@ParamCommandParameterScrollAmountTips")]
        public int Scroll
        {
            get { return _scroll; }
            set { SetProperty(ref _scroll, MathUtility.Clamp(value, 0, 100)); }
        }

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollAllowCross", Tips = "@ParamCommandParameterScrollAllowCrossTips")]
        public bool AllowCrossScroll
        {
            get => _allowCrossScroll;
            set => SetProperty(ref _allowCrossScroll, value);
        }


        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.AllowCrossScroll = true;
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ViewScrollCommandParameter;
            if (target == null) return false;
            return this == target || (this.Scroll == target.Scroll && this.AllowCrossScroll == target.AllowCrossScroll);
        }
    }

}
