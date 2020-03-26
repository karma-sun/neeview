using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundWhiteCommand : CommandElement
    {
        public SetBackgroundWhiteCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundWhite;
            this.Note = Properties.Resources.CommandSetBackgroundWhiteNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundType.White);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            Config.Current.Background.BackgroundType = BackgroundType.White;
        }
    }
}
