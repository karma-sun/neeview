﻿namespace NeeView
{
    public class LoupeScaleUpCommand : CommandElement
    {
        public LoupeScaleUpCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupViewManipulation;
            this.Text = Properties.Resources.CommandLoupeScaleUp;
            this.Note = Properties.Resources.CommandLoupeScaleUpNote;
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return ViewComponentProvider.Current.GetViewController(sender).GetLoupeMode();

        }

        public override void Execute(object sender, CommandContext e)
        {
            ViewComponentProvider.Current.GetViewController(sender).LoupeZoomIn();
        }
    }
}
