using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public class BitmapMetadataDatabase : IReadOnlyDictionary<BitmapMetadataKey, object>
    {
        private static readonly Dictionary<BitmapMetadataKey, object> _emptyMap = Enum.GetValues(typeof(BitmapMetadataKey)).Cast<BitmapMetadataKey>().ToDictionary(e => e, e => (object)null);

        private Dictionary<BitmapMetadataKey, object> _map;


        public BitmapMetadataDatabase(BitmapMetadata meta)
        {
            var accessor = BitmapMetadataAccessorFactory.Create(meta);
            _map = accessor != null ? CreateMap(accessor) : _emptyMap;
        }


        private Dictionary<BitmapMetadataKey, object> CreateMap(BitmapMetadataAccessor accessor)
        {
            var map = new Dictionary<BitmapMetadataKey, object>();

            foreach (BitmapMetadataKey key in Enum.GetValues(typeof(BitmapMetadataKey)))
            {
                try
                {
                    map[key] = accessor.GetValue(key);
                }
                catch (Exception ex)
                {
#if DEBUG
                    map[key] = $"Exception: {ex.Message}";
#else
                    _map[key] = null;
#endif
                }
            }

            return map;
        }


        public object ElementAt(BitmapMetadataKey key) => _map[key];

        #region IReadOnlyDictionary

        public object this[BitmapMetadataKey key] => ((IReadOnlyDictionary<BitmapMetadataKey, object>)_map)[key];

        public IEnumerable<BitmapMetadataKey> Keys => ((IReadOnlyDictionary<BitmapMetadataKey, object>)_map).Keys;

        public IEnumerable<object> Values => ((IReadOnlyDictionary<BitmapMetadataKey, object>)_map).Values;

        public int Count => ((IReadOnlyCollection<KeyValuePair<BitmapMetadataKey, object>>)_map).Count;

        public bool ContainsKey(BitmapMetadataKey key)
        {
            return ((IReadOnlyDictionary<BitmapMetadataKey, object>)_map).ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<BitmapMetadataKey, object>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<BitmapMetadataKey, object>>)_map).GetEnumerator();
        }

        public bool TryGetValue(BitmapMetadataKey key, out object value)
        {
            return ((IReadOnlyDictionary<BitmapMetadataKey, object>)_map).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_map).GetEnumerator();
        }

        #endregion IReadOnlyDictionary
    }

}
