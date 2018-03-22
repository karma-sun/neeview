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

            if (info.PropertyType != typeof(List<string>)) throw new InvalidOperationException("The property of the OptionValues ​​attribute must be of type List<string>");
        }

        //
        public void SetValues(object source, List<string> values)
        {
            _info.SetValue(source, values);
        }
    }


}
