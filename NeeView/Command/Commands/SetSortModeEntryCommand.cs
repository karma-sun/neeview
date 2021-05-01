using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeEntryCommand : CommandElement
    {
        public SetSortModeEntryCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageOrder;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.Entry);
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading && BookOperation.Current.PageSortModeClass.Contains(PageSortMode.Entry);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.Entry);
        }
    }
}
