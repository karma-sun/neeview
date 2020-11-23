using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeFileNameDescendingCommand : CommandElement
    {
        public SetSortModeFileNameDescendingCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeFileNameDescending;
            this.Note = Properties.Resources.CommandSetSortModeFileNameDescendingNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.FileNameDescending);
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.FileNameDescending);
        }
    }
}
