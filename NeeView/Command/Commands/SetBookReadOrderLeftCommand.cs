using System.Windows.Data;


namespace NeeView
{
    public class SetBookReadOrderLeftCommand : CommandElement
    {
        public SetBookReadOrderLeftCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BookReadOrder(PageReadOrder.LeftToRight);
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetBookReadOrder(PageReadOrder.LeftToRight);
        }
    }
}
