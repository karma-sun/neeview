using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByFileTypeDCommand : CommandElement
    {
        public SetFolderOrderByFileTypeDCommand() : base("SetFolderOrderByFileTypeD")
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

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileTypeDescending);
        }
    }
}
