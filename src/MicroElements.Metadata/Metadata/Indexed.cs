using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Collections.Caching;

namespace MicroElements.Metadata;

public interface IIndexedPropertyContainer
{
    IPropertyValue? GetPropertyValue(IProperty property);

    int GetPropertyIndex(IProperty property);
    //IPropertyValue? GetPropertyValue(int index);
}

public class PropertyIndex
{
    private Dictionary<IProperty, int> _propertyIndex;

    public IReadOnlyList<IProperty> Properties { get; }

    public IEqualityComparer<IProperty> PropertyComparer { get; }

    public IPropertyValueFactory PropertyValueFactory { get; }


    public PropertyIndex(IPropertySet propertySet, IEqualityComparer<IProperty> propertyComparer)
    {
        if (propertySet.GetProperties() is IReadOnlyList<IProperty> properties)
        {
            Properties = properties;
        }
        else
        {
            Properties = propertySet.GetProperties().ToArray();
        }

        PropertyComparer = propertyComparer;

        _propertyIndex = new Dictionary<IProperty, int>(PropertyComparer);

        for (int i = 0; i < Properties.Count; i++)
        {
            IProperty property = Properties[i];
            _propertyIndex[property] = i;
        }
    }

    public int GetPropertyIndex(IProperty property)
    {
        return _propertyIndex.GetValueOrDefault(property, -1);
    }
}

public class IndexedPropertyContainer2 : IPropertyContainer, IIndexedPropertyContainer
{
    private readonly PropertyIndex _propertyIndex;

    private readonly object[] _values;
    private readonly ValueSource[] _valueSource;

    public IndexedPropertyContainer2(IPropertyContainer propertyContainer, PropertyIndex propertyIndex)
    {
        _propertyIndex = propertyIndex;

        int propertiesCount = _propertyIndex.Properties.Count;
        _values = new object[propertiesCount];
        _valueSource = new ValueSource[propertiesCount];
    }

    /// <inheritdoc />
    public IPropertyValue? GetPropertyValue(IProperty property)
    {
        int propertyIndex = _propertyIndex.GetPropertyIndex(property);

        if (propertyIndex >= 0)
        {
            return PropertyValue.Create(_propertyIndex.Properties[propertyIndex], _values[propertyIndex], _valueSource[propertyIndex]);
        }

        return null;
    }

