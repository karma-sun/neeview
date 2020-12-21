using System.Windows.Data;


namespace NeeView
{
    public class SetStretchModeNoneCommand : CommandElement
    {
        public SetStretchModeNoneCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ImageScale;
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
            MainViewComponent.Current.ViewController.SetStretchMode(PageStretchMode.None, false);
        }
    }
}
