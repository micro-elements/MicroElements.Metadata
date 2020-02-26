using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata
{
    public static class FunctionalExtensions
    {
        /// <summary>
        /// Разложение списка списков в плоский список.
        /// </summary>
        /// <param name="listOfPropertyLists">Список списков.</param>
        /// <returns>Плоский список.</returns>
        public static IEnumerable<IProperty> Flatten(this IEnumerable<IEnumerable<IProperty>> listOfPropertyLists) =>
            listOfPropertyLists.SelectMany(propertyList => propertyList);
    }
}
