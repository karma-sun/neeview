using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class FocusMainViewCommand : CommandElement
    {
        public FocusMainViewCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPanel;
            this.Text = Properties.Resources.CommandFocusMainView;
            this.MenuText = Properties.Resources.CommandFocusMainViewMenu;
            this.Note = Properties.Resources.CommandFocusMainViewNote;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new FocusMainViewCommandParameter() { NeedClosePanels = false });
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.FocusMainView((FocusMainViewCommandParameter)param, option.HasFlag(CommandOption.ByMenu));
        }
    }


    [DataContract]
    public class FocusMainViewCommandParameter : CommandParameter
    {
        private bool _needClosePanels;

        [DataMember]
        [PropertyMember("@ParamCommandParameterFocusMainViewClosePanels")]
        public bool NeedClosePanels
        {
            get => _needClosePanels;
            set => SetProperty(ref _needClosePanels, value);
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as FocusMainViewCommandParameter;
            if (target == null) return false;
            return this == target || (this.NeedClosePanels == target.NeedClosePanels);
        }
    }

}
