using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundCheckDarkCommand : CommandElement
    {
        public SetBackgroundCheckDarkCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundCheckDark;
            this.Note = Properties.Resources.CommandSetBackgroundCheckDarkNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundType.CheckDark);
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.Background.BackgroundType = BackgroundType.CheckDark;
        }
    }
}
