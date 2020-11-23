using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByTimeStampACommand : CommandElement
    {
        public SetBookOrderByTimeStampACommand(string name) : base(name)
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

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.TimeStamp);
        }
    }
}
