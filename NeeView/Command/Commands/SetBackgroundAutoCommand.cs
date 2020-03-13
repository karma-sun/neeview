﻿using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundAutoCommand : CommandElement
    {
        public SetBackgroundAutoCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundAuto;
            this.Note = Properties.Resources.CommandSetBackgroundAutoNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundStyle.Auto);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.Auto;
        }
    }
}