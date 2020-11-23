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
            return new Binding(nameof(SliderConfig.IsHidePageSlider)) { Source = Config.Current.Slider };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Slider.IsHidePageSlider ? Properties.Resources.CommandToggleHidePageSliderOff : Properties.Resources.CommandToggleHidePageSliderOn;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.ToggleHidePageSlider();
        }
    }
}
