using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundAutoCommand : CommandElement
    {
        public SetBackgroundAutoCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundAuto;
            this.Note = Properties.Resources.CommandSetBackgroundAutoNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundType.Auto);
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.Background.BackgroundType = BackgroundType.Auto;
        }
    }
}
