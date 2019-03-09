using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// 生成したArchiver を弱参照で保持しておく機構
    /// </summary>
    public class ArchiverCache
    {
        private Dictionary<string, WeakReference<Archiver>> _caches = new Dictionary<string, WeakReference<Archiver>>();

        public void Add(Archiver archiver)
        {
            Debug.WriteLine($"ArchvierCache: Add {archiver.SystemPath}");
            _caches[archiver.SystemPath] = new WeakReference<Archiver>(archiver);
        }

        public bool TryGetValue(string path, out Archiver archiver)
        {
            if (_caches.Count > 50)
            {
                Debug.WriteLine($"ArchvierCache: CleanUp ...");
                CleanUp();
                Dump();
            }

            if (_caches.TryGetValue(path, out var weakReference))
            {
                if (weakReference.TryGetTarget(out archiver))
                {
                    return true;
                }
            }

            archiver = null;
            return false;
        }

        public void CleanUp()
        {
            var removes = _caches.Where(e => !e.Value.TryGetTarget(out var archiver)).Select(e => e.Key).ToList();
            foreach (var key in removes)
            {
                Debug.WriteLine($"ArchvierCache: Remove {key}");
                _caches.Remove(key);
            }
        }

        /// <summary>
        /// すべてのアーカイブファイルロック解除
        /// </summary>
        public void Unlock()
        {
            CleanUp();
            foreach (var item in _caches)
            {
                if (item.Value.TryGetTarget(out var archiver))
                {
                    archiver.Unlock();
                }
            }
        }

        [Conditional("DEBUG")]
        public void Dump()
        {
            int count = 0;
            foreach (var item in _caches)
            {
                if (item.Value.TryGetTarget(out var archiver))
                {
                    Debug.WriteLine($"ArchiveCache[{count}]: {archiver.SystemPath} => {archiver.TempFile?.Path}");
                }
                else
                {
                    Debug.WriteLine($"ArchiveCache[{count}]: removed.");
                }
                count++;
            }
        }

    }

}

