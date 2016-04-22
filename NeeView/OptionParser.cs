// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// オプションの種類
    /// </summary>
    public enum OptionType
    {
        None,
        String,
        FileName,
        Bool,
    }

    /// <summary>
    /// オプション設定
    /// </summary>
    public class OptionUnit
    {
        public string Name { get; set; }
        public OptionType Type { get; set; }
        public string Exception { get; set; }

        public string Value { get; set; }

        //
        public bool IsValid => Value != null;

        //
        public override string ToString()
        {
            return $"{Name} = {Value ?? "null"}";
        }


        //
        public OptionUnit(string name, OptionType type, string exception)
        {
            Name = name;
            Type = type;
            Exception = exception;
        }

        //
        public bool Bool
        {
            get
            {
                if (Type != OptionType.Bool) throw new NotSupportedException($"{Name}オプションはBoolをサポートしません");
                return Value == "on";
            }
        }

        //
        public void Set(string value)
        {
            switch (Type)
            {
                case OptionType.None:
                    if (value != "") throw new ArgumentException($"{Name}オプションの引数は不要です");
                    break;

                case OptionType.String:
                case OptionType.FileName:
                    if (value == "") throw new ArgumentException($"{Name}オプションの引数が不正です");
                    break;

                case OptionType.Bool:
                    value = (value == "") ? "on" : value;
                    if (value != "on" && value != "off") throw new ArgumentException($"{Name}オプションの引数が不正です");
                    break;
            }

            Value = value;
        }

        //
        public string HelpText
        {
            get
            {
                switch (Type)
                {
                    case OptionType.None:
                        return $"{Name}\n\t{Exception}";

                    case OptionType.String:
                        return $"{Name}=Parameter\n\t{Exception}";

                    case OptionType.FileName:
                        return $"{Name}=FileName\n\t{Exception}";

                    case OptionType.Bool:
                        return $"{Name}[=on|off]\n\t{Exception}";

                    default:
                        throw new NotImplementedException();
                }
            }
        }

    }


    /// <summary>
    /// オプション解析
    /// </summary>
    public class OptionParser
    {
        public Dictionary<string, OptionUnit> Options;
        public List<string> Args;

        public OptionParser()
        {
            Args = new List<string>();
            Options = new Dictionary<string, OptionUnit>();

            //AddOption("--help", OptionType.None, "このヘルプを表示します");
        }

        public void AddOption(string key, OptionType type, string exception)
        {
            Options.Add(key, new OptionUnit(key, type, exception));
        }

        private void SetOptionValue(string key, string value)
        {
            if (!Options.ContainsKey(key)) throw new ArgumentException($"{key}というオプションは存在しません");
            Options[key].Set(value);
        }

        public bool IsHelp => Options["--help"].IsValid;

        public string HelpText
        {
            get
            {
                string text = "";
                foreach (var option in Options.Values.OrderBy(e => e.Name))
                {
                    text += option.HelpText + "\n";
                }

                return text;
            }
        }

        // 引数チェック
        public void Parse(string[] args)
        {
            var keyRegex = new Regex(@"^(?<key>[\w-]+)$");
            var keyValueRegex = new Regex(@"^(?<key>[\w-]+)=(?<value>.+)$");

            foreach (var arg in args)
            {
                var matchKey = keyRegex.Match(arg);
                if (matchKey.Success)
                {
                    SetOptionValue(matchKey.Groups["key"].Value, "");
                    continue;
                }

                var matchKeyValue = keyValueRegex.Match(arg);
                if (matchKeyValue.Success)
                {
                    SetOptionValue(matchKeyValue.Groups["key"].Value, matchKeyValue.Groups["value"].Value);
                }

                else
                {
                    Args.Add(arg);
                }
            }

        }
    }

}
