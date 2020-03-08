using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByFileTypeACommand : CommandElement
    {
        public SetFolderOrderByFileTypeACommand(string name) : base(name)
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

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileType);
        }
    }
}
