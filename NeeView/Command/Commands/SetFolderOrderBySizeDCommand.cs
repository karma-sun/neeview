using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderBySizeDCommand : CommandElement
    {
        public SetFolderOrderBySizeDCommand(string name) : base(name)
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
        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.SizeDescending);
        }
    }
}
