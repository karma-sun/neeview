using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundCheckCommand : CommandElement
    {
        public SetBackgroundCheckCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundCheck;
            this.Note = Properties.Resources.CommandSetBackgroundCheckNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundType.Check);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            Config.Current.Layout.Background.BackgroundType = BackgroundType.Check;
        }
    }
}
