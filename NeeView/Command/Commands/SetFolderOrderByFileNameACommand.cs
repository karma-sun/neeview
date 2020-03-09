using System.Windows.Data;


namespace NeeView
{
    public class SetFolderOrderByFileNameACommand : CommandElement
    {
        public SetFolderOrderByFileNameACommand(string name) : base(name)
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

        public override void Execute(CommandParameter param, object arg, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.FileName);
        }
    }
}
