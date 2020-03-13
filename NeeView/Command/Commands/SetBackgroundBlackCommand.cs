﻿using System.Windows.Data;


namespace NeeView
{
    public class SetBackgroundBlackCommand : CommandElement
    {
        public SetBackgroundBlackCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupEffect;
            this.Text = Properties.Resources.CommandSetBackgroundBlack;
            this.Note = Properties.Resources.CommandSetBackgroundBlackNote;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.Background(BackgroundStyle.Black);
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            ContentCanvasBrush.Current.Background = BackgroundStyle.Black;
        }
    }
}