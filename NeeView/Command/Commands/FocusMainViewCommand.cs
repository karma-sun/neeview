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
