using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaDVH.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<TValue>> ChunkIn<TValue>(
                this IEnumerable<TValue> values,
                int chunkSize)
        {
            using (var enumerator = values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return GetChunk(enumerator, chunkSize).ToList();
                }
            }
        }

        private static IEnumerable<T> GetChunk<T>(
                 IEnumerator<T> enumerator,
                 int chunkSize)
        {
            do
            {
                yield return enumerator.Current;
            } while (--chunkSize > 0 && enumerator.MoveNext());
        }
    }
}
