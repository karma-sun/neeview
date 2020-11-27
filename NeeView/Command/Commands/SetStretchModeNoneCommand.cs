using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeNoneCommand : CommandElement
    {
        public SetStretchModeNoneCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandSetStretchModeNone;
            this.Note = Properties.Resources.CommandSetStretchModeNoneNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.StretchMode(PageStretchMode.None);
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponent.Current.ViewController.SetStretchMode(PageStretchMode.None, false);
        }
    }
}
