using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ThumbnaulPool for Panel
    /// </summary>
    public class BookThumbnailPool : ThumbnailPool
    {
        public static BookThumbnailPool _current;
        public static BookThumbnailPool Current
        {
            get
            {
                _current = _current ?? new BookThumbnailPool();
                return _current;
            }
        }

        public override int Limit => Config.Current.Thumbnail.ThumbnailBookCapacity;
    }
}
