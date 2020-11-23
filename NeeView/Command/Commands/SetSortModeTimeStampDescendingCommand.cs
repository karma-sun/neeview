using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeTimeStampDescendingCommand : CommandElement
    {
        public SetSortModeTimeStampDescendingCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeTimeStampDescending;
            this.Note = Properties.Resources.CommandSetSortModeTimeStampDescendingNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.TimeStampDescending);
        }
        
        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }
        
        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.TimeStampDescending);
        }
    }
}
