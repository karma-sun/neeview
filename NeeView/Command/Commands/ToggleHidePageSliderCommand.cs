using System.Windows.Data;


namespace NeeView
{
    public class ToggleHidePageSliderCommand : CommandElement
    {
        public ToggleHidePageSliderCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupWindow;
            this.Text = Properties.Resources.CommandToggleHidePageSlider;
            this.MenuText = Properties.Resources.CommandToggleHidePageSliderMenu;
            this.Note = Properties.Resources.CommandToggleHidePageSliderNote;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(MainWindowModel.Current.IsHidePageSlider)) { Source = MainWindowModel.Current };
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return MainWindowModel.Current.IsHidePageSlider ? Properties.Resources.CommandToggleHidePageSliderOff : Properties.Resources.CommandToggleHidePageSliderOn;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            MainWindowModel.Current.ToggleHidePageSlider();
        }
    }
}
