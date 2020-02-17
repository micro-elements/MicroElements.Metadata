using System.Dynamic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Dynamic wrapper for <see cref="IPropertyContainer"/>.
    /// </summary>
    public class DynamicContainer : DynamicObject
    {
        private readonly IPropertyContainer _propertyContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicContainer"/> class.
        /// </summary>
        /// <param name="propertyContainer">Property container to wrap.</param>
        public DynamicContainer(IPropertyContainer propertyContainer)
        {
            _propertyContainer = propertyContainer;
        }

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            var propertyValue = _propertyContainer.GetPropertyValueUntyped(Search.ByNameOrAlias(binder.Name).SearchInParent());
            if (propertyValue != null)
            {
                result = propertyValue.ValueUntyped;
                return true;
            }

            return false;
        }
    }

    public static partial class PropertyContainerExtensions
    {
        /// <summary>
        /// Creates  dynamic wrapper for <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>Dynamic object.</returns>
        public static dynamic AsDynamic(this IPropertyContainer propertyContainer) =>
            new DynamicContainer(propertyContainer);
    }
}
