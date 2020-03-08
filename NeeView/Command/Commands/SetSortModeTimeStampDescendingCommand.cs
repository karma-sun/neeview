using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeTimeStampDescendingCommand : CommandElement
    {
        public SetSortModeTimeStampDescendingCommand() : base("SetSortModeTimeStampDescending")
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
        
        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }
        
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.TimeStampDescending);
        }
    }
}
