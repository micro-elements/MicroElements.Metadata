# 7.7.0
- PropertyContainerSchemaFilter: GenerateKnownSchemasAsRefs uses for properties with separate schema
- PropertyContainerSchemaFilter: Uses AspNetJsonSerializerOptions to reuse AspNet JsonSerializerOptions
- Added extension method SetMetadataFrom to copy metadata from source object
- Added ISchemaBuilder to allow create copies of schema components;
- Added ISchemaBuilder extension WithDescription that creates schema component copy with desired description
- Added ISchemaComponent to allow more precise control on schema building
- 7.7.1: IAllowedValues, INumericInterval, IProperties, IStringFormat, IStringMaxLength, IStringMinLength, IStringPattern, INullability become ISchemaComponent
- 7.7.1: PropertyContainerSchemaFilter all ISchemaComponent support
- 7.7.1: JsonTypeMapper returns types with format

# 7.6.0
- IAllowedValues: added IEqualityComparer
- NumericInterval extension methods returns the same type where possible
- Required validation rule extended with SearchOptions and assertValueIsNull argument, added IRequiredPropertyValidationRule marker interface
- ExtractValidateMap initial functionality
- MapToTuple extensions methods
- 7.6.4: SetAllowedValuesFromEnum for string schemas ignores case by default
- 7.6.4: JsonSchemaGenerator initial, added option: MetadataJsonSerializationOptions.UseJsonSchema

# 7.5.0
- Added Property<T> Clone method
- Added IPropertyContainer extensions: CloneAsMutable and CloneAsReadOnly
- ToPropertyContainerOfType: added arg returnTheSameIfNoNeedToConvert
- Swagger: added GenerateKnownSchemasAsRefs to PropertyContainerSchemaFilterOptions to generates known types as separate definitions

# 7.4.0
Schema serialization and utility release
- Added IMetadataSchemaProvider
- NewtonsoftJson: Added MetadataSchemaProviderConverter that allows to use common schema section in json document
- ConfigureJsonForPropertyContainers: added action to configure MetadataJsonSerializationOptions
- MetadataJsonSerializationOptions: added options: TypeMapper, UseSchemasRoot, SchemasRootName, WriteSchemaOnceForKnownTypes
- Removed IEnumeration interface from IPropertySet 
- Removed PropertyContainerWithMetadataTypesConverter
- Many other usability improvements
- 7.4.1: added default value to GetValue extension
- 7.4.3: PropertyContainerMapper moves to MicroElements.Metadata.Mapping and becomes more flexible

