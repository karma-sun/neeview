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

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.TimeStampDescending);
        }
    }
}
