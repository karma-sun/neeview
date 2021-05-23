using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class ScriptCommand : CommandElement
    {
        public const string Prefix = "Script_";
        public const string Extension = ".nvjs";
        public const string EventOnBookLoaded = Prefix + "OnBookLoaded";
        public const string EventOnPageChanged = Prefix + "OnPageChanged";

        private static Regex _regexCommentLine = new Regex(@"^\s*/{2,}");
        private static Regex _regexDocComment = new Regex(@"^\s*/{2,}\s*(@\w+)\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        private string _scriptName;

        public ScriptCommand(string name) : base(name)
        {
            if (!name.StartsWith(Prefix)) throw new ArgumentException($"{nameof(name)} must start with '{Prefix}'");
            _scriptName = name.Substring(Prefix.Length);

            this.Group = Properties.Resources.CommandGroup_Script;
            this.Text = _scriptName;

            switch (name)
            {
                case EventOnBookLoaded:
                    this.IsCloneable = false;
                    this.Remarks = Properties.Resources.ScriptOnBookLoadedCommand_Remarks;
                    break;

                case EventOnPageChanged:
                    this.IsCloneable = false;
                    this.Remarks = Properties.Resources.ScriptOnPageChangedCommand_Remarks;
                    break;

                default:
                    this.Remarks = Properties.Resources.ScriptCommand_Remarks;
                    break;
            }

            this.ParameterSource = new CommandParameterSource(new ScriptCommandParameter());
        }

        protected override CommandElement CloneInstance()
        {
            var type = this.GetType();
            var command = new ScriptCommand(this.NameSource.Name);
            command.Text = this.Text;
            command.Remarks = this.Remarks;
            return command;
        }

        public override void Execute(object sender, CommandContext e)
        {
            CommandTable.Current.ScriptManager.Execute(sender, GetScriptFileName(), ((ScriptCommandParameter)e.Parameter).Argument);
        }

        public string GetScriptFileName()
        {
            return Path.Combine(Config.Current.Script.ScriptFolder, _scriptName + Extension);
        }

        public void LoadDocComment()
        {
            using (var reader = new StreamReader(GetScriptFileName()))
            {
                bool isComment = false;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (_regexCommentLine.IsMatch(line))
                    {
                        isComment = true;
                        var match = _regexDocComment.Match(line);
                        if (match.Success)
                        {
                            var key = match.Groups[1].Value.ToLower();
                            var value = match.Groups[2].Value.Trim();
                            switch (key)
                            {
                                case "@name":
                                    Text = value;
                                    break;
                                case "@description":
                                    Remarks = value;
                                    break;
                                case "@shortcutkey":
                                    ShortCutKey = value;
                                    break;
                                case "@mousegesture":
                                    MouseGesture = value;
                                    break;
                                case "@touchgesture":
                                    TouchGesture = value;
                                    break;
                            }
                        }
                    }
                    else if (isComment && !string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }
                }
            }
        }

        public void OpenFile()
        {
            ExternalProcess.OpenWithTextEditor(GetScriptFileName());
        }
    }



    public class ScriptCommandParameter : CommandParameter
    {
        private string _argument;

        [PropertyMember]
        public string Argument
        {
            get { return _argument; }
            set { SetProperty(ref _argument, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }
    }
}