# 7.3.0
- Added PropertyContainerMapper with methods ToPropertyContainerOfType, ToPropertyContainer and ToObject
- PropertyContainerConverters (SystemTextJson and NewtonsoftJson) uses common method ToPropertyContainerOfType and supports typed PropertyContainers
- 7.3.2: PropertyComparers types ends with PropertyComparer (was EqualityComparer), ByTypeAndNamePropertyComparer can compare types ignoring Nullable wrapper
- 7.3.3: ByNameOrAliasPropertyComparer GetHashCode fix
- 7.3.3: MetadataSchema.AppendAbsentProperties fix
- 7.3.3: PropertyContainerMapper.ToObject nullable and enum support
- 7.3.4: Added HierarchicalContainer that merges two hierarchies
- 7.3.4: GetMetadata can get metadata from attached ISchema (see 'searchInSchema'' arg)
- 7.3.4: Added GetSchemaMetadata that always uses GetMetadata with searchInSchema: true
- 7.3.4: More metadata types that can be set to ISchema (not only IProperty): IAllowedValues, INumericInterval 
- 7.3.4: PropertyContainerMapper more customization

# 7.2.0
- PropertyContainerConverter becomes generic and supports collections
- DateTimeOffsetFormatter added to DefaultFormatProvider

# 7.1.0
- MicroElements.Metadata.AspNetCore: Swashbuckle updated to 6.1.3
- MicroElements.Metadata: Schema metadata more close to JsonSchema

# 7.0.0
- Big release. Main features: XmlParsing, Performance, ExcelBuilder performance, Schema featurea and validations.  See release notes for release candidates

# 7.0.0-rc.30
- OpenXml: CellReferences optimizations
- Some minor optimizations

# 7.0.0-rc.27
- IMetadataProvider changed API: added GetMetadata, SetMetadata
- Reflection optimization

# 7.0.0-rc.26
- Performance optimization for metadata search
- Performance optimization for excel building
- Complex value Formatting based on IValueFormatter
- IPropertyValueFactory, CachedPropertyValueFactory
- CachedStringParser and InternedStringParser
- XmlParser: IPropertyValueFactory added to IXmlParserSettings
- XmlParser: GC optimizations
- ExcelReportBuilder: Caches, GC optimizations, Performance optimizations
- ExcelReportBuilder: added parameter fillCellReferences to AddReportSheet
- ExcelReportBuilder: Cell.DataType omits if cell value is null
- IHasSchema cache
- Cached expression for PropertyValue creation

# 7.0.0-rc.19
- Validation for XmlParser
- ConfigureMetadata now supports thread safe ReadOnly metadata creation and configuration

# 7.0.0-rc.18
- Many useful PropertyContainerExtensions: Merge, MergeProperties, GetListItems, GetListItemsEnriched
- ISchema added to IXmlParserContext
- Added GetPropertiesNotFromSchema that works with IXmlParserContext

# 7.0.0-rc.17
- IValueParser moved to ParseResult instead of Option because Option doesnot support valid null values and error message
- Nullable enums supported

# 7.0.0-rc.16
- Added IXmlParserSettings reference to IXmlParserContext 
- Added SchemaCache to IXmlParserContext
- IHasSchema now can create new schema instances

# 7.0.0-rc.15
- Added GetPropertiesNotFromSchema recursive search in children schemas
- AllowedValues from enum
- IXmlParserContext extracted
- Parser cache added
- Parsers moved to Parsers namespace

# 7.0.0-rc.14
- Added Validation messages to IXmlParserSettings
- XmlParser API extended

# 7.0.0-rc.13
- Added XmlParser using ISchema
- Added Parsers for types and properties
- Added XmlParserSettings.ParserRules and PropertyComparer
- Added ability to attach validation rules to IProperty
- IStringMinLength and IStringMaxLength metadata and validations
- Validation default message format optionality

# 7.0.0-rc.12
- XmlParser usability improved

# 7.0.0-rc.11
- Performance optimizations in some hot pathes (property creation, searching)
- Added IStaticPropertySet and StaticPropertySet that has auto implemented GetProperties
- Added ISchema that extends IPropertySet (subject for future changes)
- Added IHasSchema metadata that can be attached to properties and property containers
- Added IMetadata interface
- Added XmlParser that parses xml to IPropertyContainer (new package MicroElements.Metadata.Xml)

# 7.0.0-rc.10
- Fix Render dates as serial for null value
- Fix SetFormat render for null value

# 7.0.0-rc.9
- MetadataSchemaCompact
- Json serializers unified
- Json formating arrays in one string

# 7.0.0-rc.4
- OpenXml: Excel Serial Date support for reports
- OpenXml: Big refactoring
- OpenXml: Styling extensions refactored and added many useful methods

# 7.0.0-rc.3
- Added FreezeMetadata and FreezeInstanceMetadata.
- IPropertyContainer.ParentSource makes nullable, and read only. SetParentPropertySource is removed
- Added GetHierarchy, Flatten methods for IPropertyContainer
- ToReadOnly by default flattens input container
- Optimization: Search uses internal property cache for some searches.

# 7.0.0-rc.2
- Added ThreadSafe ConcurrentMutablePropertyContainer, GetInstanceMetadata uses ConcurrentMutablePropertyContainer by default
- OpenXml: GetOrAddSharedString optimized

# 7.0.0-rc.1
- Added ReflectionDependencyInjectionExtensions to allow configure AspNetCore from netstandard package
- Added DependencyInjectionExtensions.AddMetadata - all in one registration (json serialization, swagger customization)
- Added sample of using it in AspNetCore application
- Added MetadataGlobalCache.AsMetadataProvider that converts any object to IMetadataProvider. It uses new MetadataProviderWrapper
- Property.Description simplified to string (was LocalizableString)
- MetadataProviderExtensions.CopyMetadataTo now can copy metadata from any object to any other object (not only IMetadataProvider)
- MutablePropertyContainer became ThreadSafe, old implementation became MutablePropertyContainerNoLock
- IPropertyContainer simplified: implements IReadOnlyCollection instead of IReadOnlyList, Properties also is IReadOnlyCollection
- Added interface IKnownPropertySet<> to describe what properties can be in a property container
- Added IAllowNull and IAllowedValues property metadata
- Added validation rule OnlyAllowedValues
- Added 'Or' validation rules
- PropertyContainerSchemaFilter can get IPropertySet from IKnownPropertySet<>
- PropertyContainerSchemaFilter uses optional IAllowNull and IAllowedValues to generate schema
- Added PropertyAddMode to WithValues extension method
- OpenXml: Added DocumentContext extensions: AddNumberingFormat, GetNumberingFormatIndex, GetNumberingFormatId

# 6.0.0
- Added new package MicroElements.Metadata.AspNetCore that contains swagger extensions and json serialization for IPropertyContainer
- Package MicroElements.Metadata.Json renamed to MicroElements.Metadata.SystemTextJson

# 5.3.0
- Json read fixed
- Default metadata search changed to ignore case
- Added extensions AsReadOnly, AsMutable, CopyMetadataTo for metadata providers

# 5.2.0
- Added Metadata Serialization
- Added package MicroElements.Metadata.Json
- Added package MicroElements.Metadata.NewtonsoftJson
- Added package MicroElements.Metadata.All
- MicroElements.Functional updated to version 1.10.0

# 5.1.0
- MicroElements.Functional updated to version 1.6.0
- TypeCheck methods replaced with methods from MicroElements.Functional

# 5.0.0
- Breaking: IProperyContainerMapper renamed to IModelMapper
- Breaking: Removed unused IPropertyMapper
- Added: IMetadataMapper that is combination of IModelMapper<T> and IPropertySet
- Fixed: ExcelExtensions GetRows and FillCellReferences for absent sheet

# 4.9.0
- PropertyValue.Create fix to correctly work with nullable structs

# 4.8.0
- Added RowContext.RowSource to access source property container for row
- ConfigureCell and ConfigureRow search propagation
- Removed obsolete Flatten method

# 4.7.0
- Change: Fills cell references in AddReportSheet

# 4.6.0
- Added Row customization with ExcelSheetMetadata.ConfigureRow
- Added ConfigureCell, ConfigureHeaderCell, ConficureColumn, ConfigureRow configuration extensions

# 4.5.0
- Added ToMutable, ToReadOnly extensions
- Added Excel styling extensions
- Change: GetValue returned to PropertyContainer and MutablePropertyContainer as protected method to use in derived classes
- Added more nullability asserts and markup

# 4.4.0
- SetFormat fixed: now for null input values it returns NullValue
- Excel: FillCellReferences now checks whether it should fill cell references

# 4.3.0
- SearchExtensions unified and documented
- Added Search.Algorithm with global access to optionate search
- Added some documentation to README

# 4.2.0
- IPropertyCalculator now accepts SearchOptions provided by user
- Map function now accepts configureSearch function instead of SearchOptions so it uses user provided SearchOptions
- SearchOptions argument removed from Nullify, DeNullify, UseDefaultForUndefined

# 4.1.0
- IPropertyRenderer.Configure now returns IPropertyRenderer to allow chaining
- New: Added AsUntyped extension for IPropertyRenderer<T> and ConfigureTyped for IPropertyRenderer
- Change: SetTargetName, SetSearchOptions, SetFormat, SetNullValue, SetNameFromAlias bacame extension methods for untyped IPropertyRenderer

# 4.0.0
- Nullability enabled, Most API annotated with nullability attributes
- Property immutability
- IPropertyContainer: added SearchOptions property
- IPropertyContainer: removed GetValue and GetValueUntyped
- Search became external to IPropertyContainer
- Added ISearchAlgorithm with base search methods, SearchExtensions uses ISearchAlgorithm
- IReportRenderer interface extracted from IReportProvider
- IReportProvider: Added GetReportRows to provide report rows
- Added IPropertyRenderer.Configure to allow configure untyped IPropertyRenderer
- [Excel] ExcelReportBuilder: old AddReportSheet accepts IReportRenderer and rows
- [Excel] ExcelReportBuilder: added AddReportSheet that accepts IReportProvider

# 3.9.0
- Added extension ValidateAndFilter to select validated items with callback for not valid items
- MicroElements.Functional updated to version 1.3.0
- ExcelExtensions.GetCellValue now tries to parse date types as strings than as double

# 3.8.0
- Added PropertySetAttribute to attach IPropertySet to IPropertyContainer in compile time
- Added PropertySetEvaluator to search IPropertySet in runtime by PropertySetAttribute
- MicroElements.Functional updated to version 1.2.0

# 3.7.0
- PropertyRenderer configurer to allow configure untyped IPropertyRenderer
- Added ToRenderers extension to convert IEnumerable<IProperty> to IEnumerable<IPropertyRenderer> with renderers customization
- Duplicate FormatAsTuple removed in favor of MicroElements.Functional version
- MicroElements.Functional updated to version 1.1.0

# 3.6.0
- Now IMetadataProvider.GetInstanceMetadata by default creates metadata with searchOptions ByNameAndTypeComparer instead Default
- Added AddRenderer and AddRenderers to ReportProvider

# 3.5.0
- MicroElements.Functional updated to version 1.0.0

# 3.4.0
- WithValue now returns the same container type as input
- WithCombinedConfigure takes configure action and combines action with new action
- [Excel] ConfigureCell now uses CellContext
- [Excel] Customuze actions renamed to Configure

# 3.3.0
- Property Map for PropertyValue fix
- Excel: FillCellReferences, MapRows

# 3.2.0
- Nullify and DeNullify mapper extensions for properties
- Added Map extension with extended map functor

# 3.1.0
- PropertyCalculator that also returns ValueSource
- Old Calculate func removed from IProperty<T>
- New Map method that covers old other MapXX methods, old methods removed
- DeNull method that allows to convert Nullable property to NotNullable
- Excel: fixed selected tab
- Excel: More wide columns in transposed mode

# 3.0.0
- Breaking Change: OpenXml reporting and parsing moved to MicroElements.Metadata.OpenXml

# 2.0.0
- Search
  - Property search redesigned and unified
  - Property search allows to know whether property exists, not exists or exists with undefined value
  - Search API became more consistent and customizable
  - PropertyContainer and MutablePropertyContainer can accept custom SearchOptions
  - Added predefined search modes: Default, ExistingOnly, ExistingOnlyWithParent
  - Added GetValueAsOption and SetValue extensions with functional Option support

- Validation
  - New validation rules: Exists, Required
  - IValidationRule now implements IMetadataProvider
  - Added generic IValidationRule<T> for holding typed property
  - Rich rule building with combine support
  - Validation message customization moved to ValidationMessageOptions and linked to IValidationRule as metadata
  - Validation customization for last rule in chain

- Other
  - ExcelReader: GetRowsAs supports factory function
  - ExcelReportBuilder support for SharedStrings
  - PropertyRenderer: supports SearchOptions

# 2.0.0-beta.8
 - Added GetValueAsOption and SetValue extensions with functional Option support
 - ExcelReader: GetRowsAs supports factory function
 - PropertyRenderer: supports SearchOptions

# 2.0.0-beta.7
# 2.0.0-beta.6
# 2.0.0-beta.5
- minor changes

# 2.0.0-beta.4
- ExcelReportBuilder support for SharedStrings
- Fixed broken copy and paste in generated excel

# 2.0.0-beta.3
- Validation customization for last rule in chain
- NotNull for Nullable struct
- Added predefined search modes: Default, ExistingOnly, ExistingOnlyWithParent
- Search clean up.

# 2.0.0-beta.2
- Property search redesigned and unified
- Search API became more consistent and customizable
- PropertyContainer and MutablePropertyContainer can accept custom SearchOptions

# 2.0.0-beta.1
- Property search allows to know whether property exists, not exists or exists with undefined value
- New validation rules: Exists, Required
- IValidationRule now implements IMetadataProvider
- Added generic IValidationRule<T> for holding typed property
- Rich rule building with combine support
- IConfigurableValidationRule removed
- Validation message customization moved to ValidationMessageOptions and linked to IValidationRule as metadata

# 1.3.0
- IParserProvider: convert Parsers property to GetParsers method to be more functional
- Added PropertyParser extensions to create property parsers in fluent parser providers
- Added CachedParserProvider and Cached extension

# 1.2.0
- ValidationResult became readonly struct
- Added CachedValidator and Cached extension

# 1.1.0
- ValidationResult became generic
- ValidationResult.IsValid moved to extension methods
- ToValidationResult and ToValidationResults extensions

# 1.0.0
- ExcelReader added
- Validation added

# 0.14.0
- Extensions for easy excel styling

# 0.13.0
- Document customization fixed (was not applyed)

# 0.12.0
- ExcelReportBuilder sheet customizization after adding data
- FreezeTopRow moved to extension methods

# 0.11.0
- ExcelReportBuilder customize for document, sheet, column, cell

# 0.9.4
- Property Map now can be used for classes and structs
- GetPropertyValue method added new argument: useDefaultValue. So it can return null now
- ExcelMetadata moved to property model for easy extend
- Excel generator: added Transpose for Sheets

# 0.9.3
- Unified Property methods with prefixes Set, SetUntyped, With

# 0.9.2
- EnumerableReportProvider
- Property Map methods to convert property to property of other type with func
- Property WithName extension method 

# 0.9.1
- IReportProvider is now IMetadataProvider
- ExcelMetadata hierarchy

# 0.9.0
- CsvReportBuilder
- ExcelReportBuilder
- ExcelMetadata for document, sheet, column, cell

# 0.8.1
- PropertyRenderer by default renders in Invariant culture.

# 0.8.0
- ValueSource converted to class instead of enum to extend on client side.

# 0.7.0
- SetValueUntyped and WithValueUntyped extensions instead of WithValue and SetValue to reduce type errors

# 0.6.1
- Added PropertySetEnumerable and PropertySetBase

# 0.6.0
- IProperty, IPropertySet implements IEnumerable
- LocalizableString implements IEnumerable

# 0.5.0
- IPropertySet, IPropertyContainerMapper

# 0.4.0
- Added CustomizeMetadata extension for IMetadataPrivider
- Added AddRange extension for IMutablePropertyContainer

# 0.3.3
- Now returns null for DynamicContainer is property was not found

# 0.3.2
- GetDynamicMemberNames implemented for DynamicContainer

# 0.3.1
- Default value for GetMetadata
- Options for AsDynamic and DynamicContainer

# 0.3.0
- Many methods for untyped work
- Builders for properties
- Dynamic wrapper

# 0.2.x
- Added PropertyMapper
- Added initial table impl
- IMetadataProvider changed
- PropertyList removed in favor of PropertyContainer

# 0.1.0
- Initial version

Full release notes can be found at: https://github.com/micro-elements/MicroElements.MetadataModel.git/blob/master/CHANGELOG.md
