using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public class GifMetadataAccessor : BitmapMetadataAccessor
    {
        private BitmapMetadata _meta;
        private List<string> _comments = new List<string>();

        public GifMetadataAccessor(BitmapMetadata meta)
        {
            _meta = meta ?? throw new ArgumentNullException(nameof(meta));
            Debug.Assert(_meta.Format == "gif");

            // commentext  map
            foreach (var key in meta.Where(e => e.EndsWith("commentext", StringComparison.Ordinal)))
            {
                var commentextMeta = meta.GetQuery(key) as BitmapMetadata;
                if (commentextMeta != null)
                {
                    var text = commentextMeta.GetQuery("/TextEntry") as string;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        _comments.Add(text);
                    }
                }
            }
        }

        public override object GetValue(BitmapMetadataKey key)
        {
            switch (key)
            {
                case BitmapMetadataKey.Comments: return string.Join(Environment.NewLine, _comments);

                default: return null;
            }
        }
    }

}
