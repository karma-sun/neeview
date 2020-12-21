using System.Windows.Data;


namespace NeeView
{
    public class SetPageModeOneCommand : CommandElement
    {
        public SetPageModeOneCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.ShortCutKey = "Ctrl+1";
            this.MouseGesture = "RU";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.SinglePage);
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetPageMode(PageMode.SinglePage);
        }
    }
}
