using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeRandomCommand : CommandElement
    {
        public SetSortModeRandomCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageOrder;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.Random);
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.Random);
        }
    }
}
