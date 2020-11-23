using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderBySizeDCommand : CommandElement
    {
        public SetBookOrderBySizeDCommand(string name) : base(name)
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
        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.SizeDescending);
        }
    }
}
