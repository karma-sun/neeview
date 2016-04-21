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
    public class ReplaceString
    {
        //
        private class ReplaceUnit
        {
            public Regex Regex { get; set; }
            public string ReplaceString { get; set; }

            public ReplaceUnit(string key, string replaceString)
            {
                Regex = new Regex("\\" + key + "\\b");
                ReplaceString = replaceString;
            }

            public string Replace(string s)
            {
                return Regex.Replace(s, ReplaceString);
            }
        }

        //
        private Dictionary<string, ReplaceUnit> _Dictionary;

        //
        public ReplaceString()
        {
            _Dictionary = new Dictionary<string, ReplaceUnit>();
        }

        //
        public void Set(string key, string replaceString)
        {
            _Dictionary[key] = new ReplaceUnit(key, replaceString);
        }

        //
        public string Replace(string s)
        {
            foreach (var regexUnit in _Dictionary.Values)
            {
                s = regexUnit.Replace(s);
            }
            return s;
        }
    }

}
