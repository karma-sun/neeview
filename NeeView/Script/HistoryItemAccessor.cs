using System;

namespace NeeView
{
    public class HistoryItemAccessor
    {
        private BookHistory _source;

        public HistoryItemAccessor(BookHistory source)
        {
            _source = source;
        }

        internal BookHistory Source => _source;

        public string Name => _source.Name;

        public string Path => _source.Path;

        public string LastAccessTime => _source.LastAccessTime.ToString();
    }
}
