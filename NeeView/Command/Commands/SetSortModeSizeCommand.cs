using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeSizeCommand : CommandElement
    {
        public SetSortModeSizeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeSize;
            this.Note = Properties.Resources.CommandSetSortModeSizeNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.Size);
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.Size);
        }
    }
}
