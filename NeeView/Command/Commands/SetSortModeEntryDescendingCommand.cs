using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeEntryDescendingCommand : CommandElement
    {
        public SetSortModeEntryDescendingCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageOrder;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.EntryDescending);
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading && BookOperation.Current.PageSortModeClass.Contains(PageSortMode.EntryDescending);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.EntryDescending);
        }
    }

}
