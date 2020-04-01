using NeeLaboratory;
using NeeView.Windows.Property;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    public class ViewScaleDownCommand : CommandElement
    {
        public ViewScaleDownCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScaleDown;
            this.Note = Properties.Resources.CommandViewScaleDownNote;
            this.ShortCutKey = "RightButton+WheelDown";
            this.IsShowMessage = false;

            // ViewScaleUp
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter() { Scale = 20, IsSnapDefaultScale = true });
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            var parameter = (ViewScaleCommandParameter)param;
            DragTransformControl.Current.ScaleDown(parameter.Scale / 100.0, parameter.IsSnapDefaultScale, ContentCanvas.Current.MainContentScale);
        }
    }


    /// <summary>
    /// ビュー拡大コマンド用パラメータ
    /// </summary>
    [DataContract]
    public class ViewScaleCommandParameter : CommandParameter
    {
        private int _scale;

        // 属性に説明文
        [DataMember]
        [PropertyRange("@ParamCommandParameterScaleAmount", 0, 100, Tips = "@ParamCommandParameterScaleAmountTips")]
        public int Scale
        {
            get { return _scale; }
            set { _scale = MathUtility.Clamp(value, 0, 100); }
        }

        [DataMember]
        [DefaultValue(true)]
        [PropertyMember("@ParamCommandParameterScaleSnapDefault", Tips = "@ParamCommandParameterScaleSnapDefaultTips")]
        public bool IsSnapDefaultScale { get; set; } = true;


        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.IsSnapDefaultScale = true;
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ViewScaleCommandParameter;
            if (target == null) return false;
            return this == target || (this.Scale == target.Scale && this.IsSnapDefaultScale == target.IsSnapDefaultScale);
        }
    }


}
