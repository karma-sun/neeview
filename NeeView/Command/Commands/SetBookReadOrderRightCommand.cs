using System.Windows.Data;


namespace NeeView
{
    public class SetBookReadOrderRightCommand : CommandElement
    {
        public SetBookReadOrderRightCommand() : base(CommandType.SetBookReadOrderRight)
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

        public override bool CanExecute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookSettingPresenter.Current.SetBookReadOrder(PageReadOrder.RightToLeft);
        }
    }
}
