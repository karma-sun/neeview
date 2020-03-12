using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeSizeDescendingCommand : CommandElement
    {
        public SetSortModeSizeDescendingCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeSizeDescending;
            this.Note = Properties.Resources.CommandSetSortModeSizeDescendingNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.SizeDescending);
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.SizeDescending);
        }
    }
}
