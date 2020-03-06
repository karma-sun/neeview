using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderBySizeDCommand : CommandElement
    {
        public SetFolderOrderBySizeDCommand() : base(CommandType.SetFolderOrderBySizeD)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderBySizeD;
            this.Note = Properties.Resources.CommandSetFolderOrderBySizeDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.SizeDescending);
        }
        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.SizeDescending);
        }
    }
}
