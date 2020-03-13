using System.Windows.Data;


namespace NeeView
{
    public class SetBookOrderByRandomCommand : CommandElement
    {
        public SetBookOrderByRandomCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookOrder;
            this.Text = Properties.Resources.CommandSetFolderOrderByRandom;
            this.Note = Properties.Resources.CommandSetFolderOrderByRandomNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.FolderOrder(FolderOrder.Random);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            BookshelfFolderList.Current.SetFolderOrder(FolderOrder.Random);
        }
    }
}
