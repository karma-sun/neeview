using System.Windows.Data;


namespace NeeView
{
    public class SetBookReadOrderLeftCommand : CommandElement
    {
        public SetBookReadOrderLeftCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupPageSetting;
            this.Text = Properties.Resources.CommandSetBookReadOrderLeft;
            this.Note = Properties.Resources.CommandSetBookReadOrderLeftNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BookReadOrder(PageReadOrder.LeftToRight);
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetBookReadOrder(PageReadOrder.LeftToRight);
        }
    }
}
