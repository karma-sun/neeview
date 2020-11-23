using NeeLaboratory;
using NeeView.Windows.Property;

namespace NeeView
{
    public class ViewRotateLeftCommand : CommandElement
    {
        public ViewRotateLeftCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandViewRotateLeft;
            this.Note = Properties.Resources.CommandViewRotateLeftNote;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewRotateCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewControlMediator.Current.ViewRotateLeft(sender, (ViewRotateCommandParameter)e.Parameter);
        }
    }



    /// <summary>
    /// ビュー回転コマンド用パラメータ
    /// </summary>
    public class ViewRotateCommandParameter : CommandParameter 
    {
        private int _angle = 45;
        private bool _isStretch;


        // 属性に説明文
        [PropertyRange("@ParamCommandParameterRotateAmount", 0, 180, Tips = "@ParamCommandParameterRotateAmountTips")]
        public int Angle
        {
            get { return _angle; }
            set { SetProperty(ref _angle, MathUtility.Clamp(value, 0, 180)); }
        }

        // 属性に説明文
        [PropertyMember("@ParamCommandParameterRotateIsStretch", Tips = "@ParamCommandParameterRotateIsStretchTips")]
        public bool IsStretch
        {
            get { return _isStretch; }
            set { SetProperty(ref _isStretch, value); }
        }
    }
}
