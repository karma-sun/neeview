using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Collections.Generic
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// ForEach
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var e in self)
            {
                action(e);
            }
        }

        /// <summary>
        /// シーケンスを指定されたサイズのチャンクに分割します.
        /// http://devlights.hatenablog.com/entry/20121130/p2
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> self, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("Chunk size must be greater than 0.", "chunkSize");
            }

            while (self.Any())
            {
                yield return self.Take(chunkSize);
                self = self.Skip(chunkSize);
            }
        }
    }
}
