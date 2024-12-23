﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Schema;
using MicroElements.Reflection.ObjectExtensions;

namespace MicroElements.Metadata;

/// <summary>
/// Provides extension methods for metadata providers.
/// </summary>
public static partial class MetadataProviderExtensions
{
    /// <summary>
    /// Returns a value indicating whether the metadata provider has any metadata.
    /// </summary>
    /// <param name="metadataProvider">Source metadata provider.</param>
    /// <returns>A value indicating whether the metadata provider has any metadata.</returns>
    public static bool HasMetadata(this IMetadataProvider? metadataProvider)
    {
        if (metadataProvider == null)
            return false;

        return metadataProvider.GetMetadataContainer(autoCreate: false).Count > 0;
    }

    /// <summary>
    /// Gets metadata of required type.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Metadata provider.</param>
    /// <param name="metadataName">Optional metadata name.</param>
    /// <param name="defaultValue">Default value to return if not metadata found.</param>
    /// <param name="searchInSchema">Search in schema if metadata not found in current.</param>
    /// <returns>Metadata or default value if not found.</returns>
    public static TMetadata? GetMetadata<TMetadata>(
        this IMetadataProvider metadataProvider,
        string? metadataName = null,
        TMetadata? defaultValue = default,
        bool searchInSchema = false)
    {
        if (metadataProvider == null)
            throw new ArgumentNullException(nameof(metadataProvider));

        var metadataContainer = metadataProvider.GetMetadataContainer(autoCreate: false);
        if (metadataContainer.Count > 0)
        {
            string metadataNameToSearch = metadataName ?? typeof(TMetadata).FullName;
            SearchOptions metadataSearchOptions = Search.ExistingOnly
                .UseSearchByNameAndComparer<TMetadata>(metadataNameToSearch, MetadataProvider.DefaultMetadataComparer);

            if (searchInSchema && metadataContainer.GetMetadataFromContainer<IHasSchema>() is { Schema: { } schema })
            {
                metadataContainer = new PropertyContainerWithParent(metadataContainer, schema.GetMetadataContainer());
                metadataSearchOptions = metadataSearchOptions.SearchInParent(true);
            }

            var propertyValue = metadataContainer.GetPropertyValue<TMetadata>(metadataSearchOptions);
            if (propertyValue.HasValue() && propertyValue.Value.IsNotNull())
                return propertyValue.Value;
        }

        return defaultValue;
    }

    /// <summary>
    /// The same as <see cref="GetMetadata{TMetadata}"/> but also searches metadata in schema (if bound).
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Metadata provider.</param>
    /// <param name="metadataName">Optional metadata name.</param>
    /// <param name="defaultValue">Default value to return if not metadata found.</param>
    /// <returns>Metadata or default value if not found.</returns>
    public static TMetadata? GetSchemaMetadata<TMetadata>(
        this IMetadataProvider metadataProvider,
        string? metadataName = null,
        TMetadata? defaultValue = default)
    {
        return metadataProvider.GetMetadata(metadataName, defaultValue, searchInSchema: true);
    }

    /// <summary>
    /// Gets metadata of required type. The same as <see cref="GetMetadata{TMetadata}"/> but searches metadata in provided container.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataContainer">Metadata container.</param>
    /// <param name="metadataName">Optional metadata name.</param>
    /// <param name="defaultValue">Default value to return if not metadata found.</param>
    /// <returns>Metadata or default value if not found.</returns>
    public static TMetadata? GetMetadataFromContainer<TMetadata>(
        this IPropertyContainer metadataContainer,
        string? metadataName = null,
        TMetadata? defaultValue = default)
    {
        if (metadataContainer.Count > 0)
        {
            string metadataNameToSearch = metadataName ?? typeof(TMetadata).FullName;
            SearchOptions metadataSearchOptions = Search.ExistingOnly
                .UseSearchByNameAndComparer<TMetadata>(metadataNameToSearch, MetadataProvider.DefaultMetadataComparer);

            var propertyValue = metadataContainer.GetPropertyValue<TMetadata>(metadataSearchOptions);
            if (propertyValue.HasValue() && propertyValue.Value.IsNotNull())
                return propertyValue.Value;
        }

        return defaultValue;
    }

