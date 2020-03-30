using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class ScriptCommand : CommandElement
    {
        public static string Prefix => "Script_";
        public static string Extension => ".nvjs";
        public static string EventOnBookLoaded => "OnBookLoaded";

        private static Regex _regexDocName = new Regex(@"^/+\s*@name(.+)$", RegexOptions.Compiled);
        private static Regex _regexDocDescription = new Regex(@"^/+\s*@description(.+)$", RegexOptions.Compiled);


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
            var commandHost = new CommandHost(CommandTable.Current, ConfigMap.Current);
            var commandEngine = new JavascriptEngine(commandHost);
            commandEngine.LogAction = e => Debug.WriteLine(e);

            try
            {
                var path = Path.Combine(GetScriptFileName());
                commandEngine.ExecureFile(path);
            }
            catch (Exception ex)
            {
                commandEngine.Log(ex.Message);
                ToastService.Current.Show(new Toast(ex.Message, $"Script error in {_scriptName + Extension}", ToastIcon.Error));
            }
            finally
            {
                CommandTable.Current.FlushInputGesture();
            }
        }

        public string GetScriptFileName()
        {
            return Path.Combine(Config.Current.Script.GetCurrentScriptFolder(), _scriptName + Extension);
        }

        public void LoadDocComment()
        {
            using (var reader = new StreamReader(GetScriptFileName()))
            {
                bool isComment = false;
                string line;
                while ((line = reader.ReadLine()?.Trim()) != null)
                {
                    if (line.StartsWith("//"))
                    {
                        isComment = true;
                        var matchName = _regexDocName.Match(line);
                        if (matchName.Success)
                        {
                            Text = matchName.Groups[1].Value.Trim();
                            continue;
                        }

                        var matchDescription = _regexDocDescription.Match(line);
                        if (matchDescription.Success)
                        {
                            Note = matchDescription.Groups[1].Value.Trim();
                            continue;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(line) && !isComment)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
