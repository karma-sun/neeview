using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderBySizeACommand : CommandElement
    {
        public SetFolderOrderBySizeACommand(string name) : base(name)
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

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Size);
        }
    }
}
