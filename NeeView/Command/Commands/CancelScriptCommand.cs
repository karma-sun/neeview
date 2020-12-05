﻿namespace NeeView
{
    public class CancelScriptCommand : CommandElement
    {
        public CancelScriptCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupScript;
            this.Text = Properties.Resources.CommandCancelScript;
            this.Note = Properties.Resources.CommandCancelScriptNote;
            this.IsShowMessage = false;
        }

        public override void Execute(object sender, CommandContext e)
        {
            CommandTable.Current.CancelScript();
        }
    }
}