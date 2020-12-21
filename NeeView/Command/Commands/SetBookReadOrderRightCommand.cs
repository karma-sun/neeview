using System.Windows.Data;


namespace NeeView
{
    public class SetBookReadOrderRightCommand : CommandElement
    {
        public SetBookReadOrderRightCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BookReadOrder(PageReadOrder.RightToLeft);
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetBookReadOrder(PageReadOrder.RightToLeft);
        }
    }
}
