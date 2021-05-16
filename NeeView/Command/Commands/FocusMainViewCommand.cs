using NeeView.Windows.Property;
using System.Runtime.Serialization;

namespace NeeView
{
    public class FocusMainViewCommand : CommandElement
    {
        public FocusMainViewCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Panel;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new FocusMainViewCommandParameter());
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.FocusMainView((FocusMainViewCommandParameter)e.Parameter, e.Options.HasFlag(CommandOption.ByMenu));
        }
    }


    [DataContract]
    public class FocusMainViewCommandParameter : CommandParameter
    {
        private bool _needClosePanels;

        [DataMember]
        [PropertyMember]
        public bool NeedClosePanels
        {
            get => _needClosePanels;
            set => SetProperty(ref _needClosePanels, value);
        }
    }

}
