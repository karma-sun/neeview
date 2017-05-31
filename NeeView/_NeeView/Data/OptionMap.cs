// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NeeView.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionBaseAttribute : Attribute
    {
        public string HelpText { get; set; }
    }



    /// <summary>
    /// 
    /// </summary>
    public class OptionMap<T>
        where T : class, new()
    {
        // options
        private List<OptionMemberElement> _elements;

        // values
        private OptionValuesElement _values;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="source"></param>
        public OptionMap()
        {
            var type = typeof(T);

            _elements = new List<OptionMemberElement>();

            foreach (PropertyInfo info in type.GetProperties())
            {
                var attribute = (OptionBaseAttribute)Attribute.GetCustomAttributes(info, typeof(OptionBaseAttribute)).FirstOrDefault();
                if (attribute != null)
                {
                    switch (attribute)
                    {
                        case OptionMemberAttribute memberAttribute:
                            _elements.Add(new OptionMemberElement(info, memberAttribute));
                            break;
                        case OptionValuesAttribute valuesAttribute:
                            _values = new OptionValuesElement(info, valuesAttribute);
                            break;
                    }
                }
            }
        }

        //
        public OptionMemberElement GetElement(string key)
        {
            var word = key.TrimStart('-');

            if (key.StartsWith("--"))
            {
                return _elements.FirstOrDefault(e => e.LongName == word);
            }
            else
            {
                return _elements.FirstOrDefault(e => e.ShortName == word);
            }
        }

        //
        public string GetHelpText()
        {
            string text = "";
            foreach (var element in _elements)
            {
                // key
                var keys = new List<string> { element.ShortName != null ? "-" + element.ShortName : null, element.LongName != null ? "--" + element.LongName : null };
                var key = string.Join(", ", keys.Where(e => e != null));

                // key value
                string keyValue = element.GetValuePrototpye();
                if (!element.HasParameter)
                {
                    keyValue = "";
                }
                else if (element.Default == null)
                {
                    keyValue = $"<{keyValue}>";
                }
                else
                {
                    keyValue = $"[={keyValue}]";
                }

                text += $"{key} {keyValue}\n                {element.HelpText}\n";
            }

            return text;
        }


        //
        public T ParseArguments(string[] args)
        {
            // 字句解析
            var tokens = new List<string>();
            var regex = new Regex(@"^([^=]+)=(.+)$");

            foreach (var arg in args)
            {
                var match = regex.Match(arg);
                if (match.Success)
                {
                    tokens.AddRange(GetKeys(match.Groups[1].Value));
                    tokens.Add(match.Groups[2].Value);
                }
                else
                {
                    tokens.AddRange(GetKeys(arg));
                }
            }

            // 構文解析
            var options = new Dictionary<string, string>();
            var values = new List<string>();

            for (int i = 0; i < tokens.Count; ++i)
            {
                var token = tokens[i];
                var next = i + 1 < tokens.Count ? tokens[i + 1] : null;

                if (token.StartsWith("-"))
                {
                    var element = GetElement(token);
                    if (element == null) throw new ArgumentException($"{token} は未知のオプションです。");

                    if (next == null || next.StartsWith("-") || !element.HasParameter)
                    {
                        options.Add(token, null);
                    }
                    else
                    {
                        options.Add(token, next);
                        i++;
                    }
                }
                else
                {
                    values.Add(token);
                }
            }

            // マッピング
            var target = new T();
            Mapping(target, options, values);

            return target;
        }

        //
        private List<string> GetKeys(string keys)
        {
            if (keys.StartsWith("--"))
            {
                return new List<string>() { keys };
            }
            else if (keys.StartsWith("-"))
            {
                return keys.TrimStart('-').Select(e => "-" + e).ToList();
            }
            else
            {
                return new List<string>() { keys };
            }
        }

        //
        private void Mapping(T source, Dictionary<string, string> options, List<string> values)
        {
            foreach (var item in options)
            {
                var element = GetElement(item.Key);
                if (element == null) throw new ArgumentException($"{item.Key} は未知のオプションです。");

                var value = item.Value ?? element.Default;
                if (value == null) throw new ArgumentException($"{item.Key} には引数が必要です。");

                try
                {
                    element.SetValue(source, value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw new ArgumentException($"{item.Key} の引数として {value} は使用できません。");
                }
            }

            _values?.SetValues(source, values);
        }
    }


}
