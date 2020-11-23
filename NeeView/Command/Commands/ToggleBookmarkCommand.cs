using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleBookmarkCommand : CommandElement
    {
        public ToggleBookmarkCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupBookmark;
            this.Text = Properties.Resources.CommandToggleBookmark;
            this.MenuText = Properties.Resources.CommandToggleBookmarkMenu;
            this.Note = Properties.Resources.CommandToggleBookmarkNote;
            this.ShortCutKey = "Ctrl+D";
            this.IsShowMessage = true;
        }
        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.IsBookmark)) { Source = BookOperation.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookOperation.Current.IsBookmark ? Properties.Resources.CommandToggleBookmarkOff : Properties.Resources.CommandToggleBookmarkOn;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanBookmark();
        }
        
        [MethodArgument("@CommandToggleArgument")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookOperation.Current.SetBookmark(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookOperation.Current.ToggleBookmark();
            }
        }
    }
}
