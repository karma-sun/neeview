using System.Windows.Data;


namespace NeeView
{
    public class SetBookReadOrderRightCommand : CommandElement
    {
        public SetBookReadOrderRightCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetBookReadOrderRight;
            this.Note = Properties.Resources.CommandSetBookReadOrderRightNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BookReadOrder(PageReadOrder.RightToLeft);
        }

        public override bool CanExecute(CommandParameter param, object arg, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookSettingPresenter.Current.SetBookReadOrder(PageReadOrder.RightToLeft);
        }
    }
}
