# MicroElements.Metadata
Provides metadata model, parsing and reporting.

## Statuses
[![License](https://img.shields.io/github/license/micro-elements/MicroElements.Metadata.svg)](https://raw.githubusercontent.com/micro-elements/MicroElements.Metadata/master/LICENSE)
[![NuGetVersion](https://img.shields.io/nuget/v/MicroElements.Metadata.svg)](https://www.nuget.org/packages/MicroElements.Metadata)
![NuGetDownloads](https://img.shields.io/nuget/dt/MicroElements.Metadata.svg)
[![MyGetVersion](https://img.shields.io/myget/micro-elements/v/MicroElements.Metadata.svg)](https://www.myget.org/feed/micro-elements/package/nuget/MicroElements.Metadata)

[![Travis](https://img.shields.io/travis/micro-elements/MicroElements.Metadata/master.svg?logo=travis)](https://travis-ci.org/micro-elements/MicroElements.Metadata)

[![Gitter](https://img.shields.io/gitter/room/micro-elements/MicroElements.Metadata.svg)](https://gitter.im/micro-elements/MicroElements.Metadata)

## Table of Contents

- [MicroElements.Metadata](#microelementsmetadata)
  - [Statuses](#statuses)
  - [Table of Contents](#table-of-contents)
  - [Installation](#installation)
  - [Build](#build)
  - [License](#license)
  - [Getting started](#getting-started)
  - [Building blocks](#building-blocks)
    - [Property](#property)
    - [PropertyContainer](#propertycontainer)
    - [MetadataProvider](#metadataprovider)
      - [Methods](#methods)
      - [UseCases](#usecases)
    - [Searching](#searching)
      - [SearchOptions](#searchoptions)
      - [Search methods](#search-methods)
        - [ISearchAlgorithm](#isearchalgorithm)
        - [SearchExtensions](#searchextensions)
    - [Calculation, Mapping TBD](#calculation-mapping-tbd)
    - [Dynamic TBD](#dynamic-tbd)
    - [Parsing TBD](#parsing-tbd)
    - [Reporting TBD](#reporting-tbd)
    - [Validation TBD](#validation-tbd)

## Installation

```
dotnet add package MicroElements.Metadata
```

## Build
Windows: Run `build.ps1`

Linux: Run `build.sh`

## License
This project is licensed under the MIT license. See the [LICENSE] file for more info.


[LICENSE]: https://raw.githubusercontent.com/micro-elements/MicroElements.Metadata/master/LICENSE

## Getting started

## Building blocks

### Property

Represents property for describing metadata model.
There are two main interfaces: untyped `IProperty` and generic `IProperty<T>`.

`IProperty` provides `Name` and `Type` and optional `Description` and `Alias`.

`IProperty<T>` extends `IProperty` with `DefaultValue`, `Calculator` and `Examples`

### PropertyContainer

PropertyContainer represents collection that contains properties and values for these properties. `IPropertyContainer` is an immutable collection of `IPropertyValue` 

`IPropertyContainer` provides `Properties`, `ParentSource` and `SearchOptions`

`IMutablePropertyContainer` extends `IPropertyContainer` with Add* and Set* methods.

### MetadataProvider

`IMetadataProvider` represents object that has metadata where Metadata is `IPropertyContainer`.

MetadataProvider allows to extend any object with additional properties. `IMetadataProvider` default implementation uses `MetadataGlobalCache.GetInstanceMetadata` that creates `MutablePropertyContainer`. If you want mutable metadata - implement Metadata with IMutablePropertyContainer.

Metadata default search mode: ByTypeAndName

#### Methods
 ``` csharp
    /// <summary>
    /// Gets metadata of required type.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Metadata provider.</param>
    /// <param name="metadataName">Optional metadata name.</param>
    /// <param name="defaultValue">Default value to return if not metadata found.</param>
    /// <returns>Metadata or default value if not found.</returns>
    [return: MaybeNull]
    public static TMetadata GetMetadata<TMetadata>(
        this IMetadataProvider metadataProvider,
        string? metadataName = null,
        [AllowNull] TMetadata defaultValue = default)

    /// <summary>
    /// Sets metadata for target object and returns the same metadataProvider for chaining.
    /// </summary>
    /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
    /// <typeparam name="TMetadata">Metadata type.</typeparam>
    /// <param name="metadataProvider">Target metadata provider.</param>
    /// <param name="metadataName">Metadata name.</param>
    /// <param name="data">Metadata to set.</param>
    /// <returns>The same metadataProvider.</returns>
    public static TMetadataProvider SetMetadata<TMetadataProvider, TMetadata>(
        this TMetadataProvider metadataProvider,
        string? metadataName,
        TMetadata data)
        where TMetadataProvider : IMetadataProvider

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
```
other methods can be found in `MetadataProviderExtensions`

#### UseCases

- [AttachedNameUntypedExample](/test/MicroElements.Metadata.Tests/examples/AttachedNameUntypedExample.cs)
- [NamedTypedMetadataUsage](/test/MicroElements.Metadata.Tests/examples/NamedTypedMetadataUsage.cs)

### Searching

Searching is one of the important concept of working with metadata. Most search methods accepts `SearchOptions`

#### SearchOptions

Property | DefaultValue | Description
---------|----------|---------
PropertyComparer | `ByTypeAndNameEqualityComparer` | Equality comparer for comparing properties.
SearchInParent | `true` | Do search in parent if no PropertyValue was found.
CalculateValue | `true` | Calculate value if value was not found.
UseDefaultValue | `true` | Use default value from property is property value was not found.
ReturnNotDefined | `true` | Return fake PropertyValue with <see cref="ValueSource.NotDefined"/> and Value set to default if no PropertyValue was found. Returns null if ReturnNotDefined is false.

#### Search methods

- Main search methods provided by `ISearchAlgorithm`
- Main search algorithm can be get or set in `Search.Algorithm`
- All search methods from `SearchExtensions` use `Search.Algorithm`
- `SearchOptions` can be composed by `Search` class

##### ISearchAlgorithm

```csharp
/// <summary>
/// Represents search algorithm.
/// </summary>
public interface ISearchAlgorithm
{
    /// <summary>
    /// Searches <see cref="IPropertyValue{T}"/> by <see cref="IProperty{T}"/> and <see cref="SearchOptions"/>.
    /// </summary>
    /// <param name="propertyContainer">Property container.</param>
    /// <param name="property">Property to search.</param>
    /// <param name="searchOptions">Search options.</param>
    /// <returns><see cref="IPropertyValue"/> or null.</returns>
    IPropertyValue? SearchPropertyValueUntyped(
        IPropertyContainer propertyContainer,
        IProperty property,
        SearchOptions? searchOptions = default);

    /// <summary>
    /// Gets <see cref="IPropertyValue{T}"/> by <see cref="IProperty{T}"/> and <see cref="SearchOptions"/>.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    /// <param name="propertyContainer">Property container.</param>
    /// <param name="property">Property to search.</param>
    /// <param name="searchOptions">Search options.</param>
    /// <returns><see cref="IPropertyValue"/> or null.</returns>
    IPropertyValue<T>? GetPropertyValue<T>(
        IPropertyContainer propertyContainer,
        IProperty<T> property,
        SearchOptions? searchOptions = null);
}
```

##### SearchExtensions

Method | Description
---------|----------|---------
 GetPropertyValue | Gets or calculates typed property and value for property using search conditions. It's a full search that uses all search options: `SearchOptions.SearchInParent`, `SearchOptions.CalculateValue`, <see cref="SearchOptions.UseDefaultValue"/>, <see cref="SearchOptions.ReturnNotDefined"/>.
 SearchPropertyValueUntyped | Searches property and value for untyped property using search conditions. Search does not use `SearchOptions.UseDefaultValue` and `SearchOptions.CalculateValue`. Search uses only `SearchOptions.SearchInParent` and `SearchOptions.ReturnNotDefined`.
 GetPropertyValueUntyped | Gets property and value for untyped property using search conditions. Uses simple untyped search `SearchPropertyValueUntyped` if CanUseSimpleUntypedSearch or `property` has type <see cref="Search.UntypedSearch"/>. Uses full `GetPropertyValue{T}` based on property.Type in other cases.




Typed and untyped search


### Calculation, Mapping TBD

- IPropertySet
- IPropertyContainerMapper

### Dynamic TBD

### Parsing TBD

### Reporting TBD
- ReportProvider
- Renderer
- IPropertyParser

### Validation TBD
