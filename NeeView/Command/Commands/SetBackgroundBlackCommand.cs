using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundBlackCommand : CommandElement
    {
        public SetBackgroundBlackCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Effect;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundType.Black);
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.Background.BackgroundType = BackgroundType.Black;
        }
    }
}
