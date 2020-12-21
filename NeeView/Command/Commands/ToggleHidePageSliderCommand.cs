using System.Windows.Data;


namespace NeeView
{
    public class ToggleHidePageSliderCommand : CommandElement
    {
        public ToggleHidePageSliderCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Window;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SliderConfig.IsHidePageSlider)) { Source = Config.Current.Slider };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return Config.Current.Slider.IsHidePageSlider ? Properties.Resources.ToggleHidePageSliderCommand_Off : Properties.Resources.ToggleHidePageSliderCommand_On;
        }

        public override void Execute(object sender, CommandContext e)
        {
            MainWindowModel.Current.ToggleHidePageSlider();
        }
    }
}
