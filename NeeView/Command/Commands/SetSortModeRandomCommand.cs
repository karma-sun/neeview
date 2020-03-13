using System.Windows.Data;


namespace NeeView
{
    public class SetSortModeRandomCommand : CommandElement
    {
        public SetSortModeRandomCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageOrder;
            this.Text = Properties.Resources.CommandSetSortModeRandom;
            this.Note = Properties.Resources.CommandSetSortModeRandomNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.SortMode(PageSortMode.Random);
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookSettingPresenter.Current.SetSortMode(PageSortMode.Random);
        }
    }
}
