using System;
using System.IO;
using System.Windows;


namespace NeeView
{
    public static class ResourceTools
    {
        public static Stream OpenResource(string contentPath)
        {
            Uri uri = new Uri(contentPath, UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            if (info is null) throw new FileNotFoundException($"No such resource: {contentPath}");
            return info.Stream;
        }

        public static void ExportFileFromResource(string path, string contentPath)
        {
            using (var output = File.Create(path))
            using (var input = OpenResource(contentPath))
            {
                input.CopyTo(output);
            }
        }
    }
}
