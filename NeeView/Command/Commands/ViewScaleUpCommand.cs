using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ViewScaleUpCommand : CommandElement
    {
        public ViewScaleUpCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewScaleUp;
            this.Note = Properties.Resources.CommandViewScaleUpNote;
            this.ShortCutKey = "RightButton+WheelUp";
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.ScaleUp((ViewScaleCommandParameter)e.Parameter);
        }
    }


    /// <summary>
    /// ビュー拡大コマンド用パラメータ
    /// </summary>
    [DataContract]
    public class ViewScaleCommandParameter : CommandParameter
    {
        private double _scale = 0.2;
        private bool _isSnapDefaultScale = true;

        [DataMember(Name = "ScaleV2")]
        [PropertyPercent("@ParamCommandParameterScaleAmount", Tips = "@ParamCommandParameterScaleAmountTips")]
        public double Scale
        {
            get { return _scale; }
            set { SetProperty(ref _scale, MathUtility.Clamp(value, 0.0, 1.0)); }
        }

        [DataMember]
        [DefaultValue(true)]
        [PropertyMember("@ParamCommandParameterScaleSnapDefault", Tips = "@ParamCommandParameterScaleSnapDefaultTips")]
        public bool IsSnapDefaultScale
        {
            get => _isSnapDefaultScale;
            set => SetProperty(ref _isSnapDefaultScale, value);
        }


        #region Obsolete
        [Obsolete, JsonIgnore, EqualsIgnore, DataMember(Name = "Scale", EmitDefaultValue = false)] // ver.37
        [PropertyMapIgnore]
        public int ScaleV1
        {
            get => 0;
            set => Scale = value / 100.0;
        }
        #endregion


        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.Scale = 0.2;
            this.IsSnapDefaultScale = true;
        }
    }
}
