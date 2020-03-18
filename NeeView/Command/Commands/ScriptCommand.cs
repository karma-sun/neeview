using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    public class ScriptCommand : CommandElement
    {
        public static string Prefix => "Script.";
        public static string Extension => ".nvjs";
        public static string EventOnBookLoaded => "OnBookLoaded";


        private string _scriptName;

        public ScriptCommand(string name) : base(name)
        {
            if (!name.StartsWith(Prefix)) throw new ArgumentException($"{nameof(name)} must start with '{Prefix}'");
            _scriptName = name.Substring(Prefix.Length);

            this.Group = Properties.Resources.CommandGroupScript;
            this.Text = _scriptName;

            if (_scriptName == EventOnBookLoaded)
            {
                this.Note = Properties.Resources.CommandScriptOnBookLoadedNote;
            }
            else
            {
                this.Note = Properties.Resources.CommandScriptNote;
            }
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            var commandHost = new CommandHost(CommandTable.Current);
            var commandEngine = new JavascriptEngine(commandHost);
            commandEngine.LogAction = e => Debug.WriteLine(e);

            var filename = _scriptName + Extension;
            try
            {
                var path = Path.Combine(Config.Current.Script.GetCurrentScriptFolder(), filename);
                commandEngine.ExecureFile(path);
            }
            catch (Exception ex)
            {
                commandEngine.Log(ex.Message);
                ToastService.Current.Show(new Toast(ex.Message, $"Script error in {filename}", ToastIcon.Error));
            }
            finally
            {
                CommandTable.Current.FlushInputGesture();
            }
        }
    }
}
