using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByFileTypeDCommand : CommandElement
    {
        public SetFolderOrderByFileTypeDCommand(string name) : base(name)
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

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileTypeDescending);
        }
    }
}
