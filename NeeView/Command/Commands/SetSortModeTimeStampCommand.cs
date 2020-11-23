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

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.TimeStamp);
        }
    }
}
