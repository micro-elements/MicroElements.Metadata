# MicroElements.Metadata
Provides metadata model, schema, parsing, formatting, validation and reporting.

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
    - [Property](#property)
    - [PropertyValue](#propertyvalue)
    - [PropertyContainer](#propertycontainer)
      - [Sample](#sample)
    - [MetadataProvider](#metadataprovider)
      - [Methods](#methods)
      - [UseCases](#usecases)
    - [Searching](#searching)
      - [SearchOptions](#searchoptions)
      - [Search methods](#search-methods)
        - [ISearchAlgorithm](#isearchalgorithm)
        - [SearchExtensions](#searchextensions)
    - [EntityParseValidateReport example](#entityparsevalidatereport-example)
    - [Calculation, Mapping TBD](#calculation-mapping-tbd)
    - [Dynamic](#dynamic)
    - [Parsing TBD](#parsing-tbd)
    - [Validation TBD](#validation-tbd)
    - [Reporting TBD](#reporting-tbd)

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

### Property

Represents property for describing metadata model.
There are two main interfaces: untyped `IProperty` and generic `IProperty<T>`.

`IProperty` provides `Name` and `Type` and optional `Description` and `Alias`.

`IProperty<T>` extends `IProperty` with `DefaultValue`, `Calculator` and `Examples`

Source: [IProperty.cs](/src/MicroElements.Metadata/Metadata/IProperty.cs)

### PropertyValue

Represents property and its value.

Has untyped form: `IPropertyValue` and strong typed: `IPropertyValue<T>`.

Source: [IPropertyValue.cs](/src/MicroElements.Metadata/Metadata/IPropertyValue.cs)

### PropertyContainer

PropertyContainer represents collection that contains properties and values for these properties. `IPropertyContainer` is an immutable collection of `IPropertyValue` 

`IPropertyContainer` provides `Properties`, `ParentSource` and `SearchOptions`

`IMutablePropertyContainer` extends `IPropertyContainer` with Add* and Set* methods.

#### Sample

```csharp
  public class PropertyContainerUsage
  {
      public class EntityMeta
      {
          public static readonly IProperty<DateTime> CreatedAt = new Property<DateTime>("CreatedAt");
          public static readonly IProperty<string> Description = new Property<string>("Description");
      }

      [Fact]
      public void simple_set_and_get_value()
      {
          IPropertyContainer propertyContainer = new MutablePropertyContainer()
              .WithValue(EntityMeta.CreatedAt, DateTime.Today)
              .WithValue(EntityMeta.Description, "description");

          propertyContainer.GetValue(EntityMeta.CreatedAt).Should().Be(DateTime.Today);
          propertyContainer.GetValue(EntityMeta.Description).Should().Be("description");
      }

      [Fact]
      public void get_property_value()
      {
          IPropertyContainer propertyContainer = new MutablePropertyContainer()
              .WithValue(EntityMeta.CreatedAt, DateTime.Today)
              .WithValue(EntityMeta.Description, "description");

          IPropertyValue<string>? propertyValue = propertyContainer.GetPropertyValue(EntityMeta.Description);
          propertyValue.Should().NotBeNull();
          propertyValue.Property.Should().BeSameAs(EntityMeta.Description);
          propertyValue.Value.Should().Be("description");
          propertyValue.Source.Should().Be(ValueSource.Defined);
      }
  }
```

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
---------|----------
 GetPropertyValue | Gets or calculates typed property and value for property using search conditions. It's a full search that uses all search options: `SearchOptions.SearchInParent`, `SearchOptions.CalculateValue`, `SearchOptions.UseDefaultValue`, `SearchOptions.ReturnNotDefined`.
 SearchPropertyValueUntyped | Searches property and value for untyped property using search conditions. Search does not use `SearchOptions.UseDefaultValue` and `SearchOptions.CalculateValue`. Search uses only `SearchOptions.SearchInParent` and `SearchOptions.ReturnNotDefined`.
 GetPropertyValueUntyped | Gets property and value for untyped property using search conditions. Uses simple untyped search `SearchPropertyValueUntyped` if CanUseSimpleUntypedSearch or `property` has type `Search.UntypedSearch`. Uses full `GetPropertyValue{T}` based on property.Type in other cases.
 GetValue | Gets or calculates value for property.
 GetValueAsOption | Gets or calculates optional not null value.
 GetValueUntyped | Gets or calculates untyped value for property.
 GetValueByName | Gets or calculates value by name.


### EntityParseValidateReport example

Source: [EntityParseValidateReport.cs](/test/MicroElements.Metadata.Tests/examples/EntityParseValidateReport.cs)

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Parsing;
using MicroElements.Reporting.Excel;
using MicroElements.Validation;
using MicroElements.Validation.Rules;
using Xunit;

namespace MicroElements.Metadata.Tests.examples
{
    public class EntityParseValidateReport
    {
        public class Entity
        {
            public DateTime CreatedAt { get; }
            public string Name { get; }

            public Entity(DateTime createdAt, string name)
            {
                CreatedAt = createdAt;
                Name = name;
            }
        }

        public class EntityMeta : IPropertySet, IPropertyContainerMapper<Entity>
        {
            public static readonly EntityMeta Instance = new EntityMeta();

            public static readonly IProperty<DateTime> CreatedAt = new Property<DateTime>("CreatedAt");
            public static readonly IProperty<string> Name = new Property<string>("Name");

            /// <inheritdoc />
            public IEnumerable<IProperty> GetProperties()
            {
                yield return CreatedAt;
                yield return Name;
            }

            /// <inheritdoc />
            public IPropertyContainer ToContainer(Entity model)
            {
                return new MutablePropertyContainer()
                    .WithValue(CreatedAt, model.CreatedAt)
                    .WithValue(Name, model.Name);
            }

            /// <inheritdoc />
            public Entity ToModel(IPropertyContainer container)
            {
                return new Entity(
                    createdAt: container.GetValue(CreatedAt),
                    name: container.GetValue(Name));
            }
        }

        public class EntityParser : ParserProvider
        {
            protected Option<DateTime> ParseDate(string value) => Prelude.ParseDateTime(value);

            /// <inheritdoc />
            public EntityParser()
            {
                Source("CreatedAt", ParseDate).Target(EntityMeta.CreatedAt);
                Source("Name").Target(EntityMeta.Name);
            }
        }

        public class EntityValidator : IValidator
        {
            /// <inheritdoc />
            public IEnumerable<IValidationRule> GetRules()
            {
                yield return EntityMeta.CreatedAt.NotDefault();
                yield return EntityMeta.Name.Required();
            }
        }

        public class EntityReport : ReportProvider
        {
            public EntityReport(string reportName = "Entities")
                : base(reportName)
            {
                Add(EntityMeta.CreatedAt);
                Add(EntityMeta.Name);
            }
        }

        public Stream ReportToExcel(Entity[] entities)
        {
            var reportRows = entities.Select(entity => EntityMeta.Instance.ToContainer(entity));

            var excelStream = new MemoryStream();
            ExcelReportBuilder.Create(excelStream)
                .AddReportSheet(new EntityReport("Entities"), reportRows)
                .SaveAndClose();

            return excelStream;
        }

        public Entity[] ParseExcel(Stream stream)
        {
            var document = SpreadsheetDocument.Open(stream, false);

            var messages = new List<Message>();

            var entities = document
                .GetSheet("Entities")
                .GetRowsAs(new EntityParser(), list => new PropertyContainer(list))
                .ValidateAndFilter(new EntityValidator(), result => messages.AddRange(result.ValidationMessages))
                .Select(container => EntityMeta.Instance.ToModel(container))
                .ToArray();

            return entities;
        }

        [Fact]
        public void UseCase()
        {
            // Trim DateTime to milliseconds because default DateTime render trimmed to milliseconds
            DateTime NowTrimmed()
            {
                DateTime now = DateTime.Now;
                return now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMillisecond));
            }

            Entity[] entities = {
                new Entity(NowTrimmed(), "Name1"),
                new Entity(NowTrimmed(), "Name2"),
                new Entity(NowTrimmed(), "Name3"),
            };

            Stream excelStream = ReportToExcel(entities);

            Entity[] fromExcel = ParseExcel(excelStream);

            fromExcel.Should().HaveCount(3);
            fromExcel.Should().BeEquivalentTo(entities);
        }
    }
}

```

### Calculation, Mapping TBD

- IPropertySet
- IPropertyContainerMapper

### Dynamic

`IPropertyContainer` can be casted to `dynamic` object with `AsDynamic` extension.

```csharp
  [Fact]
  public void DynamicContainer()
  {
      var propertyContainer = new MutablePropertyContainer();
      propertyContainer.SetValue("PropertyA", "ValueA");
      propertyContainer.SetValue(new Property<int>("PropertyB"), 42);

      dynamic dynamicContainer = propertyContainer.AsDynamic();
      object valueA = dynamicContainer.PropertyA;
      valueA.Should().Be("ValueA");

      object valueB = dynamicContainer.PropertyB;
      valueB.Should().Be(42);

      object notFoundProperty = dynamicContainer.NotFoundProperty;
      notFoundProperty.Should().BeNull();
  }
```

### Parsing TBD

See: [EntityParseValidateReport example](#entityparsevalidatereport-example)

### Validation TBD

See: [EntityParseValidateReport example](#entityparsevalidatereport-example)

### Reporting TBD

See: [EntityParseValidateReport example](#entityparsevalidatereport-example)

- ReportProvider
- Renderer
- IPropertyParser
