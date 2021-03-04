using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public static class BitmapMetadataExtensions
    {
        public static object GetQuery(this BitmapMetadata meta, params string[] queries)
        {
            return meta.GetQueryFirst(queries);
        }

        public static object GetQueryFirst(this BitmapMetadata meta, IEnumerable<string> queries)
        {
            foreach (var query in queries)
            {
                var value = meta.GetQuery(query);
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }
    }

}
