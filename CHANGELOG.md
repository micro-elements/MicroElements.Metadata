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
- IPropertyContainer: added SearachOptions property
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
