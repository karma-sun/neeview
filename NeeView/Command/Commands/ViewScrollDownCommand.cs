using NeeLaboratory;
using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ViewScrollDownCommand : CommandElement
    {
        public ViewScrollDownCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScrollDown;
            this.Note = Properties.Resources.CommandViewScrollDownNote;
            this.IsShowMessage = false;
            
            // ViewScrollUp
            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter() { Scroll = 25, AllowCrossScroll = true });
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            DragTransformControl.Current.ScrollDown((ViewScrollCommandParameter)param);
        }
    }



    /// <summary>
    /// ビュースクロールコマンド用パラメータ
    /// </summary>
    [DataContract]
    public class ViewScrollCommandParameter : CommandParameter
    {
        // 属性に説明文
        [DataMember]
        [PropertyRange("@ParamCommandParameterScrollAmount", 0, 100, Tips = "@ParamCommandParameterScrollAmountTips")]
        public int Scroll
        {
            get { return _scroll; }
            set { _scroll = MathUtility.Clamp(value, 0, 100); }
        }
        private int _scroll;

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollAllowCross", Tips = "@ParamCommandParameterScrollAllowCrossTips")]
        public bool AllowCrossScroll { get; set; } = true;

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
