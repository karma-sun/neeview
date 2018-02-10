// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;

namespace NeeView
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AliasNameAttribute : Attribute
    {
        public string AliasName { get; private set; }

        public string Tips { get; set; }

        public AliasNameAttribute(string aliasName)
        {
            AliasName = aliasName;
        }
    }
}

