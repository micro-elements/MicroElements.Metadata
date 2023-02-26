using System;
using MicroElements.Metadata.Schema;
using MicroElements.Reflection.ObjectExtensions;

namespace MicroElements.Metadata;

public static partial class PropertyExtensions
{
    public static IProperty<TResult?> MapNew<TSource, TResult>(
        this IProperty<TSource> property,
        Func<TSource?, TResult?> map,
        bool allowMapNull = false,
        bool allowMapUndefined = false,
        Func<SearchOptions, SearchOptions>? configureSearch = null)
    {
        var state = new MapState<TSource, TResult>(property, map, allowMapNull, allowMapUndefined, configureSearch);
        var propertyCalculator = new PropertyCalculator<TResult?, MapState<TSource, TResult>>(
            calculate: static (ref CalculationContext context, MapState<TSource, TResult> mapState)
                => MapInternal(ref context, mapState), state);

        return new Property<TResult?>(property.Name)
            .With(description: property.Description, alias: property.Alias)
            .WithPropertyCalculator(propertyCalculator);
    }

    private sealed record MapState<TSource, TResult>(
        IProperty<TSource> property,
        Func<TSource?, TResult?> map,
        bool allowMapNull = false,
        bool allowMapUndefined = false,
        Func<SearchOptions, SearchOptions>? configureSearch = null);

    private static TResult? MapInternal<TSource, TResult>(ref CalculationContext context, MapState<TSource, TResult> state)
    {
        var search = state.configureSearch?.Invoke(context.SearchOptions) ?? context.SearchOptions;
        //IPropertyValue<TSource>? sourcePropertyValue = context.PropertyContainer.GetPropertyValue(state.property, search);
        DefaultSearchAlgorithm.Instance.GetPropertyValue2(context.PropertyContainer, state.property, search, out var sourcePropertyValue);

        if (sourcePropertyValue.Source != ValueSource.NotDefined)
        {
            TSource? sourceValue = sourcePropertyValue.Value;
            if (!sourceValue.IsNull() || state.allowMapNull)
            {
                TResult? resultValue1 = state.map(sourceValue);
                context.ValueSource = ValueSource.Calculated;
                return resultValue1;
            }
        }
        else
        {
            if (state.allowMapUndefined)
            {
                TResult? resultValue2 = state.map(default);
                context.ValueSource = ValueSource.Calculated;
                return resultValue2;
            }
        }

        context.ValueSource = ValueSource.NotDefined;
        return default;
    }
}
