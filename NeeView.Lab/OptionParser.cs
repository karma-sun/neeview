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

namespace NeeView.CommandLine
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionBaseAttribute : Attribute
    {
        public string HelpText { get; set; }
    }

    //
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionMemberAttribute : OptionBaseAttribute
    {
        // short option name
        public string ShortName { get; set; }

        // long option name
        public string LongName { get; set; }

        // 引数を省略した場合の既定値
        public string Default { get; set; }

        public bool HasParameter { get; set; }

        public OptionMemberAttribute() { }
        public OptionMemberAttribute(string shortName, string longName)
        {
            ShortName = shortName;
            LongName = longName;
        }
    }

    //
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionValuesAttribute : OptionBaseAttribute
    {
    }


    //
    public class OptionMemberElement
    {
        public string LongName => _attribute.LongName;
        public string ShortName => _attribute.ShortName;
        public string Default => _attribute.Default;
        public bool HasParameter => _attribute.HasParameter;
        public string HelpText => _attribute.HelpText;

        public string PropertyName => _info.Name;

        private PropertyInfo _info;
        private OptionMemberAttribute _attribute;


        public OptionMemberElement(PropertyInfo info, OptionMemberAttribute attribute)
        {
            _info = info;
            _attribute = attribute;
        }

        /// <summary>
        /// オプション引数指定可能値を取得
        /// ヘルプ用
        /// </summary>
        /// <returns></returns>
        public string GetValuePrototpye()
        {
            if (_info.PropertyType.IsEnum)
            {
                return string.Join("|", Enum.GetNames(_info.PropertyType));
            }

            Type nullable = Nullable.GetUnderlyingType(_info.PropertyType);
            if ((nullable != null) && nullable.IsEnum)
            {
                return string.Join("|", Enum.GetNames(nullable));
            }

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.String:
                    return "string";
                case TypeCode.Int32:
                    return "number";
                case TypeCode.Double:
                    return "number";
                default:
                    throw new NotSupportedException($"{_info.PropertyType} はサポート外の引数型です。");
            }
        }

        //
        public void SetValue(object _source, string value)
        {
            if (_info.PropertyType.IsEnum)
            {
                _info.SetValue(_source, Enum.Parse(_info.PropertyType, value));
                return;
            }

            Type nullable = Nullable.GetUnderlyingType(_info.PropertyType);
            if ((nullable != null) && nullable.IsEnum)
            {
                _info.SetValue(_source, Enum.Parse(nullable, value));
                return;
            }

            TypeCode typeCode = Type.GetTypeCode(_info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    _info.SetValue(_source, bool.Parse(value));
                    break;
                case TypeCode.String:
                    _info.SetValue(_source, value);
                    break;
                case TypeCode.Int32:
                    _info.SetValue(_source, int.Parse(value));
                    break;
                case TypeCode.Double:
                    _info.SetValue(_source, double.Parse(value));
                    break;
                default:
                    throw new NotSupportedException($"{_info.PropertyType.Name} はサポート外の型です。");
            }
        }

    }

    //
    public class OptionValuesElement
    {
        private PropertyInfo _info;
        private OptionValuesAttribute _attribute;

        //
        public OptionValuesElement(PropertyInfo info, OptionValuesAttribute attribute)
        {
            _info = info;
            _attribute = attribute;

            if (info.PropertyType != typeof(List<string>)) throw new InvalidOperationException("OptionValues属性のプロパティはList<string>型でなければいけません");
        }

        //
        public void SetValues(object source, List<string> values)
        {
            _info.SetValue(source, values);
        }
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
            foreach (var element in this._elements.OrderBy(e => e.LongName))
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
                    if (element == null) throw new ArgumentException($"{token} というオプションは存在しません。");

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




    #region Sample

    public enum SwitchOption
    {
        on,
        off,
    }

    public class SampleOption
    {
        [OptionMember("x", "setting", HasParameter = true, HelpText = "設定ファイル(UserSetting.xml)のパスを指定します")]
        public string SettingFilename { get; set; }

        [OptionMember("r", "reset-placement", Default = "on", HelpText = "ウィンドウ座標を初期化します")]
        public SwitchOption IsResetPlacement { get; set; }

        [OptionMember("b", "blank", Default = "on", HelpText = "画像ファイルを開かずに起動します")]
        public SwitchOption IsBlank { get; set; }

        [OptionMember("n", "new-window", Default = "on", HasParameter = true, HelpText = "新しいウィンドウで起動するかを指定します")]
        public SwitchOption? IsNewWindow { get; set; }

        [OptionMember("f", "fullscrreen", Default = "on", HasParameter = true, HelpText = "フルスクリーンで起動するかを指定します")]
        public SwitchOption? IsFullScreen { get; set; }

        [OptionMember("s", "slideshow", Default = "on", HasParameter = true, HelpText = "スライドショウを開始するかを指定します")]
        public SwitchOption? IsSlideShow { get; set; }

        [OptionMember("h", "help", Default = "true", HelpText = "このヘルプを表示します")]
        public bool IsHelp { get; set; }

        [OptionValues]
        public List<string> Values { get; set; }
    }


    public class CommandLineHelpException : Exception
    {
    }

    //
    public class SampleOptionTest
    {
        public SampleOption Exec(string[] args)
        {
            var optionMap = new OptionMap<SampleOption>();

            try
            {
                //
                var option = optionMap.ParseArguments(args);

                if (option.IsHelp)
                {
                    throw new CommandLineHelpException();
                }

                return option;
            }
            catch (Exception ex)
            {
                if (!(ex is CommandLineHelpException))
                {
                    Debug.WriteLine(ex.Message + "\n\n");
                }
                ShowCommandLineHelp(optionMap);
                throw;
            }
        }

        private void ShowCommandLineHelp(OptionMap<SampleOption> parser)
        {
            string text = "Usage: NeeView.exe [options] [file or folder]\n\n";
            text += parser.GetHelpText();

            Debug.WriteLine(text);
        }
    }


    #endregion
}
