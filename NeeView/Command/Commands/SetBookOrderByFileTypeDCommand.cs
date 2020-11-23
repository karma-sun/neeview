using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByFileTypeDCommand : CommandElement
    {
        public SetBookOrderByFileTypeDCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByFileTypeD;
            this.Note = Properties.Resources.CommandSetFolderOrderByFileTypeDNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.FileTypeDescending);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileTypeDescending);
        }
    }
}
