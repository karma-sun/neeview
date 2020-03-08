using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeSizeCommand : CommandElement
    {
        public SetSortModeSizeCommand() : base("SetSortModeSize")
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

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.Size);
        }
    }
}
