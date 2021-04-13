using System;
using System.IO;
using System.Windows.Resources;

namespace NeeView
{
    public static class ResourceTools
    {
        public static StreamResourceInfo GetCultureResource(string path)
        {
            try
            {
                var uri = GetClutureResoureUri(path, Config.Current.System.Language);
                return System.Windows.Application.GetResourceStream(uri);
            }
            catch (IOException)
            {
                var uri = GetClutureResoureUri(path, "en");
                return System.Windows.Application.GetResourceStream(uri);
            }
        }

        private static Uri GetClutureResoureUri(string path, string culture)
        {
            var culturePath = Path.Combine("/Resources", culture, path);
            return new Uri(culturePath, UriKind.Relative);
        }
    }
}


