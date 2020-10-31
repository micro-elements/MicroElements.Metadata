# MicroElements.Metadata
Provides metadata model, parsing and reporting.

## Statuses
[![License](https://img.shields.io/github/license/micro-elements/MicroElements.Metadata.svg)](https://raw.githubusercontent.com/micro-elements/MicroElements.Metadata/master/LICENSE)
[![NuGetVersion](https://img.shields.io/nuget/v/MicroElements.Metadata.svg)](https://www.nuget.org/packages/MicroElements.Metadata)
![NuGetDownloads](https://img.shields.io/nuget/dt/MicroElements.Metadata.svg)
[![MyGetVersion](https://img.shields.io/myget/micro-elements/v/MicroElements.Metadata.svg)](https://www.myget.org/feed/micro-elements/package/nuget/MicroElements.Metadata)

[![Travis](https://img.shields.io/travis/micro-elements/MicroElements.Metadata/master.svg?logo=travis)](https://travis-ci.org/micro-elements/MicroElements.Metadata)

[![Gitter](https://img.shields.io/gitter/room/micro-elements/MicroElements.Metadata.svg)](https://gitter.im/micro-elements/MicroElements.Metadata)

## Installation

### Package Reference:

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

#### Methods:
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


### Searching TBD

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
