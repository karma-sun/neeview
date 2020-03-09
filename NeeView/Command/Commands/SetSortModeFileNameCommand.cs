using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeFileNameCommand : CommandElement
    {
        public SetSortModeFileNameCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeFileName;
            this.Note = Properties.Resources.CommandSetSortModeFileNameNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.FileName);
        }
        
        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.FileName);
        }
    }
}
