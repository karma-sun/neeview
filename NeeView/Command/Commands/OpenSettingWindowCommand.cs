﻿namespace NeeView
{
    public class OpenSettingWindowCommand : CommandElement
    {
        public OpenSettingWindowCommand() : base("OpenSettingWindow")
        {
            this.Group = Properties.Resources.CommandGroupOther;
            this.Text = Properties.Resources.CommandOpenSettingWindow;
            this.MenuText = Properties.Resources.CommandOpenSettingWindowMenu;
            this.Note = Properties.Resources.CommandOpenSettingWindowNote;
            this.IsShowMessage = false;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            MainWindowModel.Current.OpenSettingWindow();
        }
    }
}
