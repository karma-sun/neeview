using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByPathDCommand : CommandElement
    {
        public SetBookOrderByPathDCommand(string name) : base(name)
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

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.PathDescending);
        }
    }
}
