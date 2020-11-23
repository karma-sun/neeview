using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByFileNameACommand : CommandElement
    {
        public SetBookOrderByFileNameACommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByFileNameA;
            this.Note = Properties.Resources.CommandSetFolderOrderByFileNameANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileName);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileName);
        }
    }
}
