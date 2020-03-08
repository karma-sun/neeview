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
            return BindingGenerator.Background(BackgroundStyle.White);
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.White;
        }
    }
}
