using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderBySizeACommand : CommandElement
    {
        public SetBookOrderBySizeACommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderBySizeA;
            this.Note = Properties.Resources.CommandSetFolderOrderBySizeANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Size);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Size);
        }
    }
}
