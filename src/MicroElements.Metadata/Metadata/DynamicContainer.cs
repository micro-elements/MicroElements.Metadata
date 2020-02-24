using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Dynamic wrapper for <see cref="IPropertyContainer"/>.
    /// </summary>
    public class DynamicContainer : DynamicObject
    {
        private readonly IPropertyContainer _propertyContainer;
        private readonly bool _ignoreCase;
        private readonly bool _searchInParent;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicContainer"/> class.
        /// </summary>
        /// <param name="propertyContainer">Property container to wrap.</param>
        /// <param name="ignoreCase">Use ignore case comparison.</param>
        /// <param name="searchInParent">Search property in parent if not found in current.</param>
        public DynamicContainer(IPropertyContainer propertyContainer, bool ignoreCase = true, bool searchInParent = true)
        {
            _propertyContainer = propertyContainer;
            _ignoreCase = ignoreCase;
            _searchInParent = searchInParent;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _propertyContainer.Properties.SelectMany(propertyValue => propertyValue.PropertyUntyped.GetNameAndAliases());
        }

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var propertyValue = _propertyContainer.GetPropertyValueUntyped(Search.ByNameOrAlias(binder.Name, _ignoreCase).SearchInParent(_searchInParent));
            if (propertyValue != null)
            {
                result = propertyValue.ValueUntyped;
                return true;
            }

            // returns null if property was not found.
            result = null;
            return true;
        }
    }

    public static partial class PropertyContainerExtensions
    {
        /// <summary>
        /// Creates  dynamic wrapper for <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="ignoreCase">Use ignore case comparison.</param>
        /// <param name="searchInParent">Search property in parent if not found in current.</param>
        /// <returns>Dynamic object.</returns>
        public static dynamic AsDynamic(this IPropertyContainer propertyContainer, bool ignoreCase = true, bool searchInParent = true) =>
            new DynamicContainer(propertyContainer, ignoreCase, searchInParent);
    }
}
