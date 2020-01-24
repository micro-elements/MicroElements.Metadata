using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicroElements.Functional;

namespace MicroElements.Utils
{
    /// <summary>
    /// TODO: Move methods to MicroElements.Functional.
    /// </summary>
    internal static class Functional
    {
        /// <summary>
        /// Returns the first element of the sequence that satisfies a condition or None if no such element is found.
        /// It's like FirstOrDefault but returns Option.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>First element that satisfies <paramref name="predicate"/> or None.</returns>
        public static Option<T> FirstOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
            source.FirstOrDefault(predicate);
    }
}
