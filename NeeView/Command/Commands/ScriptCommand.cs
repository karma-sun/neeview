using System;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    public class ScriptCommand : CommandElement
    {
        public static string Prefix => "Script.";
        public static string Extension => ".js";

        private string _filename;

        public ScriptCommand(string name) : base(name)
        {
            if (!name.StartsWith(Prefix)) throw new ArgumentException($"{nameof(name)} must start with '{Prefix}'");
            _filename = name.Substring(Prefix.Length) + Extension;

            this.Group = Properties.Resources.CommandGroupScript;
            this.Text = name;
        }

        public override void Execute(CommandParameter param, CommandOption option = CommandOption.None)
        {
            var commandHost = new CommandHost(CommandTable.Current);
            var commandEngine = new JavascriptEngine(commandHost);
            commandEngine.LogAction = e => Debug.WriteLine(e);

            try
            {
                var path = Path.Combine(CommandTable.Current.ScriptFolder ?? "", _filename);
                commandEngine.ExecureFile(path);
            }
            catch (Exception ex)
            {
                commandEngine.Log(ex.Message);
            }
        }
    }
}
