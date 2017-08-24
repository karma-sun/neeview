// Copyright (c) 2016-2017 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeeView.Data
{
    //
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionValuesAttribute : OptionBaseAttribute
    {
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


}
