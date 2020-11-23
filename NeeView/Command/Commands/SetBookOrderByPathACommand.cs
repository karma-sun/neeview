using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByPathACommand : CommandElement
    {
        public SetBookOrderByPathACommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByPathA;
            this.Note = Properties.Resources.CommandSetFolderOrderByPathANote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Path);
        }

        public override void Execute(object sender, CommandContext e)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Path);
        }
    }
}
