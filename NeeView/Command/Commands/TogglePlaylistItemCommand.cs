using System;
using System.Windows.Data;


namespace NeeView
{
    public class TogglePlaylistItemCommand : CommandElement
    {
        public TogglePlaylistItemCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Playlist;
            this.ShortCutKey = "Ctrl+M";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookOperation.Current.IsMarked)) { Source = BookOperation.Current, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return BookOperation.Current.IsMarked ? Properties.Resources.TogglePlaylistItemCommand_Off : Properties.Resources.TogglePlaylistItemCommand_On;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return BookOperation.Current.CanMark();
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookOperation.Current.SetMark(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookOperation.Current.ToggleMark();
            }
        }
    }
}
