using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeTimeStampCommand : CommandElement
    {
        public SetSortModeTimeStampCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeTimeStamp;
            this.Note = Properties.Resources.CommandSetSortModeTimeStampNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.TimeStamp);
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.TimeStamp);
        }
    }
}
