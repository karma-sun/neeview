using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByTimeStampACommand : CommandElement
    {
        public SetFolderOrderByTimeStampACommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByTimeStampA;
            this.Note = Properties.Resources.CommandSetFolderOrderByTimeStampANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.TimeStamp);
        }

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.TimeStamp);
        }
    }
}
