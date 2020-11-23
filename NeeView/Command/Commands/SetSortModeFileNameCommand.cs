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
        
        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.FileName);
        }
    }
}
