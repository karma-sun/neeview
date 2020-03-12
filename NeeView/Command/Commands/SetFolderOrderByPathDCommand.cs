using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByPathDCommand : CommandElement
    {
        public SetFolderOrderByPathDCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByPathD;
            this.Note = Properties.Resources.CommandSetFolderOrderByPathDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.PathDescending);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.PathDescending);
        }
    }
}