    /// <summary>
    /// Sets metadata for item and returns the same metadataProvider for chaining.
    /// </summary>
    /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Target metadata provider.</param>
    /// <param name="metadata">Metadata to set.</param>
    /// <param name="valueSource">Optional value source for metadata.</param>
    /// <returns>The same metadataProvider.</returns>
    public static TMetadataProvider SetMetadata<TMetadataProvider, TMetadata>(
        this TMetadataProvider metadataProvider,
        TMetadata? metadata,
        ValueSource? valueSource = null)
        where TMetadataProvider : IMetadataProvider
    {
        metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));

        return metadataProvider.SetMetadata(typeof(TMetadata).FullName, metadata, valueSource);
    }

    public static TMetadataProvider ConfigureMetadataProvider<TMetadataProvider>(
        this TMetadataProvider metadataProvider,
        bool keepItReadOnly = true,
        bool isMutationAllowed = true)
        where TMetadataProvider : IMetadataProvider
    {
        metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));

        return metadataProvider.SetMetadata("keepItReadOnly");
    }

    /// <summary>
    /// Sets metadata for target object and returns the same metadataProvider for chaining.
    /// </summary>
    /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Target metadata provider.</param>
    /// <param name="metadataName">Metadata name.</param>
    /// <param name="metadata">Metadata to set.</param>
    /// <param name="valueSource">Optional value source for metadata.</param>
    /// <param name="isMutationAllowed"></param>
    /// <returns>The same metadataProvider.</returns>
    public static TMetadataProvider SetMetadata<TMetadataProvider, TMetadata>(
        this TMetadataProvider metadataProvider,
        string? metadataName,
        TMetadata? metadata,
        ValueSource? valueSource = null,
        bool isMutationAllowed = true)
        where TMetadataProvider : IMetadataProvider
    {
        metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));

        metadataName ??= typeof(TMetadata).FullName;
        IProperty<TMetadata> metadataProperty = Search.CachedProperty<TMetadata>.ByName(metadataName);

        var metadataContainer = metadataProvider.GetMetadataContainer(autoCreate: true);

        if (metadataContainer is IMutablePropertyContainer mutablePropertyContainer)
        {
            mutablePropertyContainer.SetValue(metadataProperty, metadata, valueSource);
        }
        else
        {
            if (isMutationAllowed)
            {
                var mutable = metadataContainer.ToMutable();
                mutable.SetValue(metadataProperty, metadata, valueSource);
                metadataProvider.SetMetadataContainer(mutable);
            }
            else
            {
                throw new InvalidOperationException("Mutation is not allowed");
            }
        }

        return metadataProvider;
    }

    /// <summary>
    /// Converts configure action to configure function.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="configureAction">Source configure action.</param>
    /// <returns>Result configure function.</returns>
    public static Func<T, T> ToConfigureFunc<T>(this Action<T> configureAction)
    {
        return arg =>
        {
            configureAction(arg);
            return arg;
        };
    }

    /// <summary>
    /// Configures metadata with action. Can be called many times.
    /// If metadata is not exists then it creates with default constructor.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Target metadata provider.</param>
    /// <param name="configureMetadata">Configure action.</param>
    /// <param name="metadataName">Optional metadata name.</param>
    /// <returns>The same metadataProvider.</returns>
    public static IMetadataProvider ConfigureMetadata<TMetadata>(
        this IMetadataProvider metadataProvider,
        Action<TMetadata> configureMetadata,
        string? metadataName = null)
        where TMetadata : new()
    {
        return metadataProvider.ConfigureMetadata<IMetadataProvider, TMetadata>(
            createMetadata: provider => new TMetadata(),
            configureMetadata: configureMetadata.ToConfigureFunc(),
            metadataName: metadataName);
    }

    /// <summary>
    /// Configures metadata with action. Can be called many times.
    /// If metadata is not exists then it creates with default constructor.
    /// </summary>
    /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Target metadata provider.</param>
    /// <param name="configureMetadata">Configure action.</param>
    /// <param name="metadataName">Optional metadata name.</param>
    /// <returns>The same metadataProvider.</returns>
    public static TMetadataProvider ConfigureMetadata<TMetadataProvider, TMetadata>(
        this TMetadataProvider metadataProvider,
        Action<TMetadata> configureMetadata,
        string? metadataName = null)
        where TMetadataProvider : IMetadataProvider
        where TMetadata : new()
    {
        return metadataProvider.ConfigureMetadata<TMetadataProvider, TMetadata>(
            createMetadata: provider => new TMetadata(),
            configureMetadata: configureMetadata.ToConfigureFunc(),
            metadataName: metadataName);
    }

    /// <summary>
    /// Configures metadata with action. Can be called many times.
    /// If metadata is not exists then it creates with default constructor.
    /// </summary>
    /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
    /// <typeparam name="TMetadataInterface">Metadata interface type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Target metadata provider.</param>
    /// <param name="configureMetadata">Configure action.</param>
    /// <param name="metadataName">Optional metadata name.</param>
    /// <returns>The same metadataProvider.</returns>
    public static TMetadataProvider ConfigureMetadata<TMetadataProvider, TMetadataInterface, TMetadata>(
        this TMetadataProvider metadataProvider,
        Action<TMetadata> configureMetadata,
        string? metadataName = null)
        where TMetadataProvider : IMetadataProvider
        where TMetadata : TMetadataInterface, new()
    {
        return metadataProvider.ConfigureMetadata<TMetadataProvider, TMetadataInterface>(
            createMetadata: provider => new TMetadata(),
            configureMetadata: metadata =>
            {
                configureMetadata((TMetadata)metadata);
                return metadata;
            },
            metadataName: metadataName);
    }

    /// <summary>
    /// Configures metadata with action. Can be called many times.
    /// If metadata is not exists then it creates with default constructor.
    /// </summary>
    /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Target metadata provider.</param>
    /// <param name="createMetadata">Metadata factory function.</param>
    /// <param name="configureMetadata">Metadata configure action.</param>
    /// <param name="metadataName">Optional metadata name.</param>
    /// <returns>The same metadataProvider.</returns>
    public static TMetadataProvider ConfigureMetadata<TMetadataProvider, TMetadata>(
        this TMetadataProvider metadataProvider,
        Func<TMetadataProvider, TMetadata> createMetadata,
        Func<TMetadata, TMetadata> configureMetadata,
        string? metadataName = null)
        where TMetadataProvider : IMetadataProvider
    {
        metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));
        createMetadata.AssertArgumentNotNull(nameof(createMetadata));
        configureMetadata.AssertArgumentNotNull(nameof(configureMetadata));

        lock (metadataProvider)
        {
            bool isJustCreated = false;

            TMetadata metadata = metadataProvider.GetMetadata<TMetadata>(metadataName);

            if (metadata.IsNull())
            {
                metadata = createMetadata(metadataProvider);
                isJustCreated = true;
            }

            TMetadata metadataNew = configureMetadata(metadata);

            if (isJustCreated || !ReferenceEquals(metadataNew, metadata))
                metadataProvider.SetMetadata(metadataName, metadataNew);
        }

        return metadataProvider;
    }

    /// <summary>
    /// Returns provider metadata as <see cref="IPropertyContainer"/>.
    /// </summary>
    /// <param name="metadataProvider">Source metadata provider.</param>
    /// <returns>Metadata.</returns>
    public static IPropertyContainer GetMetadataContainer(this IMetadataProvider metadataProvider)
    {
        if (metadataProvider == null)
            throw new ArgumentNullException(nameof(metadataProvider));

        return metadataProvider.GetMetadataContainer(autoCreate: false);
    }

    /// <summary>
    /// Returns provider metadata as <see cref="IMutablePropertyContainer"/>.
    /// If current metadata is not mutable then creates a mutable copy and sets to the source metadataProvider.
    /// </summary>
    /// <param name="metadataProvider">Source metadata provider.</param>
    /// <returns>Mutable metadata.</returns>
    public static IMutablePropertyContainer AsMutable(this IMetadataProvider metadataProvider)
    {
        if (metadataProvider == null)
            throw new ArgumentNullException(nameof(metadataProvider));

        var metadata = metadataProvider.GetMetadataContainer(autoCreate: false);

        // Already mutable
        if (metadata is IMutablePropertyContainer mutable)
            return mutable;

        // Creates mutable copy and sets to provider
        var mutableFromReadOnly = metadata.ToMutable();
        metadataProvider.SetMetadataContainer(mutableFromReadOnly);

        return mutableFromReadOnly;
    }

    /// <summary>
    /// Replaces <see cref="IMutablePropertyContainer"/> with read only version.
    /// </summary>
    /// <param name="metadataProvider">Source metadata provider.</param>
    /// <returns>Current instance metadata.</returns>
    public static IPropertyContainer FreezeMetadata(this IMetadataProvider metadataProvider)
    {
        if (metadataProvider == null)
            throw new ArgumentNullException(nameof(metadataProvider));

        IPropertyContainer metadataContainer = metadataProvider.GetMetadataContainer(autoCreate: false);
        if (metadataContainer is IMutablePropertyContainer mutable)
        {
            var readOnlyMetadata = mutable.ToReadOnly(flattenHierarchy: true);
            metadataProvider.SetMetadataContainer(readOnlyMetadata);
            return readOnlyMetadata;
        }

        return metadataContainer;
    }
}
