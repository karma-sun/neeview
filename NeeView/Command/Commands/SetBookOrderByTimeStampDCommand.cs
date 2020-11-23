using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByTimeStampDCommand : CommandElement
    {
        public SetBookOrderByTimeStampDCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByTimeStampD;
            this.Note = Properties.Resources.CommandSetFolderOrderByTimeStampDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.TimeStampDescending);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.TimeStampDescending);
        }
    }
}
