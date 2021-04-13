using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NeeView
{
    public static class CultureInfoTools
    {
        public static CultureInfo GetBetterCulture(CultureInfo cultureInfo)
        {
            if (Environment.Cultures.IndexOf(cultureInfo.Name) >= 0)
            {
                return cultureInfo;
            }

            if (cultureInfo.Parent != CultureInfo.InvariantCulture)
            {
                return GetBetterCulture(cultureInfo.Parent);
            }

            return CultureInfo.GetCultureInfo("en");
        }
    }

}