    /// <inheritdoc />
    public int GetPropertyIndex(IProperty property)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public IEnumerator<IPropertyValue> GetEnumerator()
    {
        for (var i = 0; i < _propertyIndex.Properties.Count; i++)
        {
            yield return PropertyValue.Create(_propertyIndex.Properties[i], _values[i], _valueSource[i]);
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IPropertyValue> Properties => this.ToArray();

    /// <inheritdoc />
    public int Count => _values.Length;

    /// <inheritdoc />
    public IPropertyContainer? ParentSource { get; }

    /// <inheritdoc />
    public SearchOptions SearchOptions { get; }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class IndexedPropertyContainer : IPropertyContainer, IIndexedPropertyContainer
{
    public static SearchOptions DefaultSearchOptions = SearchOptions.ExistingOnly.WithPropertyComparer(ByTypeAndNamePropertyComparer.Strict);
    private readonly IPropertyContainer _propertyContainer;
    private readonly Dictionary<IProperty, IPropertyValue> _propertyValuesDictionary;
    private readonly SearchOptions _searchOptions;

    public IndexedPropertyContainer(IPropertyContainer propertyContainer, SearchOptions? searchOptions = null)
    {
        _propertyContainer = propertyContainer;
        _searchOptions = searchOptions ?? propertyContainer.SearchOptions;
        _propertyValuesDictionary = MakeIndex(propertyContainer);
    }

    private Dictionary<IProperty, IPropertyValue> MakeIndex(IPropertyContainer propertyContainer)
    {
        var propertyValues = new Dictionary<IProperty, IPropertyValue>(_searchOptions.PropertyComparer);

        foreach (IPropertyValue propertyValue in propertyContainer)
        {
            propertyValues[propertyValue.PropertyUntyped] = propertyValue;
        }

        return propertyValues;
    }

    public IPropertyValue? GetPropertyValue(IProperty property)
    {
        return _propertyValuesDictionary.GetValueOrDefault(property);
    }

    /// <inheritdoc />
    public int GetPropertyIndex(IProperty property)
    {
        throw new System.NotImplementedException();
    }

    public SearchOptions SearchOptions => _searchOptions;

    public IPropertyContainer? ParentSource => _propertyContainer.ParentSource;

    public IReadOnlyCollection<IPropertyValue> Properties => _propertyContainer.Properties;

    public int Count => _propertyContainer.Count;

    public IPropertyContainer GetMetadataContainer(bool autoCreate = false)
        => _propertyContainer.GetMetadataContainer(autoCreate);

    public void SetMetadataContainer(IPropertyContainer metadata)
        => _propertyContainer.SetMetadataContainer(metadata);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<IPropertyValue> GetEnumerator() => _propertyContainer.GetEnumerator();
}

public sealed class IndexedSearch : ISearchAlgorithm
{
    private ISearchAlgorithm _searchAlgorithm;
    private ConcurrentDictionary<IEqualityComparer<IProperty>, ConcurrentDictionary<IProperty, int>> _index = new ();

    public IndexedSearch(ISearchAlgorithm searchAlgorithm)
    {
        _searchAlgorithm = searchAlgorithm;
    }

    public IPropertyValue? SearchPropertyValueUntyped(IPropertyContainer propertyContainer, IProperty property, SearchOptions? searchOptions = null)
    {
        if (propertyContainer is IIndexedPropertyContainer indexed)
        {
            return indexed.GetPropertyValue(property);
        }

        return _searchAlgorithm.SearchPropertyValueUntyped(propertyContainer, property, searchOptions);
    }

    public IPropertyValue<T>? GetPropertyValue<T>(IPropertyContainer propertyContainer, IProperty<T> property, SearchOptions? searchOptions = null)
    {
        if (propertyContainer is IIndexedPropertyContainer indexed)
        {
            return (IPropertyValue<T>)indexed.GetPropertyValue(property);
        }

        return _searchAlgorithm.GetPropertyValue(propertyContainer, property, searchOptions);
    }

    /// <inheritdoc />
    public void GetPropertyValue2<T>(IPropertyContainer propertyContainer, IProperty<T> property, SearchOptions? searchOptions,
        out PropertyValueData<T> result)
    {
        throw new System.NotImplementedException();
    }
}

public static class IndexedExtensions
{
    public static IPropertyContainer? Indexed(this IPropertyContainer? propertyContainer, SearchOptions? searchOptions)
    {
        if (propertyContainer == null)
            return null;
        return new IndexedPropertyContainer(propertyContainer, searchOptions ?? IndexedPropertyContainer.DefaultSearchOptions);
    }
}

public class Indexed : IPropertyContainer
{
    private readonly IPropertyContainer _propertyContainer;

    public Indexed(IPropertyContainer propertyContainer, IPropertySet schema, IEqualityComparer<IProperty> equalityComparer)
    {
        _propertyContainer = propertyContainer;
        IPropertyValue?[] indexed = Index(_propertyContainer, schema, equalityComparer);
    }

    /// <inheritdoc />
    public IEnumerator<IPropertyValue> GetEnumerator() => _propertyContainer.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public int Count => _propertyContainer.Count;

    /// <inheritdoc />
    public IPropertyContainer GetMetadataContainer(bool autoCreate = false)
    {
        return _propertyContainer.GetMetadataContainer(autoCreate);
    }

    /// <inheritdoc />
    public void SetMetadataContainer(IPropertyContainer metadata)
    {
        _propertyContainer.SetMetadataContainer(metadata);
    }

    /// <inheritdoc />
    public IPropertyContainer? ParentSource => _propertyContainer.ParentSource;

    /// <inheritdoc />
    public IReadOnlyCollection<IPropertyValue> Properties => _propertyContainer.Properties;

    /// <inheritdoc />
    public SearchOptions SearchOptions => _propertyContainer.SearchOptions;

    public static IPropertyValue?[] Index(IPropertyContainer propertyContainer, IPropertySet schema, IEqualityComparer<IProperty> propertyComparer)
    {
        IProperty[] properties = schema.GetCached(set => set.GetProperties().ToArray(), timeToLive: TimeSpan.FromMinutes(1));

        IPropertyValue?[] indexed = new IPropertyValue[properties.Length];
        Lazy<List<IPropertyValue>> not_indexed = new(() => new List<IPropertyValue>(properties.Length));

        foreach (var propertyValue in propertyContainer.Properties)
        {
            bool isInSchema = false;
            for (int i = 0; i < properties.Length; i++)
            {
                if (indexed[i] != null)
                    continue;

                IProperty property = properties[i];
                if (propertyComparer.Equals(propertyValue.PropertyUntyped, property))
                {
                    indexed[i] = propertyValue;
                    isInSchema = true;
                    break;
                }
            }

            if (!isInSchema)
            {
                not_indexed.Value.Add(propertyValue);
            }
        }

        return indexed;
    }


}
