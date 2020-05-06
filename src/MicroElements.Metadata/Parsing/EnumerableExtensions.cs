using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Parsing
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// В отладке показыает перечисление как Array<T>, в релизной сборке IEnumerable<T>.
        /// Используется для более простой отладки, но чтобы не влияло на Performance в релизе.
        /// </summary>
        public static IEnumerable<T> ToArrayDebug<T>(this IEnumerable<T> source)
        {
#if DEBUG
            return source.ToArray();
#else
            return source;
#endif
        }

        public static IEnumerable<T> MaterializeDebug<T>(this IEnumerable<T> source, Action<IReadOnlyList<T>> action)
        {
#if DEBUG
            var materializedItems = source.ToArray();
            action(materializedItems);
            return materializedItems;
#else
            return source;
#endif
        }

        public static IEnumerable<T> Materialize<T>(this IEnumerable<T> source, Action<IReadOnlyList<T>> action)
        {
            var materializedItems = source.ToArray();
            action(materializedItems);
            return materializedItems;
        }
    }
}
