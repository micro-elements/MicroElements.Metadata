namespace MicroElements.Metadata;

public static partial class MetadataProviderExtensions
{
    /// <summary>
    /// Copies metadata from source object to target object.
    /// Source and target can be <see cref="IMetadataProvider"/> or any other reference type.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="target">Target object.</param>
    public static void CopyMetadataTo(this object? source, object? target)
    {
        if (source == null || target == null || ReferenceEquals(source, target)) return;

        var from = source.AsMetadataProvider();
        IPropertyContainer sourceValues = from.GetMetadataContainer(autoCreate: false);
        if (sourceValues.Count > 0)
        {
            var to = target.AsMetadataProvider();

            IMutablePropertyContainer targetMetadata = to.GetMetadataContainer(autoCreate: false).ToMutable();
            targetMetadata.SetValues(sourceValues.Properties);

            to.SetMetadataContainer(targetMetadata);
        }
    }

    /// <summary>
    /// Copies metadata from source object to target object.
    /// Source and target can be <see cref="IMetadataProvider"/> or any other reference type.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="target">Target object.</param>
    public static void CopyMetadataTo(this IMetadataProvider? source, IMetadataProvider? target, bool makeReadOnly = true)
    {
        if (source == null || target == null || ReferenceEquals(source, target))
            return;

        IPropertyContainer sourceValues = source.GetMetadataContainer(autoCreate: false);
        if (sourceValues.Count == 0)
            return;

        IPropertyContainer targetContainer = target.GetMetadataContainer(autoCreate: false);

        if (targetContainer.Count == 0)
        {
            if (sourceValues is IMutablePropertyContainer mutableSourceValues)
            {
                // Create protective readonly copy
                targetContainer = mutableSourceValues.CloneAsReadOnly();
            }
            else
            {
                // Target is Empty and ReadOnly so just replace it with readonly source
                targetContainer = sourceValues;
            }
        }
        else
        {
            // Add values to not empty target
            targetContainer = targetContainer.ToMutable().WithValues(sourceValues);
            //new HierarchicalContainer()
        }

        target.SetMetadataContainer(targetContainer);
    }

    /// <summary>
    /// Sets metadata from <paramref name="metadataSource"/>.
    /// This is inverted version of <see cref="CopyMetadataTo"/>.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    /// <param name="target">Metadata target.</param>
    /// <param name="metadataSource">Metadata source.</param>
    /// <returns>The same instance.</returns>
    public static T SetMetadataFrom<T>(this T target, object metadataSource)
    {
        metadataSource.CopyMetadataTo(target);
        return target;
    }
}
