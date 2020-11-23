using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByFileTypeACommand : CommandElement
    {
        public SetBookOrderByFileTypeACommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByFileTypeA;
            this.Note = Properties.Resources.CommandSetFolderOrderByFileTypeANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileType);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileType);
        }
    }
}
